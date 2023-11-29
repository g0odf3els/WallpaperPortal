using ImageTagger.Core;
using WallpaperPortal.Services.Abstract;

namespace WallpaperPortal.Services
{
    public class ModelPredictionService : IModelPredictionService
    {
        private readonly ModelPrediction _modelPrediction;

        public ModelPredictionService()
        {
            _modelPrediction = new ModelPrediction("AIModels\\prediction.onnx", "AIModels\\prediction_categories.txt");
        }

        public IEnumerable<Prediction> PredictTags(string filePath)
        {
            return _modelPrediction.PredictTags(filePath);
        }
    }
}
