use anyhow::Result;
use tokio::net::windows::named_pipe::{NamedPipeServer, ServerOptions};

use crate::constants::PIPE_NAME;

use super::connection::serve;
use super::context::ServerContext;

pub struct RpcServer {
    context: ServerContext,
}

impl RpcServer {
    pub fn new(context: ServerContext) -> Self {
        Self { context }
    }

    pub async fn listen(&self) -> Result<()> {
        let mut pipe = create_pipe(true)?;

        loop {
            pipe.connect().await?;
            let connected = std::mem::replace(&mut pipe, create_pipe(false)?);
            let context = self.context.clone();
            tokio::spawn(serve(connected, context));
        }
    }
}

fn create_pipe(first: bool) -> Result<NamedPipeServer> {
    Ok(ServerOptions::new()
        .first_pipe_instance(first)
        .create(PIPE_NAME)?)
}
