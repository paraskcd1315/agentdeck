use std::collections::BTreeSet;

use alacritty_terminal::Term;
use alacritty_terminal::grid::Dimensions;
use alacritty_terminal::index::{Column, Line, Point};
use alacritty_terminal::term::TermDamage;
use alacritty_terminal::term::cell::Flags;

use crate::pty::session_id::PtyId;

use super::event_proxy::EventProxy;
use super::wire::color::WireColor;
use super::wire::cursor::WireCursor;
use super::wire::line::WireLine;
use super::wire::mode::WireMode;
use super::wire::snapshot::WireSnapshot;
use super::wire::span::WireSpan;
use super::wire::style::WireStyle;

const CURSOR_SHAPE_BLOCK: &str = "block";

pub fn full(id: PtyId, term: &Term<EventProxy>) -> WireSnapshot {
    let rows = (0..term.screen_lines()).collect();
    build(id, term, rows, true)
}

pub fn damaged(id: PtyId, term: &mut Term<EventProxy>) -> Option<WireSnapshot> {
    let rows: BTreeSet<usize> = match term.damage() {
        TermDamage::Full => (0..term.screen_lines()).collect(),
        TermDamage::Partial(iterator) => iterator.map(|bounds| bounds.line).collect(),
    };

    term.reset_damage();

    if rows.is_empty() {
        return None;
    }

    Some(build(id, term, rows, false))
}

fn build(
    id: PtyId,
    term: &Term<EventProxy>,
    rows: BTreeSet<usize>,
    full: bool,
) -> WireSnapshot {
    let grid = term.grid();
    let columns = grid.columns();
    let screen_lines = grid.screen_lines();

    let offset = grid.display_offset() as i32;

    let lines = rows
        .into_iter()
        .filter(|row| *row < screen_lines)
        .map(|row| WireLine {
            line: row,
            spans: spans_for(term, row as i32 - offset, columns),
        })
        .collect();

    WireSnapshot {
        pty_id: id.value(),
        columns,
        rows: screen_lines,
        full,
        cursor: cursor_of(term),
        mode: WireMode::new(*term.mode()),
        lines,
    }
}

fn spans_for(term: &Term<EventProxy>, row: i32, columns: usize) -> Vec<WireSpan> {
    let grid = term.grid();

    if row < grid.topmost_line().0 || row > grid.bottommost_line().0 {
        return Vec::new();
    }

    let mut spans: Vec<WireSpan> = Vec::new();

    for column in 0..columns {
        let cell = &grid[Line(row)][Column(column)];

        if cell.flags.contains(Flags::WIDE_CHAR_SPACER) {
            continue;
        }

        let style = WireStyle::new(
            WireColor::foreground(cell.fg),
            WireColor::background(cell.bg),
            cell.flags,
        );

        match spans.last_mut() {
            Some(last) if last.style == style => last.text.push(cell.c),
            _ => {
                let mut span = WireSpan::new(style);
                span.text.push(cell.c);
                spans.push(span);
            }
        }
    }

    spans
}

fn cursor_of(term: &Term<EventProxy>) -> WireCursor {
    let grid = term.grid();
    let Point { line, column } = grid.cursor.point;
    let offset = grid.display_offset() as i32;
    let row = line.0 + offset;

    WireCursor {
        line: row.max(0) as usize,
        column: column.0,
        visible: row >= 0 && (row as usize) < grid.screen_lines(),
        shape: CURSOR_SHAPE_BLOCK.to_string(),
    }
}
