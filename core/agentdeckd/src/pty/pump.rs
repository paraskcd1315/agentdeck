use tokio::sync::mpsc::UnboundedSender;

use crate::constants::PTY_READ_BUFFER_BYTES;

use super::handle::PtyHandle;

pub(crate) fn pump(reader: PtyHandle, sender: UnboundedSender<Vec<u8>>) {
    let mut buffer = vec![0u8; PTY_READ_BUFFER_BYTES];

    loop {
        match reader.read(&mut buffer) {
            Ok(0) => break,
            Ok(count) => {
                if sender.send(buffer[..count].to_vec()).is_err() {
                    break;
                }
            }
            Err(_) => break,
        }
    }
}
