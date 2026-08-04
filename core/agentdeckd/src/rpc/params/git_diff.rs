use serde::Deserialize;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct GitDiffParams {
    pub path: String,
    pub file: String,
}
