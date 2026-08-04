use std::path::Path;

use serde::{Deserialize, Serialize};

use super::block::Block;
use super::error::PanelError;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct Panel {
    pub schema: String,
    pub title: String,
    #[serde(default)]
    pub blocks: Vec<Block>,
}

impl Panel {
    pub fn load(path: &Path) -> Result<Self, PanelError> {
        let contents = std::fs::read_to_string(path)
            .map_err(|source| PanelError::Read(path.display().to_string(), source))?;

        serde_json::from_str(&contents)
            .map_err(|source| PanelError::Parse(path.display().to_string(), source))
    }
}
