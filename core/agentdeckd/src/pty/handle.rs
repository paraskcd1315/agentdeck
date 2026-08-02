use windows::Win32::Foundation::{CloseHandle, HANDLE};
use windows::Win32::Storage::FileSystem::{ReadFile, WriteFile};

use super::error::PtyError;

pub struct PtyHandle(HANDLE);

unsafe impl Send for PtyHandle {}
unsafe impl Sync for PtyHandle {}

impl PtyHandle {
    pub(crate) fn adopt(raw: HANDLE) -> Result<Self, PtyError> {
        if raw.is_invalid() {
            return Err(PtyError::InvalidHandle);
        }
        Ok(Self(raw))
    }

    pub(crate) fn raw(&self) -> HANDLE {
        self.0
    }

    pub fn read(&self, buffer: &mut [u8]) -> Result<usize, PtyError> {
        let mut count = 0u32;
        unsafe { ReadFile(self.0, Some(buffer), Some(&mut count), None) }.map_err(PtyError::Read)?;
        Ok(count as usize)
    }

    pub fn write_all(&self, bytes: &[u8]) -> Result<(), PtyError> {
        let mut remaining = bytes;
        while !remaining.is_empty() {
            let mut count = 0u32;
            unsafe { WriteFile(self.0, Some(remaining), Some(&mut count), None) }
                .map_err(PtyError::Write)?;
            if count == 0 {
                return Err(PtyError::WriteZero);
            }
            remaining = &remaining[count as usize..];
        }
        Ok(())
    }
}

impl Drop for PtyHandle {
    fn drop(&mut self) {
        unsafe {
            let _ = CloseHandle(self.0);
        }
    }
}
