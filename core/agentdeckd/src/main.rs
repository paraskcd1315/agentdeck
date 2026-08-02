mod constants;
mod pty;
mod rpc;

use anyhow::Result;

use constants::PIPE_NAME;
use rpc::context::ServerContext;
use rpc::server::RpcServer;

#[tokio::main]
async fn main() -> Result<()> {
    let server = RpcServer::new(ServerContext::new());
    eprintln!("[agentdeckd] listening on {PIPE_NAME}");
    server.listen().await
}
