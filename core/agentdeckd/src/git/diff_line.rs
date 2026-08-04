use serde::{Deserialize, Serialize};

use super::diff_line_kind::DiffLineKind;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct DiffLine {
    pub kind: DiffLineKind,
    pub text: String,
    pub old_line: Option<u32>,
    pub new_line: Option<u32>,
}

impl DiffLine {
    pub fn new(
        kind: DiffLineKind,
        text: impl Into<String>,
        old_line: Option<u32>,
        new_line: Option<u32>,
    ) -> Self {
        Self {
            kind,
            text: text.into(),
            old_line,
            new_line,
        }
    }
}
