use serde::{Deserialize, Serialize};

use super::known_block::KnownBlock;

#[derive(Clone, Debug, Deserialize, Serialize)]
#[serde(untagged)]
pub enum Block {
    Known(KnownBlock),
    Unknown(serde_json::Value),
}
