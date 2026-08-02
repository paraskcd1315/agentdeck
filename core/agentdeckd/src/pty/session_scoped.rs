use crate::constants::{SESSION_SCOPED_ENV_NAMES, SESSION_SCOPED_ENV_PREFIXES};

pub(crate) fn is_session_scoped(key: &str) -> bool {
    SESSION_SCOPED_ENV_NAMES.contains(&key)
        || SESSION_SCOPED_ENV_PREFIXES
            .iter()
            .any(|prefix| key.starts_with(prefix))
}
