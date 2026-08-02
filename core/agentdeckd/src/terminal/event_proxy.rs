use alacritty_terminal::event::{Event, EventListener};
use tokio::sync::mpsc::UnboundedSender;

#[derive(Clone)]
pub struct EventProxy {
    events: UnboundedSender<Event>,
}

impl EventProxy {
    pub fn new(events: UnboundedSender<Event>) -> Self {
        Self { events }
    }
}

impl EventListener for EventProxy {
    fn send_event(&self, event: Event) {
        let _ = self.events.send(event);
    }
}
