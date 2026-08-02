use alacritty_terminal::term::TermMode;
use serde::Serialize;

#[derive(Clone, Debug, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct WireMode {
    pub alt_screen: bool,
    pub application_cursor: bool,
    pub mouse_report: bool,
    pub sgr_mouse: bool,
    pub alternate_scroll: bool,
}

impl WireMode {
    pub fn new(mode: TermMode) -> Self {
        Self {
            alt_screen: mode.contains(TermMode::ALT_SCREEN),
            application_cursor: mode.contains(TermMode::APP_CURSOR),
            mouse_report: mode.intersects(TermMode::MOUSE_MODE),
            sgr_mouse: mode.contains(TermMode::SGR_MOUSE),
            alternate_scroll: mode.contains(TermMode::ALTERNATE_SCROLL),
        }
    }
}
