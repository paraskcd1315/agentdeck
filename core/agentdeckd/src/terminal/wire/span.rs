use serde::Serialize;

use super::style::WireStyle;

#[derive(Clone, Debug, Serialize)]
pub struct WireSpan {
    pub text: String,
    #[serde(flatten)]
    pub style: WireStyle,
}

impl WireSpan {
    pub fn new(style: WireStyle) -> Self {
        Self {
            text: String::new(),
            style,
        }
    }
}
