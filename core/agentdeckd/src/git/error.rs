use thiserror::Error;

#[derive(Debug, Error)]
pub enum GitError {
    #[error("no git repository at {0}")]
    Discover(String),
    #[error("failed to read repository status: {0}")]
    Status(String),
    #[error("failed to diff {0}: {1}")]
    Diff(String, String),
}
