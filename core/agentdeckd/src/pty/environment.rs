use std::collections::BTreeMap;

use super::session_scoped::is_session_scoped;

pub(crate) fn build_block(overrides: &BTreeMap<String, String>) -> Vec<u16> {
    let mut merged: BTreeMap<String, String> = std::env::vars()
        .map(|(key, value)| (key.to_uppercase(), value))
        .filter(|(key, _)| !is_session_scoped(key))
        .collect();

    for (key, value) in overrides {
        merged.insert(key.to_uppercase(), value.clone());
    }

    let mut block: Vec<u16> = Vec::new();
    for (key, value) in &merged {
        block.extend(format!("{key}={value}").encode_utf16());
        block.push(0);
    }

    block.push(0);
    block
}
