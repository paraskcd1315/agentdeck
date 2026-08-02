use serde::Serialize;

use super::cursor::WireCursor;
use super::line::WireLine;

#[derive(Clone, Debug, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct WireSnapshot {
    pub pty_id: u64,
    pub columns: usize,
    pub rows: usize,
    pub full: bool,
    pub cursor: WireCursor,
    pub lines: Vec<WireLine>,
}
