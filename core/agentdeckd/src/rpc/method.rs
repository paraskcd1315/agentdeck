use crate::constants::{
    METHOD_HOOK_EVENT, METHOD_PANEL_LIST, METHOD_PANEL_READ, METHOD_PTY_KILL, METHOD_PTY_RESIZE,
    METHOD_PTY_SPAWN, METHOD_PTY_WRITE, METHOD_TERMINAL_SNAPSHOT, METHOD_TERMINAL_TEXT,
    METHOD_WORKSPACE_OPEN,
};

#[derive(Clone, Debug, PartialEq, Eq)]
pub enum Method {
    PtySpawn,
    PtyWrite,
    PtyResize,
    PtyKill,
    HookEvent,
    PanelList,
    PanelRead,
    WorkspaceOpen,
    TerminalText,
    TerminalSnapshot,
    Unknown,
}

impl Method {
    pub fn parse(name: &str) -> Self {
        match name {
            METHOD_PTY_SPAWN => Self::PtySpawn,
            METHOD_PTY_WRITE => Self::PtyWrite,
            METHOD_PTY_RESIZE => Self::PtyResize,
            METHOD_PTY_KILL => Self::PtyKill,
            METHOD_HOOK_EVENT => Self::HookEvent,
            METHOD_PANEL_LIST => Self::PanelList,
            METHOD_PANEL_READ => Self::PanelRead,
            METHOD_WORKSPACE_OPEN => Self::WorkspaceOpen,
            METHOD_TERMINAL_TEXT => Self::TerminalText,
            METHOD_TERMINAL_SNAPSHOT => Self::TerminalSnapshot,
            _ => Self::Unknown,
        }
    }
}
