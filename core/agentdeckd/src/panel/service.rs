use std::path::{Path, PathBuf};

use crate::config::paths::StateDirs;
use crate::constants::PANEL_FILE_EXTENSION;

use super::error::PanelError;
use super::panel::Panel;

pub fn list(dirs: &StateDirs, workspace_id: &str) -> Vec<(String, PathBuf)> {
    let directory = dirs.panels(workspace_id);
    let Ok(entries) = std::fs::read_dir(&directory) else {
        return Vec::new();
    };

    let mut panels: Vec<(String, PathBuf)> = entries
        .filter_map(Result::ok)
        .map(|entry| entry.path())
        .filter(|path| has_panel_extension(path))
        .filter_map(|path| identifier(&path).map(|id| (id, path)))
        .collect();

    panels.sort_by(|left, right| left.0.cmp(&right.0));
    panels
}

pub fn read(dirs: &StateDirs, workspace_id: &str, id: &str) -> Result<Panel, PanelError> {
    let path = dirs
        .panels(workspace_id)
        .join(format!("{id}.{PANEL_FILE_EXTENSION}"));
    Panel::load(&path)
}

pub fn identifier(path: &Path) -> Option<String> {
    path.file_stem()
        .and_then(|value| value.to_str())
        .map(str::to_string)
}

fn has_panel_extension(path: &Path) -> bool {
    path.extension()
        .and_then(|value| value.to_str())
        .is_some_and(|value| value.eq_ignore_ascii_case(PANEL_FILE_EXTENSION))
}
