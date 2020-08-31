using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media.Animation;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using Prediction.Application.Services;
using Prediction.Application.ViewModels;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Prediction.Application.Controllers
{
    public class ModuleState : BindableBase
    {
        private Dictionary<Session, Matrix<double>> _sessionMatrices = new Dictionary<Session, Matrix<double>>();
        private Matrix<double> _currentInputMatrix;
        private AppState _appState;

        public ModuleState(AppState appState)
        {
            _appState = appState;
            appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;
            if (appState.ActiveSession != null)
            {
                AssignNewMatrix(appState.ActiveSession);
            }
        }

        public Matrix<double> CurrentInputMatrix
        {
            get => _currentInputMatrix;
            set => SetProperty(ref _currentInputMatrix, value);
        }

        private void AssignNewMatrix(Session session)
        {
            if(session?.TrainingData != null && session.Network != null)
            {
                CurrentInputMatrix = Matrix<double>.Build.Dense(session.Network.Layers[0].InputsCount, 1);
                _sessionMatrices[session] = _currentInputMatrix;
            }
        }

        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) e)
        {
            AssignNewMatrix(e.next);

            e.next.PropertyChanged -= NextOnPropertyChanged;
            e.next.PropertyChanged += NextOnPropertyChanged;
        }

        private void NextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AssignNewMatrix(_appState.ActiveSession);
        }
    }

    class PredictController : ControllerBase<PredictViewModel>,ITransientController<PredictService>
    {
        private readonly AppState _appState;
        private PredictService _service;
        private readonly ModuleState _moduleState;

        public PredictController(IViewModelAccessor accessor,AppState appState, ModuleState moduleState) : base(accessor)
        {
            _appState = appState;
            _moduleState = moduleState;
        }

        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) e)
        {
            if (e.next.TrainingData != null && e.next.Network != null)
            {
                _service.UpdateNetworkAndMatrix(e.next.Network, e.next.TrainingData, _moduleState.CurrentInputMatrix);
            }

            e.next.PropertyChanged -= ActiveSessionPropertyChanged;
            e.next.PropertyChanged += ActiveSessionPropertyChanged;
        }

        private void ActiveSessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_appState.ActiveSession?.TrainingData != null && _appState.ActiveSession.Network != null)
            {
                _service.UpdateNetworkAndMatrix(_appState.ActiveSession.Network, _appState.ActiveSession.TrainingData, _moduleState.CurrentInputMatrix);
            }
        }

        private void ActiveSessionOnNetworkStructureChanged(MLPNetwork obj)
        {
            _service.UpdateMatrix(obj, _moduleState.CurrentInputMatrix);
        }

        public void Initialize(PredictService service)
        {
            _service = service;
            service.PredictCommand = new DelegateCommand(Predict);
            service.Navigated = Navigated;
        }

        private void Navigated(NavigationContext obj)
        {
            _appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;
            _appState.ActiveSession.NetworkStructureChanged += ActiveSessionOnNetworkStructureChanged;
            if (_appState.ActiveSession?.TrainingData != null && _appState.ActiveSession.Network != null)
            {
                _service.UpdateNetworkAndMatrix(_appState.ActiveSession.Network, _appState.ActiveSession.TrainingData, _moduleState.CurrentInputMatrix);
            }
        }

        private void Predict()
        {
            var network = _appState.ActiveSession.Network;

            network.CalculateOutput(_moduleState.CurrentInputMatrix);
            _service.UpdateMatrix(network, _moduleState.CurrentInputMatrix);
        }
    }
}
