use serde::Deserialize;

#[derive(Debug, Deserialize)]
pub struct WorkspaceOpenParams {
    pub path: String,
}
