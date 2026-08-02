mod config;
mod constants;
mod panel;
mod pty;
mod rpc;

use std::path::PathBuf;

use anyhow::Result;
use serde_json::json;
use tokio::sync::mpsc::unbounded_channel;

use config::paths::StateDirs;
use constants::{NOTIFY_PANEL_CHANGED, PIPE_NAME};
use panel::service as panel_service;
use panel::watcher::PanelWatcher;
use rpc::context::ServerContext;
use rpc::notification::Notification;
use rpc::server::RpcServer;

#[tokio::main]
async fn main() -> Result<()> {
    let dirs = StateDirs::discover()?;
    let context = ServerContext::new(dirs.clone());

    let (changes, mut receiver) = unbounded_channel::<PathBuf>();
    let _watcher = PanelWatcher::start(&dirs, changes)?;

    let watcher_context = context.clone();
    let watcher_dirs = dirs.clone();
    tokio::spawn(async move {
        while let Some(path) = receiver.recv().await {
            let Some(workspace_id) = watcher_dirs.workspace_of(&path) else {
                continue;
            };
            let Some(id) = panel_service::identifier(&path) else {
                continue;
            };

            watcher_context.notify(Notification::new(
                NOTIFY_PANEL_CHANGED,
                json!({
                    "workspaceId": workspace_id,
                    "id": id,
                    "path": path.display().to_string(),
                }),
            ));
        }
    });

    eprintln!("[agentdeckd] listening on {PIPE_NAME}");
    RpcServer::new(context).listen().await
}
