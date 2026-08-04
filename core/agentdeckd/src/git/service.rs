use std::path::Path;

use gix::status::index_worktree::Item;
use gix::status::plumbing::index_as_worktree::{Change, EntryStatus};

use super::error::GitError;
use super::repository_status::RepositoryStatus;
use super::status_entry::StatusEntry;
use super::status_kind::StatusKind;

pub fn status(path: &Path) -> Result<RepositoryStatus, GitError> {
    let repository =
        gix::discover(path).map_err(|_| GitError::Discover(path.display().to_string()))?;

    let root = repository
        .workdir()
        .unwrap_or_else(|| repository.git_dir())
        .display()
        .to_string();

    let branch = repository
        .head_name()
        .ok()
        .flatten()
        .map(|name| name.shorten().to_string());

    let head = repository
        .head_id()
        .ok()
        .map(|id| id.shorten_or_id().to_string());

    let iterator = repository
        .status(gix::progress::Discard)
        .map_err(|error| GitError::Status(error.to_string()))?
        .into_index_worktree_iter(Vec::new())
        .map_err(|error| GitError::Status(error.to_string()))?;

    let mut entries = Vec::new();

    for item in iterator {
        let item = item.map_err(|error| GitError::Status(error.to_string()))?;
        entries.push(entry(item));
    }

    entries.sort_by(|left, right| left.path.cmp(&right.path));

    Ok(RepositoryStatus {
        root,
        branch,
        head,
        entries,
    })
}

fn entry(item: Item) -> StatusEntry {
    match item {
        Item::Modification {
            rela_path, status, ..
        } => StatusEntry::new(rela_path.to_string(), kind(&status)),
        Item::DirectoryContents { entry, .. } => {
            StatusEntry::new(entry.rela_path.to_string(), StatusKind::Untracked)
        }
        Item::Rewrite { dirwalk_entry, .. } => {
            StatusEntry::new(dirwalk_entry.rela_path.to_string(), StatusKind::Renamed)
        }
    }
}

fn kind(status: &EntryStatus<(), gix::submodule::Status>) -> StatusKind {
    match status {
        EntryStatus::Conflict { .. } => StatusKind::Conflicted,
        EntryStatus::IntentToAdd => StatusKind::Added,
        EntryStatus::NeedsUpdate(_) => StatusKind::Modified,
        EntryStatus::Change(change) => match change {
            Change::Removed => StatusKind::Deleted,
            Change::Type { .. } => StatusKind::TypeChanged,
            _ => StatusKind::Modified,
        },
    }
}
