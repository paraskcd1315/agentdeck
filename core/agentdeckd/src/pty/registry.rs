use std::collections::HashMap;

use super::session::PtySession;
use super::session_id::PtyId;

#[derive(Default)]
pub struct SessionRegistry {
    sessions: HashMap<PtyId, PtySession>,
}

impl SessionRegistry {
    pub fn insert(&mut self, session: PtySession) {
        self.sessions.insert(session.id(), session);
    }

    pub fn get(&self, id: PtyId) -> Option<&PtySession> {
        self.sessions.get(&id)
    }

    pub fn remove(&mut self, id: PtyId) -> Option<PtySession> {
        self.sessions.remove(&id)
    }
}
