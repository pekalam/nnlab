using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Framework;
using Prism.Regions;

namespace NeuralNetwork.Application.ViewModels
{
    public class NeuralNetworkShellViewModel : ViewModelBase<NeuralNetworkShellViewModel>
    {
        public event Action Navigated;
        private bool _navigatedInvoked;

        public NeuralNetworkShellViewModel()
        {
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!_navigatedInvoked)
            {
                _navigatedInvoked = true;
                Navigated?.Invoke();
            }
        }
    }
}
