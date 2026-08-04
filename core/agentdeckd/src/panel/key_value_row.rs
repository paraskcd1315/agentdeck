use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct KeyValueRow {
    pub key: String,
    pub value: String,
}
