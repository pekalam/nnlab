using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Framework;
using NeuralNetwork.Application.Services;
using Prism.Regions;
using Unity;

namespace NeuralNetwork.Application.ViewModels
{
    public class NeuralNetworkShellViewModel : ViewModelBase<NeuralNetworkShellViewModel>
    {
        private bool _navigatedInvoked;

        public NeuralNetworkShellViewModel()
        {
            
        }

        [InjectionConstructor]
        public NeuralNetworkShellViewModel(INeuralNetworkShellService service)
        {
            Service = service;
        }

        private INeuralNetworkShellService Service { get; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!_navigatedInvoked)
            {
                _navigatedInvoked = true;
                Service.Navigated(navigationContext);
            }
        }
    }
}
