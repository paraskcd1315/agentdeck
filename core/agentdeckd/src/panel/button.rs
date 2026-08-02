use serde::{Deserialize, Serialize};

use super::target::InjectionTarget;

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct Button {
    pub label: String,
    pub prompt: String,
    #[serde(default)]
    pub target: InjectionTarget,
}
