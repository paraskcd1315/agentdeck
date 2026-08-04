use serde::Deserialize;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct PtyTargetParams {
    pub pty_id: u64,
}
