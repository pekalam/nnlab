using Prism.Commands;

namespace Data.Application.Services
{
    public interface INormalizationService
    {
        DelegateCommand NoNormalizationCommand { get; set; }
        DelegateCommand MinMaxNormalizationCommand { get; set; }
        DelegateCommand MeanNormalizationCommand { get; set; }
        DelegateCommand StdNormalizationCommand { get; set; }
    }

    internal class NormalizationService : INormalizationService
    {
        public DelegateCommand NoNormalizationCommand { get; set; }
        public DelegateCommand MinMaxNormalizationCommand { get; set; }
        public DelegateCommand MeanNormalizationCommand { get; set; }
        public DelegateCommand StdNormalizationCommand { get; set; }
    }
}
