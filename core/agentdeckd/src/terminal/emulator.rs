use alacritty_terminal::Term;
use alacritty_terminal::grid::Dimensions;
use alacritty_terminal::term::Config;
use alacritty_terminal::vte::ansi::Processor;

use crate::constants::TERMINAL_SCROLLBACK_LINES;
use crate::pty::session_id::PtyId;
use crate::pty::size::PtySize;

use super::dimensions::TerminalDimensions;
use super::event_proxy::EventProxy;
use super::snapshot_builder;
use super::wire::snapshot::WireSnapshot;

pub struct Emulator {
    term: Term<EventProxy>,
    parser: Processor,
}

impl Emulator {
    pub fn new(size: PtySize, proxy: EventProxy) -> Self {
        let config = Config {
            scrolling_history: TERMINAL_SCROLLBACK_LINES,
            ..Config::default()
        };

        Self {
            term: Term::new(config, &TerminalDimensions::new(size), proxy),
            parser: Processor::new(),
        }
    }

    pub fn advance(&mut self, bytes: &[u8]) {
        self.parser.advance(&mut self.term, bytes);
    }

    pub fn resize(&mut self, size: PtySize) {
        self.term.resize(TerminalDimensions::new(size));
    }

    pub fn term(&self) -> &Term<EventProxy> {
        &self.term
    }

    pub fn snapshot(&self, id: PtyId) -> WireSnapshot {
        snapshot_builder::full(id, &self.term)
    }

    pub fn damage(&mut self, id: PtyId) -> Option<WireSnapshot> {
        snapshot_builder::damaged(id, &mut self.term)
    }

    pub fn visible_lines(&self) -> Vec<String> {
        let grid = self.term.grid();
        let columns = grid.columns();
        let mut lines = vec![String::with_capacity(columns); grid.screen_lines()];

        for indexed in grid.display_iter() {
            let row = indexed.point.line.0;
            if row < 0 {
                continue;
            }

            if let Some(line) = lines.get_mut(row as usize) {
                line.push(indexed.cell.c);
            }
        }

        for line in &mut lines {
            while line.ends_with(' ') {
                line.pop();
            }
        }

        lines
    }
}
