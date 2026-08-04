use windows::Win32::System::Console::{
    ClosePseudoConsole, CreatePseudoConsole, HPCON, ResizePseudoConsole,
};

use super::error::PtyError;
use super::handle::PtyHandle;
use super::size::PtySize;

pub struct ConPty {
    handle: HPCON,
    _input: PtyHandle,
    _output: PtyHandle,
}

unsafe impl Send for ConPty {}
unsafe impl Sync for ConPty {}

impl ConPty {
    pub(crate) fn create(
        size: PtySize,
        input: PtyHandle,
        output: PtyHandle,
    ) -> Result<Self, PtyError> {
        let handle = unsafe { CreatePseudoConsole(size.to_coord(), input.raw(), output.raw(), 0) }
            .map_err(PtyError::CreatePseudoConsole)?;

        Ok(Self {
            handle,
            _input: input,
            _output: output,
        })
    }

    pub fn resize(&self, size: PtySize) -> Result<(), PtyError> {
        unsafe { ResizePseudoConsole(self.handle, size.to_coord()) }
            .map_err(PtyError::ResizePseudoConsole)
    }

    pub(crate) fn raw(&self) -> HPCON {
        self.handle
    }
}

impl Drop for ConPty {
    fn drop(&mut self) {
        unsafe { ClosePseudoConsole(self.handle) };
    }
}
