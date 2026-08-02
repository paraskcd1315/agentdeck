use std::path::Path;

use sha2::{Digest, Sha256};

use crate::constants::WORKSPACE_ID_HEX_LENGTH;

pub fn derive(path: &Path) -> String {
    let normalised = path.to_string_lossy().replace('\\', "/").to_lowercase();
    let digest = Sha256::digest(normalised.as_bytes());
    let hex = digest
        .iter()
        .map(|byte| format!("{byte:02x}"))
        .collect::<String>();
    hex[..WORKSPACE_ID_HEX_LENGTH].to_string()
}
