use std::path::{Path, PathBuf};

use anyhow::{Result, anyhow};

use crate::constants::{PANELS_DIRECTORY, STATE_DIRECTORY, WORKSPACES_DIRECTORY};

#[derive(Clone, Debug)]
pub struct StateDirs {
    root: PathBuf,
}

impl StateDirs {
    pub fn discover() -> Result<Self> {
        let home = dirs::home_dir().ok_or_else(|| anyhow!("could not resolve the home directory"))?;
        Ok(Self {
            root: home.join(STATE_DIRECTORY),
        })
    }

    pub fn workspaces(&self) -> PathBuf {
        self.root.join(WORKSPACES_DIRECTORY)
    }

    pub fn panels(&self, workspace_id: &str) -> PathBuf {
        self.workspaces().join(workspace_id).join(PANELS_DIRECTORY)
    }

    pub fn ensure_panels(&self, workspace_id: &str) -> Result<PathBuf> {
        let directory = self.panels(workspace_id);
        std::fs::create_dir_all(&directory)?;
        Ok(directory)
    }

    pub fn workspace_of(&self, path: &Path) -> Option<String> {
        let relative = path.strip_prefix(self.workspaces()).ok()?;
        relative
            .components()
            .next()
            .map(|component| component.as_os_str().to_string_lossy().to_string())
    }
}
