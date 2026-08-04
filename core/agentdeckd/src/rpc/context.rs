use std::sync::{Arc, Mutex};

use base64::Engine;
use base64::engine::general_purpose::STANDARD as BASE64;
use serde_json::json;
use tokio::sync::broadcast;

use crate::config::paths::StateDirs;
use crate::constants::{
    BROADCAST_CAPACITY, NOTIFY_PTY_DATA, NOTIFY_PTY_EXIT, NOTIFY_TERMINAL_DAMAGE,
};
use crate::pty::command::PtyCommand;
use crate::pty::session_id::PtyId;
use crate::pty::size::PtySize;
use crate::terminal::registry::SessionRegistry;
use crate::terminal::session::TerminalSession;
use crate::terminal::wire::snapshot::WireSnapshot;

use super::error::RpcError;
use super::notification::Notification;

#[derive(Clone)]
pub struct ServerContext {
    registry: Arc<Mutex<SessionRegistry>>,
    notifications: broadcast::Sender<Notification>,
    dirs: StateDirs,
}

impl ServerContext {
    pub fn new(dirs: StateDirs) -> Self {
        let (notifications, _) = broadcast::channel(BROADCAST_CAPACITY);
        Self {
            registry: Arc::new(Mutex::new(SessionRegistry::default())),
            notifications,
            dirs,
        }
    }

    pub fn dirs(&self) -> &StateDirs {
        &self.dirs
    }

    pub fn subscribe(&self) -> broadcast::Receiver<Notification> {
        self.notifications.subscribe()
    }

    pub fn notify(&self, notification: Notification) {
        let _ = self.notifications.send(notification);
    }

    pub fn spawn_pty(&self, command: &PtyCommand, size: PtySize) -> Result<PtyId, RpcError> {
        let (session, mut output, _events) = TerminalSession::spawn(command, size)
            .map_err(|error| RpcError::internal(error.to_string()))?;
        let id = session.id();
        let emulator = session.emulator();

        self.registry
            .lock()
            .map_err(|_| RpcError::internal("session registry is poisoned"))?
            .insert(session);

        let context = self.clone();
        tokio::spawn(async move {
            while let Some(chunk) = output.recv().await {
                let damage = match emulator.lock() {
                    Ok(mut emulator) => {
                        emulator.advance(&chunk);
                        emulator.damage(id)
                    }
                    Err(_) => None,
                };

                if let Some(snapshot) = damage
                    && let Ok(params) = serde_json::to_value(snapshot)
                {
                    context.notify(Notification::new(NOTIFY_TERMINAL_DAMAGE, params));
                }

                context.notify(Notification::new(
                    NOTIFY_PTY_DATA,
                    json!({ "ptyId": id.value(), "dataB64": BASE64.encode(&chunk) }),
                ));
            }
            let code = context.exit_code(id);
            context.notify(Notification::new(
                NOTIFY_PTY_EXIT,
                json!({ "ptyId": id.value(), "code": code }),
            ));
        });

        Ok(id)
    }

    pub fn write_pty(&self, id: PtyId, bytes: &[u8]) -> Result<(), RpcError> {
        let emulator = self.with_session(id, |session| Ok(session.emulator()))?;
        emulator
            .lock()
            .map_err(|_| RpcError::internal("the emulator lock is poisoned"))?
            .snap_to_bottom();

        self.with_session(id, |session| {
            session
                .write(bytes)
                .map_err(|error| RpcError::internal(error.to_string()))
        })
    }

    pub fn resize_pty(&self, id: PtyId, size: PtySize) -> Result<(), RpcError> {
        self.with_session(id, |session| {
            session
                .resize(size)
                .map_err(|error| RpcError::internal(error.to_string()))
        })
    }

    pub fn kill_pty(&self, id: PtyId) -> Result<(), RpcError> {
        self.with_session(id, |session| {
            session
                .kill()
                .map_err(|error| RpcError::internal(error.to_string()))
        })
    }

    pub fn snapshot(&self, id: PtyId) -> Result<WireSnapshot, RpcError> {
        let emulator = self.with_session(id, |session| Ok(session.emulator()))?;
        let emulator = emulator
            .lock()
            .map_err(|_| RpcError::internal("the emulator lock is poisoned"))?;
        Ok(emulator.snapshot(id))
    }

    pub fn scroll(&self, id: PtyId, delta: i32) -> Result<WireSnapshot, RpcError> {
        let emulator = self.with_session(id, |session| Ok(session.emulator()))?;
        let mut emulator = emulator
            .lock()
            .map_err(|_| RpcError::internal("the emulator lock is poisoned"))?;

        emulator.scroll(delta);
        Ok(emulator.snapshot(id))
    }

    pub fn visible_lines(&self, id: PtyId) -> Result<Vec<String>, RpcError> {
        let emulator = self.with_session(id, |session| Ok(session.emulator()))?;
        let emulator = emulator
            .lock()
            .map_err(|_| RpcError::internal("the emulator lock is poisoned"))?;
        Ok(emulator.visible_lines())
    }

    fn exit_code(&self, id: PtyId) -> Option<u32> {
        let registry = self.registry.lock().ok()?;
        registry.get(id)?.exit_code().ok().flatten()
    }

    fn with_session<T>(
        &self,
        id: PtyId,
        action: impl FnOnce(&TerminalSession) -> Result<T, RpcError>,
    ) -> Result<T, RpcError> {
        let registry = self
            .registry
            .lock()
            .map_err(|_| RpcError::internal("session registry is poisoned"))?;
        let session = registry
            .get(id)
            .ok_or_else(|| RpcError::invalid_params(format!("unknown ptyId: {}", id.value())))?;
        action(session)
    }
}

