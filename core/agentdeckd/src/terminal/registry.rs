use std::collections::HashMap;

use crate::pty::session_id::PtyId;

use super::session::TerminalSession;

#[derive(Default)]
pub struct SessionRegistry {
    sessions: HashMap<PtyId, TerminalSession>,
}

impl SessionRegistry {
    pub fn insert(&mut self, session: TerminalSession) {
        self.sessions.insert(session.id(), session);
    }

    pub fn get(&self, id: PtyId) -> Option<&TerminalSession> {
        self.sessions.get(&id)
    }

    pub fn remove(&mut self, id: PtyId) -> Option<TerminalSession> {
        self.sessions.remove(&id)
    }
}
