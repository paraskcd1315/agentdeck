mod constants;
mod pty;

use std::io::Write;
use std::time::{Duration, Instant};

use anyhow::Result;

use constants::{
    ARGUMENT_SEPARATOR, DEFAULT_SHELL, PTY_SMOKE_DRAIN_MILLIS, PTY_SMOKE_POLL_MILLIS,
    PTY_SMOKE_SETTLE_MILLIS,
};
use pty::command::PtyCommand;
use pty::session::PtySession;
use pty::size::PtySize;

#[tokio::main]
async fn main() -> Result<()> {
    let arguments: Vec<String> = std::env::args().skip(1).collect();
    let separator = arguments.iter().position(|value| value == ARGUMENT_SEPARATOR);
    let (invocation, lines) = match separator {
        Some(index) => (&arguments[..index], &arguments[index + 1..]),
        None => (&arguments[..], &arguments[arguments.len()..]),
    };

    let program = invocation
        .first()
        .cloned()
        .unwrap_or_else(|| DEFAULT_SHELL.to_string());
    let command = PtyCommand::new(&program).with_args(invocation.iter().skip(1).cloned());

    let started = Instant::now();
    let (session, mut output) = PtySession::spawn(&command, PtySize::default())?;

    let printer = tokio::spawn(async move {
        let mut total = 0usize;
        while let Some(chunk) = output.recv().await {
            total += chunk.len();
            print!("{}", String::from_utf8_lossy(&chunk));
            let _ = std::io::stdout().flush();
        }
        total
    });

    let settle = Duration::from_millis(PTY_SMOKE_SETTLE_MILLIS);
    let deadline = Duration::from_millis(PTY_SMOKE_DRAIN_MILLIS);
    let mut died_after = None;
    let mut written = false;

    while started.elapsed() < deadline {
        if !session.is_running() {
            died_after = Some(started.elapsed());
            break;
        }
        if !written && started.elapsed() >= settle {
            for line in lines {
                session.write(format!("{line}\r").as_bytes())?;
                eprintln!("[smoke] wrote {line:?} at {:?}", started.elapsed());
            }
            written = true;
        }
        tokio::time::sleep(Duration::from_millis(PTY_SMOKE_POLL_MILLIS)).await;
    }

    let exit_code = session.exit_code()?;
    session.kill()?;
    drop(session);

    let bytes = printer.await.unwrap_or_default();
    eprintln!(
        "[smoke] program={program} bytes={bytes} died_after={died_after:?} exit_code={exit_code:?}"
    );

    Ok(())
}
