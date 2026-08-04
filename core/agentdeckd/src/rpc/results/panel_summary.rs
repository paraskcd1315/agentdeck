use serde::Serialize;

#[derive(Debug, Serialize)]
pub struct PanelSummary {
    pub id: String,
    pub path: String,
}
