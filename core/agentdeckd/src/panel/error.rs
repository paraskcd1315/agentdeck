use thiserror::Error;

#[derive(Debug, Error)]
pub enum PanelError {
    #[error("failed to read {0}: {1}")]
    Read(String, #[source] std::io::Error),
    #[error("failed to parse {0}: {1}")]
    Parse(String, #[source] serde_json::Error),
}
