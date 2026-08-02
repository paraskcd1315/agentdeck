use std::path::Path;

use base64::Engine;
use base64::engine::general_purpose::STANDARD as BASE64;
use serde::de::DeserializeOwned;
use serde_json::{Value, json};

use crate::config::workspace_id;
use crate::constants::{NOTIFY_HOOK_EVENT, STATE_DIR_ENV};
use crate::panel::service as panel_service;
use crate::pty::command::PtyCommand;
use crate::pty::session_id::PtyId;
use crate::pty::size::PtySize;

use super::context::ServerContext;
use super::error::RpcError;
use super::method::Method;
use super::notification::Notification;
use super::params::panel_list::PanelListParams;
use super::params::panel_read::PanelReadParams;
use super::params::pty_target::PtyTargetParams;
use super::params::resize::ResizeParams;
use super::params::spawn::SpawnParams;
use super::params::workspace_open::WorkspaceOpenParams;
use super::params::write::WriteParams;
use super::results::panel_list::PanelListResult;
use super::results::panel_summary::PanelSummary;
use super::results::spawn::SpawnResult;
use super::results::workspace_open::WorkspaceOpenResult;

pub fn dispatch(method: Method, params: Value, context: &ServerContext) -> Result<Value, RpcError> {
    match method {
        Method::PtySpawn => spawn(parse(params)?, context),
        Method::PtyWrite => write(parse(params)?, context),
        Method::PtyResize => resize(parse(params)?, context),
        Method::PtyKill => kill(parse(params)?, context),
        Method::HookEvent => hook_event(params, context),
        Method::PanelList => panel_list(parse(params)?, context),
        Method::PanelRead => panel_read(parse(params)?, context),
        Method::WorkspaceOpen => workspace_open(parse(params)?, context),
        Method::TerminalText => terminal_text(parse(params)?, context),
        Method::TerminalSnapshot => terminal_snapshot(parse(params)?, context),
        Method::Unknown => Err(RpcError::method_not_found("")),
    }
}

fn panel_list(params: PanelListParams, context: &ServerContext) -> Result<Value, RpcError> {
    let panels = panel_service::list(context.dirs(), &params.workspace_id)
        .into_iter()
        .map(|(id, path)| PanelSummary {
            id,
            path: path.display().to_string(),
        })
        .collect();

    serde_json::to_value(PanelListResult { panels })
        .map_err(|error| RpcError::internal(error.to_string()))
}

fn panel_read(params: PanelReadParams, context: &ServerContext) -> Result<Value, RpcError> {
    let panel = panel_service::read(context.dirs(), &params.workspace_id, &params.id)
        .map_err(|error| RpcError::invalid_params(error.to_string()))?;

    serde_json::to_value(panel).map_err(|error| RpcError::internal(error.to_string()))
}

fn workspace_open(params: WorkspaceOpenParams, context: &ServerContext) -> Result<Value, RpcError> {
    let id = workspace_id::derive(Path::new(&params.path));
    let panels = context
        .dirs()
        .ensure_panels(&id)
        .map_err(|error| RpcError::internal(error.to_string()))?;

    serde_json::to_value(WorkspaceOpenResult {
        workspace_id: id,
        panels_path: panels.display().to_string(),
    })
    .map_err(|error| RpcError::internal(error.to_string()))
}

fn parse<T: DeserializeOwned>(params: Value) -> Result<T, RpcError> {
    serde_json::from_value(params).map_err(|error| RpcError::invalid_params(error.to_string()))
}

fn spawn(params: SpawnParams, context: &ServerContext) -> Result<Value, RpcError> {
    let mut env = params.env;
    env.insert(
        STATE_DIR_ENV.to_string(),
        context.dirs().root().display().to_string(),
    );

    let mut command = PtyCommand::new(params.program)
        .with_args(params.args)
        .with_env(env);

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

fn kill(params: PtyTargetParams, context: &ServerContext) -> Result<Value, RpcError> {
    context.kill_pty(PtyId::from_value(params.pty_id))?;
    Ok(json!({}))
}

fn terminal_text(params: PtyTargetParams, context: &ServerContext) -> Result<Value, RpcError> {
    let lines = context.visible_lines(PtyId::from_value(params.pty_id))?;
    Ok(json!({ "lines": lines }))
}

fn terminal_snapshot(params: PtyTargetParams, context: &ServerContext) -> Result<Value, RpcError> {
    let snapshot = context.snapshot(PtyId::from_value(params.pty_id))?;
    serde_json::to_value(snapshot).map_err(|error| RpcError::internal(error.to_string()))
}

fn hook_event(params: Value, context: &ServerContext) -> Result<Value, RpcError> {
    context.notify(Notification::new(NOTIFY_HOOK_EVENT, params));
    Ok(json!({}))
}
