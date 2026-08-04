use serde::Serialize;

use super::span::WireSpan;

#[derive(Clone, Debug, Serialize)]
pub struct WireLine {
    pub line: usize,
    pub spans: Vec<WireSpan>,
}
