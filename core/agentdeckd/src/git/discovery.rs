use std::path::{Path, PathBuf};

use crate::constants::{GIT_DIRECTORY, GIT_SCAN_MAX_DEPTH, GIT_SCAN_SKIP_DIRECTORIES};

pub fn repositories(root: &Path) -> Vec<PathBuf> {
    let mut found = Vec::new();
    collect(root, 0, &mut found);
    found
}

fn collect(directory: &Path, depth: usize, found: &mut Vec<PathBuf>) {
    if directory.join(GIT_DIRECTORY).exists() {
        found.push(directory.to_path_buf());
        return;
    }

    if depth >= GIT_SCAN_MAX_DEPTH {
        return;
    }

    let Ok(entries) = std::fs::read_dir(directory) else {
        return;
    };

    let mut children: Vec<PathBuf> = entries
        .filter_map(Result::ok)
        .map(|entry| entry.path())
        .filter(|path| path.is_dir())
        .filter(|path| !is_skipped(path))
        .collect();

    children.sort();

    for child in children {
        collect(&child, depth + 1, found);
    }
}

fn is_skipped(path: &Path) -> bool {
    path.file_name()
        .and_then(|name| name.to_str())
        .is_some_and(|name| {
            name.starts_with('.') || GIT_SCAN_SKIP_DIRECTORIES.contains(&name)
        })
}
