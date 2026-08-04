use serde::{Deserialize, Serialize};

use super::log_level::LogLevel;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct LogEntry {
    pub level: LogLevel,
    pub text: String,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub time: Option<String>,
}
