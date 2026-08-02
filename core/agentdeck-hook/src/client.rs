use std::fs::OpenOptions;
use std::io::Write;

use anyhow::Result;

use crate::constants::PIPE_PATH;

pub fn send(line: &str) -> Result<()> {
    let mut pipe = OpenOptions::new().read(true).write(true).open(PIPE_PATH)?;
    pipe.write_all(line.as_bytes())?;
    pipe.write_all(b"\n")?;
    pipe.flush()?;
    Ok(())
}
