use crate::constants::{
    METHOD_HOOK_EVENT, METHOD_PTY_KILL, METHOD_PTY_RESIZE, METHOD_PTY_SPAWN, METHOD_PTY_WRITE,
};

#[derive(Clone, Debug, PartialEq, Eq)]
pub enum Method {
    PtySpawn,
    PtyWrite,
    PtyResize,
    PtyKill,
    HookEvent,
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
            _ => Self::Unknown,
        }
    }
}
