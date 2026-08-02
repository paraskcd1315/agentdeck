pub const DEFAULT_SHELL: &str = "powershell.exe";
pub const ARGUMENT_SEPARATOR: &str = "--";
pub const DEFAULT_PTY_COLS: u16 = 120;
pub const DEFAULT_PTY_ROWS: u16 = 30;
pub const PTY_READ_BUFFER_BYTES: usize = 16 * 1024;
pub const PTY_SMOKE_SETTLE_MILLIS: u64 = 800;
pub const PTY_SMOKE_DRAIN_MILLIS: u64 = 4000;
pub const PTY_SMOKE_POLL_MILLIS: u64 = 100;
pub const PROCESS_LIVENESS_POLL_MILLIS: u32 = 0;
pub const PSEUDOCONSOLE_ATTRIBUTE_COUNT: u32 = 1;
pub const TERMINAL_SCROLLBACK_LINES: usize = 10_000;

pub const PIPE_NAME: &str = r"\\.\pipe\agentdeck";
pub const JSONRPC_VERSION: &str = "2.0";
pub const BROADCAST_CAPACITY: usize = 1024;

pub const METHOD_PTY_SPAWN: &str = "pty.spawn";
pub const METHOD_PTY_WRITE: &str = "pty.write";
pub const METHOD_PTY_RESIZE: &str = "pty.resize";
pub const METHOD_PTY_KILL: &str = "pty.kill";
pub const METHOD_HOOK_EVENT: &str = "hook.event";
pub const METHOD_PANEL_LIST: &str = "panel.list";
pub const METHOD_PANEL_READ: &str = "panel.read";
pub const METHOD_WORKSPACE_OPEN: &str = "workspace.open";
pub const METHOD_TERMINAL_TEXT: &str = "terminal.text";
pub const METHOD_TERMINAL_SNAPSHOT: &str = "terminal.snapshot";

pub const STATE_DIRECTORY: &str = ".agentdeck";
pub const WORKSPACES_DIRECTORY: &str = "workspaces";
pub const PANELS_DIRECTORY: &str = "panels";
pub const PANEL_FILE_EXTENSION: &str = "json";
pub const PANEL_SCHEMA_V1: &str = "panel/v1";
pub const WORKSPACE_ID_HEX_LENGTH: usize = 16;

pub const NOTIFY_PTY_DATA: &str = "pty.data";
pub const NOTIFY_PTY_EXIT: &str = "pty.exit";
pub const NOTIFY_HOOK_EVENT: &str = "hook.event";
pub const NOTIFY_PANEL_CHANGED: &str = "panel.changed";
pub const NOTIFY_TERMINAL_DAMAGE: &str = "terminal.damage";

pub const ERROR_PARSE: i32 = -32700;
pub const ERROR_INVALID_REQUEST: i32 = -32600;
pub const ERROR_METHOD_NOT_FOUND: i32 = -32601;
pub const ERROR_INVALID_PARAMS: i32 = -32602;
pub const ERROR_INTERNAL: i32 = -32603;
