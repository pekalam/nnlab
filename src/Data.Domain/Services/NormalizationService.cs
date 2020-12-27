using Common.Domain;
using NNLib.Data;
using System;
using System.Threading.Tasks;
using NNLib;

namespace Data.Domain.Services
{
    public interface INormalizationDomainService
    {
        Task MinMaxNormalization();
        Task MeanNormalization();
        Task StdNormalization();

        Task RobustNormalization();

        void NoNormalization();

        Task Normalize(NormalizationMethod method);
    }

    internal class NormalizationDomainService : INormalizationDomainService
    {
        private AppState _appState;

        public NormalizationDomainService(AppState appState)
        {
            _appState = appState;
        }

        public async Task MinMaxNormalization()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingData sets = trainingData.CloneOriginalSets();
            var normalization = new MinMaxNormalization(0, 1);
            await normalization.FitAndTransform(sets);
            trainingData.ChangeNormalization(sets, NormalizationMethod.MinMax, normalization);
        }

        public async Task MeanNormalization()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingData sets = trainingData.CloneOriginalSets();
            var normalization = new MeanNormalization();
            await normalization.FitAndTransform(sets);
            trainingData.ChangeNormalization(sets, NormalizationMethod.Mean, normalization);
        }

        public async Task StdNormalization()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingData sets = trainingData.CloneOriginalSets();
            var normalization = new Standarization();
            await normalization.FitAndTransform(sets);
            trainingData.ChangeNormalization(sets, NormalizationMethod.Std, normalization);
        }

        public async Task RobustNormalization()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingData sets = trainingData.CloneOriginalSets();
            var normalization = new RobutstNormalization();
            await normalization.FitAndTransform(sets);
            trainingData.ChangeNormalization(sets, NormalizationMethod.Robust, normalization);
        }

        public void NoNormalization()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            trainingData.ChangeNormalization(trainingData.OriginalSets, NormalizationMethod.None, null);
        }

        public async Task Normalize(NormalizationMethod method)
        {
            switch (method)
            {
                case NormalizationMethod.None:
                    NoNormalization();
                    break;
                case NormalizationMethod.Mean:
                    await MeanNormalization();
                    break;
                case NormalizationMethod.MinMax:
                    await MinMaxNormalization();
                    break;
                case NormalizationMethod.Std:
                    await StdNormalization();
                    break;
                case NormalizationMethod.Robust:
                    await RobustNormalization();
                    break;
            }
        }
    }
}
