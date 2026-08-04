use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct ChartSeries {
    pub label: String,
    pub points: Vec<f64>,
}
