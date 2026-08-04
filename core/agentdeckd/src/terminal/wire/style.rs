use alacritty_terminal::term::cell::Flags;
use serde::Serialize;

use super::color::WireColor;

#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
pub struct WireStyle {
    pub fg: WireColor,
    pub bg: WireColor,
    #[serde(skip_serializing_if = "std::ops::Not::not")]
    pub bold: bool,
    #[serde(skip_serializing_if = "std::ops::Not::not")]
    pub italic: bool,
    #[serde(skip_serializing_if = "std::ops::Not::not")]
    pub underline: bool,
    #[serde(skip_serializing_if = "std::ops::Not::not")]
    pub inverse: bool,
    #[serde(skip_serializing_if = "std::ops::Not::not")]
    pub dim: bool,
    #[serde(skip_serializing_if = "std::ops::Not::not")]
    pub strikeout: bool,
}

impl WireStyle {
    pub fn new(fg: WireColor, bg: WireColor, flags: Flags) -> Self {
        Self {
            fg,
            bg,
            bold: flags.contains(Flags::BOLD),
            italic: flags.contains(Flags::ITALIC),
            underline: flags.contains(Flags::UNDERLINE),
            inverse: flags.contains(Flags::INVERSE),
            dim: flags.contains(Flags::DIM),
            strikeout: flags.contains(Flags::STRIKEOUT),
        }
    }
}
