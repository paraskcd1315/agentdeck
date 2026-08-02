use base64::Engine;
use base64::engine::general_purpose::STANDARD as BASE64;
use serde::de::DeserializeOwned;
use serde_json::{Value, json};

use crate::constants::NOTIFY_HOOK_EVENT;
use crate::pty::command::PtyCommand;
use crate::pty::session_id::PtyId;
use crate::pty::size::PtySize;

use super::context::ServerContext;
use super::error::RpcError;
use super::method::Method;
use super::notification::Notification;
use super::params::kill::KillParams;
use super::params::resize::ResizeParams;
use super::params::spawn::SpawnParams;
use super::params::write::WriteParams;
use super::results::spawn::SpawnResult;

pub fn dispatch(method: Method, params: Value, context: &ServerContext) -> Result<Value, RpcError> {
    match method {
        Method::PtySpawn => spawn(parse(params)?, context),
        Method::PtyWrite => write(parse(params)?, context),
        Method::PtyResize => resize(parse(params)?, context),
        Method::PtyKill => kill(parse(params)?, context),
        Method::HookEvent => hook_event(params, context),
        Method::Unknown => Err(RpcError::method_not_found("")),
    }
}

fn parse<T: DeserializeOwned>(params: Value) -> Result<T, RpcError> {
    serde_json::from_value(params).map_err(|error| RpcError::invalid_params(error.to_string()))
}

fn spawn(params: SpawnParams, context: &ServerContext) -> Result<Value, RpcError> {
    let mut command = PtyCommand::new(params.program).with_args(params.args);
    if let Some(cwd) = params.cwd {
        command = command.with_cwd(cwd);
    }

    let default = PtySize::default();
    let size = PtySize::new(
        params.cols.unwrap_or(default.cols),
        params.rows.unwrap_or(default.rows),
    );

    let id = context.spawn_pty(&command, size)?;
    serde_json::to_value(SpawnResult { pty_id: id.value() })
        .map_err(|error| RpcError::internal(error.to_string()))
}

fn write(params: WriteParams, context: &ServerContext) -> Result<Value, RpcError> {
    let bytes = BASE64
        .decode(params.data_b64)
        .map_err(|error| RpcError::invalid_params(error.to_string()))?;
    context.write_pty(PtyId::from_value(params.pty_id), &bytes)?;
    Ok(json!({}))
}

fn resize(params: ResizeParams, context: &ServerContext) -> Result<Value, RpcError> {
    context.resize_pty(
        PtyId::from_value(params.pty_id),
        PtySize::new(params.cols, params.rows),
    )?;
    Ok(json!({}))
}

fn kill(params: KillParams, context: &ServerContext) -> Result<Value, RpcError> {
    context.kill_pty(PtyId::from_value(params.pty_id))?;
    Ok(json!({}))
}

fn hook_event(params: Value, context: &ServerContext) -> Result<Value, RpcError> {
    context.notify(Notification::new(NOTIFY_HOOK_EVENT, params));
    Ok(json!({}))
}
