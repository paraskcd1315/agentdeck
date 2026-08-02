use serde::Deserialize;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ScrollParams {
    pub pty_id: u64,
    pub delta: i32,
}
