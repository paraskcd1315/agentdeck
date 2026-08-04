use serde_json::Value;
use tokio::io::{AsyncBufReadExt, AsyncWriteExt, BufReader, split};
use tokio::net::windows::named_pipe::NamedPipeServer;

use super::context::ServerContext;
use super::dispatch::dispatch;
use super::error::RpcError;
use super::method::Method;
use super::request::Request;
use super::response::Response;

pub async fn serve(pipe: NamedPipeServer, context: ServerContext) {
    let (reader, mut writer) = split(pipe);
    let mut lines = BufReader::new(reader).lines();
    let mut notifications = context.subscribe();

    loop {
        tokio::select! {
            line = lines.next_line() => {
                match line {
                    Ok(Some(line)) => {
                        if let Some(reply) = handle(&line, &context) {
                            if write_line(&mut writer, &reply).await.is_err() {
                                return;
                            }
                        }
                    }
                    _ => return,
                }
            }
            notification = notifications.recv() => {
                let Ok(notification) = notification else { continue };
                let Ok(encoded) = serde_json::to_string(&notification) else { continue };
                if write_line(&mut writer, &encoded).await.is_err() {
                    return;
                }
            }
        }
    }
}

fn handle(line: &str, context: &ServerContext) -> Option<String> {
    if line.trim().is_empty() {
        return None;
    }

    let request: Request = match serde_json::from_str(line) {
        Ok(request) => request,
        Err(error) => return encode(Response::failure(Value::Null, RpcError::parse(error.to_string()))),
    };

    let is_notification = request.is_notification();
    let id = request.id.clone().unwrap_or(Value::Null);
    let method = Method::parse(&request.method);

    if method == Method::Unknown && !is_notification {
        return encode(Response::failure(id, RpcError::method_not_found(&request.method)));
    }

    let outcome = dispatch(method, request.params, context);

    if is_notification {
        return None;
    }

    match outcome {
        Ok(result) => encode(Response::success(id, result)),
        Err(error) => encode(Response::failure(id, error)),
    }
}

fn encode(response: Response) -> Option<String> {
    serde_json::to_string(&response).ok()
}

async fn write_line<W>(writer: &mut W, payload: &str) -> std::io::Result<()>
where
    W: AsyncWriteExt + Unpin,
{
    writer.write_all(payload.as_bytes()).await?;
    writer.write_all(b"\n").await?;
    writer.flush().await
}
