use alacritty_terminal::vte::ansi::{Color, NamedColor};
use serde::Serialize;

const ANSI_PALETTE_SIZE: usize = 16;

#[derive(Clone, Debug, PartialEq, Eq, Serialize)]
#[serde(tag = "k", content = "v", rename_all = "lowercase")]
pub enum WireColor {
    Idx(u8),
    Rgb(String),
    Fg,
    Bg,
}

impl WireColor {
    pub fn foreground(color: Color) -> Self {
        Self::convert(color, Self::Fg)
    }

    pub fn background(color: Color) -> Self {
        Self::convert(color, Self::Bg)
    }

    fn convert(color: Color, fallback: Self) -> Self {
        match color {
            Color::Spec(rgb) => Self::Rgb(format!("#{:02X}{:02X}{:02X}", rgb.r, rgb.g, rgb.b)),
            Color::Indexed(index) => Self::Idx(index),
            Color::Named(named) => Self::from_named(named, fallback),
        }
    }

    fn from_named(named: NamedColor, fallback: Self) -> Self {
        match named {
            NamedColor::Foreground => Self::Fg,
            NamedColor::Background => Self::Bg,
            other => {
                let index = other as usize;
                if index < ANSI_PALETTE_SIZE {
                    Self::Idx(index as u8)
                } else {
                    fallback
                }
            }
        }
    }
}
