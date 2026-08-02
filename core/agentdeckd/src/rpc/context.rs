use std::sync::{Arc, Mutex};

use base64::Engine;
use base64::engine::general_purpose::STANDARD as BASE64;
use serde_json::json;
use tokio::sync::broadcast;

use crate::constants::{BROADCAST_CAPACITY, NOTIFY_PTY_DATA, NOTIFY_PTY_EXIT};
use crate::pty::command::PtyCommand;
use crate::pty::registry::SessionRegistry;
use crate::pty::session::PtySession;
use crate::pty::session_id::PtyId;
use crate::pty::size::PtySize;

use super::error::RpcError;
use super::notification::Notification;

#[derive(Clone)]
pub struct ServerContext {
    registry: Arc<Mutex<SessionRegistry>>,
    notifications: broadcast::Sender<Notification>,
}

impl ServerContext {
    pub fn new() -> Self {
        let (notifications, _) = broadcast::channel(BROADCAST_CAPACITY);
        Self {
            registry: Arc::new(Mutex::new(SessionRegistry::default())),
            notifications,
        }
    }

    pub fn subscribe(&self) -> broadcast::Receiver<Notification> {
        self.notifications.subscribe()
    }

    pub fn notify(&self, notification: Notification) {
        let _ = self.notifications.send(notification);
    }

    pub fn spawn_pty(&self, command: &PtyCommand, size: PtySize) -> Result<PtyId, RpcError> {
        let (session, mut output) =
            PtySession::spawn(command, size).map_err(|error| RpcError::internal(error.to_string()))?;
        let id = session.id();

        self.registry
            .lock()
            .map_err(|_| RpcError::internal("session registry is poisoned"))?
            .insert(session);

        let context = self.clone();
        tokio::spawn(async move {
            while let Some(chunk) = output.recv().await {
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

    fn exit_code(&self, id: PtyId) -> Option<u32> {
        let registry = self.registry.lock().ok()?;
        registry.get(id)?.exit_code().ok().flatten()
    }

    fn with_session<T>(
        &self,
        id: PtyId,
        action: impl FnOnce(&PtySession) -> Result<T, RpcError>,
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

impl Default for ServerContext {
    fn default() -> Self {
        Self::new()
    }
}
