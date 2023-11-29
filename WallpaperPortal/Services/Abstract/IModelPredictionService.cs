using ImageTagger.Core;

namespace WallpaperPortal.Services.Abstract
{
    public interface IModelPredictionService
    {
        IEnumerable<Prediction> PredictTags(string filePath);
    }
}
