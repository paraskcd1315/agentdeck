use serde::{Deserialize, Serialize};

use super::button::Button;
use super::key_value_row::KeyValueRow;
use super::state::PanelState;

#[derive(Clone, Debug, Deserialize, Serialize)]
#[serde(tag = "type", rename_all = "lowercase")]
pub enum Block {
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
}
