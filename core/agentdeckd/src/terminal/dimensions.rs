use alacritty_terminal::grid::Dimensions;

use crate::constants::TERMINAL_SCROLLBACK_LINES;
use crate::pty::size::PtySize;

pub struct TerminalDimensions {
    columns: usize,
    screen_lines: usize,
}

impl TerminalDimensions {
    pub fn new(size: PtySize) -> Self {
        Self {
            columns: size.cols as usize,
            screen_lines: size.rows as usize,
        }
    }
}

impl Dimensions for TerminalDimensions {
    fn total_lines(&self) -> usize {
        self.screen_lines + TERMINAL_SCROLLBACK_LINES
    }

    fn screen_lines(&self) -> usize {
        self.screen_lines
    }

    fn columns(&self) -> usize {
        self.columns
    }
}
