use serde::{Deserialize, Serialize};

use super::button::Button;
use super::chart_kind::ChartKind;
use super::chart_series::ChartSeries;
use super::diff_line::DiffLine;
use super::key_value_row::KeyValueRow;
use super::log_entry::LogEntry;
use super::state::PanelState;
use super::step::Step;

#[derive(Clone, Debug, Deserialize, Serialize)]
#[serde(tag = "type", rename_all = "lowercase")]
pub enum KnownBlock {
    Markdown {
        text: String,
    },
    KeyValue {
        rows: Vec<KeyValueRow>,
    },
    Status {
        state: PanelState,
        text: String,
    },
    Actions {
        buttons: Vec<Button>,
    },
    Table {
        columns: Vec<String>,
        rows: Vec<Vec<String>>,
    },
    Diff {
        file: String,
        lines: Vec<DiffLine>,
    },
    Steps {
        steps: Vec<Step>,
    },
    Log {
        entries: Vec<LogEntry>,
    },
    Chart {
        kind: ChartKind,
        labels: Vec<String>,
        series: Vec<ChartSeries>,
    },
}
