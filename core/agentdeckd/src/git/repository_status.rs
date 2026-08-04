use serde::{Deserialize, Serialize};

use super::status_entry::StatusEntry;

#[derive(Clone, Debug, Deserialize, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct RepositoryStatus {
    pub root: String,
    pub relative_root: String,
    pub branch: Option<String>,
    pub head: Option<String>,
    pub entries: Vec<StatusEntry>,
}
