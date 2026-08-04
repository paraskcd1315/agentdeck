use serde::Deserialize;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct PanelReadParams {
    pub workspace_id: String,
    pub id: String,
}
