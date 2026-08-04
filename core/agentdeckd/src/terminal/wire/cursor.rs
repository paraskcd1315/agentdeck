use serde::Serialize;

#[derive(Clone, Debug, Serialize)]
pub struct WireCursor {
    pub line: usize,
    pub column: usize,
    pub visible: bool,
    pub shape: String,
}
