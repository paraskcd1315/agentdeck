mod client;
mod constants;
mod notification;

use std::io::Read;

fn main() {
    let mut payload = String::new();
    if std::io::stdin().read_to_string(&mut payload).is_err() {
        return;
    }

    let _ = client::send(&notification::build(&payload));
}
