use thiserror::Error;

#[derive(Debug, Error)]
pub enum PtyError {
    #[error("the operating system returned an invalid handle")]
    InvalidHandle,
    #[error("failed to create a pipe pair: {0}")]
    CreatePipe(#[source] windows::core::Error),
    #[error("failed to create the pseudoconsole: {0}")]
    CreatePseudoConsole(#[source] windows::core::Error),
    #[error("failed to resize the pseudoconsole: {0}")]
    ResizePseudoConsole(#[source] windows::core::Error),
    #[error("failed to initialise the process attribute list: {0}")]
    InitialiseAttributeList(#[source] windows::core::Error),
    #[error("failed to attach the pseudoconsole to the attribute list: {0}")]
    UpdateAttributeList(#[source] windows::core::Error),
    #[error("failed to spawn {program}: {source}")]
    Spawn {
        program: String,
        #[source]
        source: windows::core::Error,
    },
    #[error("failed to read from the pseudoconsole: {0}")]
    Read(#[source] windows::core::Error),
    #[error("failed to write to the pseudoconsole: {0}")]
    Write(#[source] windows::core::Error),
    #[error("the pseudoconsole accepted zero bytes")]
    WriteZero,
    #[error("failed to terminate the child process: {0}")]
    Terminate(#[source] windows::core::Error),
    #[error("failed to read the child process exit code: {0}")]
    ExitCode(#[source] windows::core::Error),
}
