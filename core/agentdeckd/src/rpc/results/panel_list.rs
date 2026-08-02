use serde::Serialize;

use super::panel_summary::PanelSummary;

#[derive(Debug, Serialize)]
pub struct PanelListResult {
    pub panels: Vec<PanelSummary>,
}
