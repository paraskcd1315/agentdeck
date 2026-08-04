use serde::Deserialize;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ResizeParams {
    pub pty_id: u64,
    pub cols: u16,
    pub rows: u16,
}
