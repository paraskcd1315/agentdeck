use serde::{Deserialize, Serialize};

use super::diff_line::DiffLine;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct FileDiff {
    pub path: String,
    pub binary: bool,
    pub added: usize,
    pub removed: usize,
    pub lines: Vec<DiffLine>,
}
