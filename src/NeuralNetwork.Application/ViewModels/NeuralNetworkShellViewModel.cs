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

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public NeuralNetworkShellViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
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
