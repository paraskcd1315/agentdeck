use serde::{Deserialize, Serialize};

use super::status_kind::StatusKind;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct StatusEntry {
    pub path: String,
    pub kind: StatusKind,
}

impl StatusEntry {
    pub fn new(path: impl Into<String>, kind: StatusKind) -> Self {
        Self {
            path: path.into(),
            kind,
        }
    }
}
