use serde::{Deserialize, Serialize};

use super::repository_status::RepositoryStatus;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct WorkspaceStatus {
    pub root: String,
    pub repositories: Vec<RepositoryStatus>,
}
