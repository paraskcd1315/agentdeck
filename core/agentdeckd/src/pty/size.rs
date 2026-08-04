use windows::Win32::System::Console::COORD;

use crate::constants::{DEFAULT_PTY_COLS, DEFAULT_PTY_ROWS};

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct PtySize {
    pub cols: u16,
    pub rows: u16,
}

impl PtySize {
    pub fn new(cols: u16, rows: u16) -> Self {
        Self { cols, rows }
    }

    pub(crate) fn to_coord(self) -> COORD {
        COORD {
            X: self.cols as i16,
            Y: self.rows as i16,
        }
    }
}

impl Default for PtySize {
    fn default() -> Self {
        Self::new(DEFAULT_PTY_COLS, DEFAULT_PTY_ROWS)
    }
}
