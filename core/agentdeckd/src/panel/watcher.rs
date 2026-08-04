use std::path::{Path, PathBuf};

use anyhow::Result;
use notify::{Event, RecommendedWatcher, RecursiveMode, Watcher};
use tokio::sync::mpsc::{UnboundedSender, unbounded_channel};

use crate::config::paths::StateDirs;
use crate::constants::PANEL_FILE_EXTENSION;

pub struct PanelWatcher {
    _watcher: RecommendedWatcher,
}

impl PanelWatcher {
    pub fn start(dirs: &StateDirs, changes: UnboundedSender<PathBuf>) -> Result<Self> {
        let workspaces = dirs.workspaces();
        std::fs::create_dir_all(&workspaces)?;

        let (raw, mut receiver) = unbounded_channel::<Event>();
        let mut watcher = notify::recommended_watcher(move |event: notify::Result<Event>| {
            if let Ok(event) = event {
                let _ = raw.send(event);
            }
        })?;

        watcher.watch(&workspaces, RecursiveMode::Recursive)?;

        tokio::spawn(async move {
            while let Some(event) = receiver.recv().await {
                for path in event.paths {
                    if is_panel_file(&path) {
                        let _ = changes.send(path);
                    }
                }
            }
        });

        Ok(Self { _watcher: watcher })
    }
}

fn is_panel_file(path: &Path) -> bool {
    path.extension()
        .and_then(|value| value.to_str())
        .is_some_and(|value| value.eq_ignore_ascii_case(PANEL_FILE_EXTENSION))
}
