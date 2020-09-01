using Data.Application.Services;
using Data.Domain.Services;
using Prism.Commands;
using System;
using System.ComponentModel;
using Common.Domain;
using Common.Framework;
using Data.Application.Controllers.DataSource;
using Prism.Ioc;

namespace Data.Application.Services
{
    public interface INormalizationService
    {
        DelegateCommand NoNormalizationCommand { get; set; }
        DelegateCommand MinMaxNormalizationCommand { get; set; }
        DelegateCommand MeanNormalizationCommand { get; set; }
        DelegateCommand StdNormalizationCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<INormalizationService, NormalizationController>();
        }
    }
}

namespace Data.Application.Controllers.DataSource
{
    internal class NormalizationController : INormalizationService
    {
        private readonly INormalizationDomainService _normalizationService;

        public NormalizationController(INormalizationDomainService normalizationService, AppState appState)
        {
            _normalizationService = normalizationService;

            NoNormalizationCommand = new DelegateCommand(NoNormalization);
            MinMaxNormalizationCommand = new DelegateCommand(MinMaxNormalization);
            MeanNormalizationCommand = new DelegateCommand(MeanNormalization);
            StdNormalizationCommand = new DelegateCommand(StdNormalization);

            appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;
        }

        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) e)
        {
            e.next.PropertyChanged += SessionPropertyChanged;
        }

        private void SessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.TrainingData))
            {
                var session= (sender as Session);
                if(session.TrainingData == null) return;
                session.TrainingData.PropertyChanged -= TrainingDataOnPropertyChanged;
                session.TrainingData.PropertyChanged += TrainingDataOnPropertyChanged;
            }
        }

        private void TrainingDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingData.Variables))
            {
                var trainingData = (sender as TrainingData)!;
                if (trainingData.Source == TrainingDataSource.Csv)
                {
                    switch (trainingData.NormalizationMethod)
                    {
                        case NormalizationMethod.Mean:
                            MeanNormalization();
                            break;
                        case NormalizationMethod.Std:
                            StdNormalization();
                            break;
                        case NormalizationMethod.MinMax:
                            MinMaxNormalization();
                            break;
                    }
                }
            }
        }

        public DelegateCommand NoNormalizationCommand { get; set; }
        public DelegateCommand MinMaxNormalizationCommand { get; set; }
        public DelegateCommand MeanNormalizationCommand { get; set; }
        public DelegateCommand StdNormalizationCommand { get; set; }

        private void StdNormalization()
        {
            _normalizationService.StdNormalization();
        }

        private void MeanNormalization()
        {
            _normalizationService.MeanNormalization();

        }

        private void MinMaxNormalization()
        {
            _normalizationService.MinMaxNormalization();

        }

        private void NoNormalization()
        {
            _normalizationService.NoNormalization();

        }

        public void Initialize()
        {
        }
    }
}
