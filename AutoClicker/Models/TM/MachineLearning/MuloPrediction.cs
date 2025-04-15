using Microsoft.ML.Data;

namespace AutoClicker.Models.TM.MachineLearning
{
    public class MuloPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabelValue { get; set; }
        public float[] Score { get; set; }
    }
}
