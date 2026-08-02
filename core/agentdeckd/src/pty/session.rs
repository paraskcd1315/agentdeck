use tokio::sync::mpsc::{UnboundedReceiver, unbounded_channel};

use super::child::Child;
use super::command::PtyCommand;
use super::conpty::ConPty;
use super::error::PtyError;
use super::handle::PtyHandle;
use super::pipe::create_pipe;
use super::pump::pump;
use super::session_id::PtyId;
use super::size::PtySize;
use super::spawn::spawn_attached;

pub struct PtySession {
    id: PtyId,
    pty: ConPty,
    writer: PtyHandle,
    child: Child,
}

impl PtySession {
    pub fn spawn(
        command: &PtyCommand,
        size: PtySize,
    ) -> Result<(Self, UnboundedReceiver<Vec<u8>>), PtyError> {
        let (input_read, input_write) = create_pipe()?;
        let (output_read, output_write) = create_pipe()?;

        let pty = ConPty::create(size, input_read, output_write)?;

        let child = spawn_attached(command, &pty)?;

        let (sender, receiver) = unbounded_channel();
        std::thread::spawn(move || pump(output_read, sender));

        let session = Self {
            id: PtyId::allocate(),
            pty,
            writer: input_write,
            child,
        };

        Ok((session, receiver))
    }

    pub fn id(&self) -> PtyId {
        self.id
    }

    pub fn write(&self, bytes: &[u8]) -> Result<(), PtyError> {
        self.writer.write_all(bytes)
    }

    pub fn resize(&self, size: PtySize) -> Result<(), PtyError> {
        self.pty.resize(size)
    }

    pub fn is_running(&self) -> bool {
        self.child.is_running()
    }

    pub fn exit_code(&self) -> Result<Option<u32>, PtyError> {
        self.child.exit_code()
    }

    pub fn kill(&self) -> Result<(), PtyError> {
        self.child.kill()
    }
}
