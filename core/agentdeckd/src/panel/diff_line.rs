use serde::{Deserialize, Serialize};

use super::diff_line_kind::DiffLineKind;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct DiffLine {
    pub kind: DiffLineKind,
    pub text: String,
}
