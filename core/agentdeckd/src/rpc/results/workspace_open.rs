use serde::Serialize;

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct WorkspaceOpenResult {
    pub workspace_id: String,
    pub panels_path: String,
}
