use serde::{Deserialize, Serialize};

use super::status_entry::StatusEntry;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct RepositoryStatus {
    pub root: String,
    pub branch: Option<String>,
    pub head: Option<String>,
    pub entries: Vec<StatusEntry>,
}
