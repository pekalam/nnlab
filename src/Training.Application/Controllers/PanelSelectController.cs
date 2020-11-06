
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
    public interface IPanelSelectService : ITransientController
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
    class PanelSelectController : ControllerBase<PanelSelectViewModel>,IPanelSelectService
    {
        private PanelSelectionResult _selectionResult = null!;
        private Panels? _startSelected;
        private bool _single;

        public PanelSelectController()
        {
            ApplySelectionCommand = new DelegateCommand(ApplySelection);
            
            Navigated = OnNavigated;
        }

        private void OnNavigated(IDialogParameters parameters)
        {
            _selectionResult = parameters.GetValue<PanelSelectionResult>(nameof(PanelSelectionResult));
            if (parameters.GetValue<bool>("single"))
            {
                _single = true;
                Vm!.SelectionMode = SelectionMode.Single;
                if (parameters.ContainsKey("selected"))
                {
                    _startSelected = (Panels)parameters.GetValue<Panels>("selected");
                    Vm!.SingleSelected = Vm!.Panels.Single(v => v.PanelType == _startSelected);
                }
            }
            else
            {
                Vm!.SelectionMode = SelectionMode.Multiple;

                if (parameters.ContainsKey("maxSelected"))
                {
                    Vm!.MaxSelected = parameters.GetValue<int>("maxSelected");
                }

                if (parameters.ContainsKey("selected"))
                {
                    var panels = parameters.GetValue<Panels[]>("selected");
                    Vm!.SetSelectedItems?.Invoke(Vm!.Panels.Where(v => panels.Contains(v.PanelType)).ToList()!);
                }
            }
        }

        private void ApplySelection()
        {

            if (_single && Vm!.Selected?[0].PanelType == _startSelected)
            {
                return;
            }
            _selectionResult.SetResult(Vm!.Selected ?? new List<PanelSelectModel>());
            Vm!.CloseDialog(ButtonResult.OK);
        }

        public DelegateCommand ApplySelectionCommand { get; }
        public Action<IDialogParameters> Navigated { get; set; }
    }
}