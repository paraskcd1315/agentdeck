use windows::Win32::Foundation::WAIT_OBJECT_0;
use windows::Win32::System::Threading::{GetExitCodeProcess, TerminateProcess, WaitForSingleObject};

use crate::constants::PROCESS_LIVENESS_POLL_MILLIS;

use super::error::PtyError;
use super::handle::PtyHandle;

pub struct Child {
    process: PtyHandle,
}

impl Child {
    pub(crate) fn new(process: PtyHandle) -> Self {
        Self { process }
    }

    pub fn is_running(&self) -> bool {
        let status = unsafe { WaitForSingleObject(self.process.raw(), PROCESS_LIVENESS_POLL_MILLIS) };
        status != WAIT_OBJECT_0
    }

    pub fn exit_code(&self) -> Result<Option<u32>, PtyError> {
        if self.is_running() {
            return Ok(None);
        }
        let mut code = 0u32;
        unsafe { GetExitCodeProcess(self.process.raw(), &mut code) }.map_err(PtyError::ExitCode)?;
        Ok(Some(code))
    }

    pub fn kill(&self) -> Result<(), PtyError> {
        if !self.is_running() {
            return Ok(());
        }
        unsafe { TerminateProcess(self.process.raw(), 1) }.map_err(PtyError::Terminate)
    }
}
