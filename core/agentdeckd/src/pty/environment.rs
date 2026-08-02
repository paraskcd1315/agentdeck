use std::collections::BTreeMap;

pub(crate) fn build_block(overrides: &BTreeMap<String, String>) -> Option<Vec<u16>> {
    if overrides.is_empty() {
        return None;
    }

    let mut merged: BTreeMap<String, String> = std::env::vars()
        .map(|(key, value)| (key.to_uppercase(), value))
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
    Some(block)
}
