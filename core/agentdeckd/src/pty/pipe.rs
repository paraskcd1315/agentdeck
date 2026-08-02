use windows::Win32::Foundation::HANDLE;
use windows::Win32::System::Pipes::CreatePipe;

use super::error::PtyError;
use super::handle::PtyHandle;

pub(crate) fn create_pipe() -> Result<(PtyHandle, PtyHandle), PtyError> {
    let mut read = HANDLE::default();
    let mut write = HANDLE::default();

    unsafe { CreatePipe(&mut read, &mut write, None, 0) }.map_err(PtyError::CreatePipe)?;

    Ok((PtyHandle::adopt(read)?, PtyHandle::adopt(write)?))
}
