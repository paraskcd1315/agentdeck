use std::path::Path;

use gix::diff::blob::{Algorithm, InternedInput, diff_with_slider_heuristics};

use crate::constants::DIFF_CONTEXT_LINES;

use super::diff_line::DiffLine;
use super::diff_line_kind::DiffLineKind;
use super::error::GitError;
use super::file_diff::FileDiff;

pub fn file(root: &Path, relative: &str) -> Result<FileDiff, GitError> {
    let absolute = root.join(relative);
    let anchor = absolute.parent().unwrap_or(root);

    let repository =
        gix::discover(anchor).map_err(|_| GitError::Discover(anchor.display().to_string()))?;

    let workdir = repository
        .workdir()
        .ok_or_else(|| GitError::Discover(anchor.display().to_string()))?
        .to_path_buf();

    let tracked = absolute
        .strip_prefix(&workdir)
        .map(|path| path.display().to_string().replace('\\', "/"))
        .unwrap_or_else(|_| relative.to_string());

    let old = committed(&repository, &tracked);
    let new = std::fs::read_to_string(&absolute).unwrap_or_default();

    if old.contains('\0') || new.contains('\0') {
        return Ok(FileDiff {
            path: relative.to_string(),
            binary: true,
            added: 0,
            removed: 0,
            lines: Vec::new(),
        });
    }

    Ok(lines(relative, &old, &new))
}

fn committed(repository: &gix::Repository, relative: &str) -> String {
    let Ok(mut tree) = repository.head_tree() else {
        return String::new();
    };

    let Ok(Some(entry)) = tree.peel_to_entry_by_path(relative) else {
        return String::new();
    };

    entry
        .object()
        .ok()
        .map(|object| String::from_utf8_lossy(&object.data).into_owned())
        .unwrap_or_default()
}

fn lines(path: &str, old: &str, new: &str) -> FileDiff {
    let input = InternedInput::new(old, new);
    let diff = diff_with_slider_heuristics(Algorithm::Histogram, &input);

    let before: Vec<&str> = old.lines().collect();
    let after: Vec<&str> = new.lines().collect();

    let mut lines = Vec::new();
    let mut added = 0;
    let mut removed = 0;
    let mut cursor = 0usize;

    for hunk in diff.hunks() {
        let start = hunk.before.start as usize;
        let context_start = start.saturating_sub(DIFF_CONTEXT_LINES);

        if context_start > cursor {
            lines.push(DiffLine::new(
                DiffLineKind::Hunk,
                format!("@@ -{},{} +{},{} @@", start + 1, hunk.before.len(), hunk.after.start + 1, hunk.after.len()),
                None,
                None,
            ));
            cursor = context_start;
        }

        while cursor < start {
            push_context(&mut lines, &before, &after, cursor, hunk.before.start, hunk.after.start);
            cursor += 1;
        }

        for index in hunk.before.clone() {
            removed += 1;
            lines.push(DiffLine::new(
                DiffLineKind::Delete,
                before.get(index as usize).copied().unwrap_or_default(),
                Some(index + 1),
                None,
            ));
        }

        for index in hunk.after.clone() {
            added += 1;
            lines.push(DiffLine::new(
                DiffLineKind::Add,
                after.get(index as usize).copied().unwrap_or_default(),
                None,
                Some(index + 1),
            ));
        }

        cursor = hunk.before.end as usize;

        let trailing = (cursor + DIFF_CONTEXT_LINES).min(before.len());
        while cursor < trailing {
            push_context(&mut lines, &before, &after, cursor, hunk.before.end, hunk.after.end);
            cursor += 1;
        }
    }

    FileDiff {
        path: path.to_string(),
        binary: false,
        added,
        removed,
        lines,
    }
}

fn push_context(
    lines: &mut Vec<DiffLine>,
    before: &[&str],
    _after: &[&str],
    index: usize,
    before_anchor: u32,
    after_anchor: u32,
) {
    let offset = index as i64 - before_anchor as i64;
    let new_line = (after_anchor as i64 + offset).max(0) as u32;

    lines.push(DiffLine::new(
        DiffLineKind::Context,
        before.get(index).copied().unwrap_or_default(),
        Some(index as u32 + 1),
        Some(new_line + 1),
    ));
}
