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
        private readonly Dictionary<Session, Matrix<double>> _sessionMatrices = new Dictionary<Session, Matrix<double>>();
        private readonly AppState _appState;
        private readonly AppStateHelper _helper;

        public ModuleState(AppState appState)
        {
            _appState = appState;
            _helper = new AppStateHelper(appState);

            _helper.OnNetworkChanged(network =>
            {
                AssignNewMatrix(_appState.ActiveSession!);
                _appState.ActiveSession!.NetworkStructureChanged -= ActiveSessionOnNetworkStructureChanged;
                _appState.ActiveSession!.NetworkStructureChanged += ActiveSessionOnNetworkStructureChanged;
            });

            if (appState.ActiveSession != null)
            {
                AssignNewMatrix(appState.ActiveSession);
            }
        }

        private void ActiveSessionOnNetworkStructureChanged(MLPNetwork obj)
        {
            AssignNewMatrix(_appState.ActiveSession!);
        }

        public Matrix<double> CurrentInputMatrix
        {
            get
            {
                if (!_sessionMatrices.ContainsKey(_appState.ActiveSession!))
                {
                    AssignNewMatrix(_appState.ActiveSession!);
                }
                return _sessionMatrices[_appState.ActiveSession!];
            }
        }

        private void AssignNewMatrix(Session session)
        {
            if(session.TrainingData != null && session.Network != null)
            {
                _sessionMatrices[session] = Matrix<double>.Build.Dense(session.Network.Layers[0].InputsCount, 1);
                RaisePropertyChanged(nameof(CurrentInputMatrix));
            }
        }
    }

    class PredictController : ControllerBase<PredictViewModel>,ITransientController<PredictService>
    {
        private PredictService _service = null!;
        private readonly AppState _appState;
        private readonly ModuleState _moduleState;
        private readonly AppStateHelper _helper;

        public PredictController(IViewModelAccessor accessor,AppState appState, ModuleState moduleState) : base(accessor)
        {
            _appState = appState;
            _helper = new AppStateHelper(appState);
            _moduleState = moduleState;
        }

        private void ActiveSessionOnNetworkStructureChanged(MLPNetwork obj)
        {
            _service.UpdateNetworkAndMatrix(_appState.ActiveSession!.Network!, _appState.ActiveSession!.TrainingData!, _moduleState.CurrentInputMatrix);
        }

        public void Initialize(PredictService service)
        {
            _service = service;
            service.PredictCommand = new DelegateCommand(Predict);
            service.Navigated = Navigated;
        }

        private void Navigated(NavigationContext obj)
        {
            _helper.OnNetworkChanged(network =>
            {
                _appState.ActiveSession!.NetworkStructureChanged -= ActiveSessionOnNetworkStructureChanged;
                _appState.ActiveSession!.NetworkStructureChanged += ActiveSessionOnNetworkStructureChanged;
                _service.UpdateNetworkAndMatrix(_appState.ActiveSession.Network!, _appState.ActiveSession.TrainingData!, _moduleState.CurrentInputMatrix);
            });
        }

        private void Predict()
        {
            var network = _appState.ActiveSession!.Network!;

            network.CalculateOutput(_moduleState.CurrentInputMatrix);
            _service.UpdateMatrix(network, _appState.ActiveSession!.TrainingData!, _moduleState.CurrentInputMatrix);
        }
    }
}
