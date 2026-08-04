use serde::{Deserialize, Serialize};

use super::step_state::StepState;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct Step {
    pub label: String,
    pub state: StepState,
}
