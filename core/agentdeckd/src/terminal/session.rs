use std::sync::{Arc, Mutex};

use alacritty_terminal::event::Event;
use tokio::sync::mpsc::{UnboundedReceiver, unbounded_channel};

use crate::pty::command::PtyCommand;
use crate::pty::error::PtyError;
use crate::pty::session::PtySession;
use crate::pty::session_id::PtyId;
use crate::pty::size::PtySize;

use super::emulator::Emulator;
use super::event_proxy::EventProxy;

pub struct TerminalSession {
    pty: PtySession,
    emulator: Arc<Mutex<Emulator>>,
}

impl TerminalSession {
    pub fn spawn(
        command: &PtyCommand,
        size: PtySize,
    ) -> Result<(Self, UnboundedReceiver<Vec<u8>>, UnboundedReceiver<Event>), PtyError> {
        let (pty, output) = PtySession::spawn(command, size)?;
        let (events, event_receiver) = unbounded_channel();
        let emulator = Emulator::new(size, EventProxy::new(events));

        let session = Self {
            pty,
            emulator: Arc::new(Mutex::new(emulator)),
        };

        Ok((session, output, event_receiver))
    }

    pub fn id(&self) -> PtyId {
        self.pty.id()
    }

    pub fn emulator(&self) -> Arc<Mutex<Emulator>> {
        Arc::clone(&self.emulator)
    }

    pub fn write(&self, bytes: &[u8]) -> Result<(), PtyError> {
        self.pty.write(bytes)
    }

    pub fn resize(&self, size: PtySize) -> Result<(), PtyError> {
        self.pty.resize(size)?;

        if let Ok(mut emulator) = self.emulator.lock() {
            emulator.resize(size);
        }

        Ok(())
    }

    pub fn exit_code(&self) -> Result<Option<u32>, PtyError> {
        self.pty.exit_code()
    }

    pub fn kill(&self) -> Result<(), PtyError> {
        self.pty.kill()
    }
}
