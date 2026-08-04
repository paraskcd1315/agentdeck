use serde_json::{Value, json};

use crate::constants::{
    BYTE_ORDER_MARK, DEFAULT_HOOK_NAME, HOOK_NAME_ENV, JSONRPC_VERSION, METHOD_HOOK_EVENT,
};

pub fn build(payload: &str) -> String {
    let trimmed = payload.trim_start_matches(BYTE_ORDER_MARK).trim();
    let parsed: Value =
        serde_json::from_str(trimmed).unwrap_or_else(|_| json!({ "raw": trimmed }));
    let hook = std::env::var(HOOK_NAME_ENV).unwrap_or_else(|_| DEFAULT_HOOK_NAME.to_string());
    let tool = parsed.get("tool_name").and_then(Value::as_str);

    let notification = json!({
        "jsonrpc": JSONRPC_VERSION,
        "method": METHOD_HOOK_EVENT,
        "params": {
            "hook": hook,
            "tool": tool,
            "payload": parsed,
        }
    });

    notification.to_string()
}
