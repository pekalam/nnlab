
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Common.Framework;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Shell.Interface;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;

namespace Training.Application.Services
{
    public interface IPanelSelectService
    {
        DelegateCommand ApplySelectionCommand { get; }
        Action<IDialogParameters> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IPanelSelectService, PanelSelectController>();
        }
    }
}

namespace Training.Application.Controllers
{
    class PanelSelectController : IPanelSelectService, ITransientController
    {
        private IViewModelAccessor _accessor;

        private PanelSelectionResult _selectionResult;
        private Panels? _startSelected;
        private bool _single;

        public PanelSelectController(IViewModelAccessor accessor)
        {
            _accessor = accessor;

            ApplySelectionCommand = new DelegateCommand(ApplySelection);
            
            Navigated = OnNavigated;
        }

        private void OnNavigated(IDialogParameters parameters)
        {
            var vm = _accessor.Get<PanelSelectViewModel>();

            _selectionResult = parameters.GetValue<PanelSelectionResult>(nameof(PanelSelectionResult));
            if (parameters.GetValue<bool>("single"))
            {
                _single = true;
                vm.SelectionMode = SelectionMode.Single;
                if (parameters.ContainsKey("selected"))
                {
                    _startSelected = (Panels)parameters.GetValue<Panels>("selected");
                    vm.SingleSelected = vm.Panels.Single(v => v.PanelType == _startSelected);
                }
            }
            else
            {
                vm.SelectionMode = SelectionMode.Multiple;

                if (parameters.ContainsKey("maxSelected"))
                {
                    vm.MaxSelected = parameters.GetValue<int>("maxSelected");
                }

                if (parameters.ContainsKey("selected"))
                {
                    var panels = parameters.GetValue<Panels[]>("selected");
                    vm.SetSelectedItems(vm.Panels.Where(v => panels.Contains(v.PanelType)).ToList());
                }
            }
        }

        private void ApplySelection()
        {
            var vm = _accessor.Get<PanelSelectViewModel>();

            if (_single && vm.Selected?[0].PanelType == _startSelected)
            {
                return;
            }
            _selectionResult.SetResult(vm.Selected ?? new List<PanelSelectModel>());
            vm.CloseDialog(ButtonResult.OK);
        }

        public DelegateCommand ApplySelectionCommand { get; }
        public Action<IDialogParameters> Navigated { get; set; }
    }
}