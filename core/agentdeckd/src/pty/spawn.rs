use std::ffi::c_void;

use windows::Win32::Foundation::INVALID_HANDLE_VALUE;
use windows::Win32::System::Console::HPCON;
use windows::Win32::System::Threading::{
    CreateProcessW, DeleteProcThreadAttributeList, EXTENDED_STARTUPINFO_PRESENT,
    InitializeProcThreadAttributeList, LPPROC_THREAD_ATTRIBUTE_LIST, PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
    PROCESS_INFORMATION, STARTF_USESTDHANDLES, STARTUPINFOEXW, UpdateProcThreadAttribute,
};
use windows::core::{PCWSTR, PWSTR};

use crate::constants::PSEUDOCONSOLE_ATTRIBUTE_COUNT;

use super::child::Child;
use super::command::PtyCommand;
use super::conpty::ConPty;
use super::error::PtyError;
use super::handle::PtyHandle;
use super::wide::{to_wide_null, to_wide_null_str};

pub(crate) fn spawn_attached(command: &PtyCommand, pty: &ConPty) -> Result<Child, PtyError> {
    let mut attribute_bytes = attribute_list_size();
    let mut attribute_buffer = vec![0u8; attribute_bytes];
    let attribute_list =
        LPPROC_THREAD_ATTRIBUTE_LIST(attribute_buffer.as_mut_ptr() as *mut c_void);

    unsafe {
        InitializeProcThreadAttributeList(
            Some(attribute_list),
            PSEUDOCONSOLE_ATTRIBUTE_COUNT,
            None,
            &mut attribute_bytes,
        )
    }
    .map_err(PtyError::InitialiseAttributeList)?;

    let result = attach_and_create(command, pty, attribute_list);

    unsafe { DeleteProcThreadAttributeList(attribute_list) };

    result
}

fn attribute_list_size() -> usize {
    let mut bytes = 0usize;
    unsafe {
        let _ = InitializeProcThreadAttributeList(
            None,
            PSEUDOCONSOLE_ATTRIBUTE_COUNT,
            None,
            &mut bytes,
        );
    }
    bytes
}

fn attach_and_create(
    command: &PtyCommand,
    pty: &ConPty,
    attribute_list: LPPROC_THREAD_ATTRIBUTE_LIST,
) -> Result<Child, PtyError> {
    unsafe {
        UpdateProcThreadAttribute(
            attribute_list,
            0,
            PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE as usize,
            Some(pty.raw().0 as *const c_void),
            size_of::<HPCON>(),
            None,
            None,
        )
    }
    .map_err(PtyError::UpdateAttributeList)?;

    let mut startup = STARTUPINFOEXW::default();
    startup.StartupInfo.cb = size_of::<STARTUPINFOEXW>() as u32;
    startup.StartupInfo.dwFlags = STARTF_USESTDHANDLES;
    startup.StartupInfo.hStdInput = INVALID_HANDLE_VALUE;
    startup.StartupInfo.hStdOutput = INVALID_HANDLE_VALUE;
    startup.StartupInfo.hStdError = INVALID_HANDLE_VALUE;
    startup.lpAttributeList = attribute_list;

    let mut command_line = to_wide_null_str(&command.command_line());
    let working_directory = command.cwd.as_ref().map(|path| to_wide_null(path.as_os_str()));
    let working_directory = working_directory
        .as_ref()
        .map_or(PCWSTR::null(), |value| PCWSTR(value.as_ptr()));
    let mut information = PROCESS_INFORMATION::default();

    unsafe {
        CreateProcessW(
            None,
            Some(PWSTR(command_line.as_mut_ptr())),
            None,
            None,
            false,
            EXTENDED_STARTUPINFO_PRESENT,
            None,
            working_directory,
            &startup.StartupInfo,
            &mut information,
        )
    }
    .map_err(|source| PtyError::Spawn {
        program: command.program.clone(),
        source,
    })?;

    let process = PtyHandle::adopt(information.hProcess)?;
    drop(PtyHandle::adopt(information.hThread)?);

    Ok(Child::new(process))
}
