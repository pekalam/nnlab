using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Common.Framework;
using Prism.Regions;
using Prism.Services.Dialogs;
using Training.Application.Services;
using Unity;

namespace Training.Application.ViewModels
{
    public class PanelSelectionResult
    {
        private readonly Action<List<PanelSelectModel>> _callback;

        public PanelSelectionResult(Action<List<PanelSelectModel>> callback)
        {
            _callback = callback;
        }

        public void SetResult(List<PanelSelectModel> selected) => _callback(selected);
    }

    public class PanelSelectViewModel : ViewModelBase<PanelSelectViewModel>, IDialogAware
    {
        private List<PanelSelectModel>? _selected;
        private PanelSelectModel? _singleSelected;
        private SelectionMode _selectionMode;

        public PanelSelectViewModel()
        {
            
        }

        [InjectionConstructor]
        public PanelSelectViewModel(IPanelSelectService service)
        {
            Service = service;
            //ApplySelection = controller.ApplySelection;
        }

        public IPanelSelectService Service { get; }

        public Action<List<PanelSelectModel>>? SetSelectedItems { get; set; }


        public List<PanelSelectModel> Panels { get; set; } = new List<PanelSelectModel>()
        {
            new PanelSelectModel()
            {
                PanelType = Application.Panels.LiveEditPanel,
            },
            new PanelSelectModel()
            {
                PanelType = Application.Panels.Accuracy,
            },
            new PanelSelectModel()
            {
                PanelType = Application.Panels.MatrixPreview,
            },
            new PanelSelectModel()
            {
                PanelType = Application.Panels.NetworkVisualization,
            },
            new PanelSelectModel()
            {
                PanelType = Application.Panels.NetworkError,
            },
        };

        public List<PanelSelectModel>? Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public PanelSelectModel? SingleSelected
        {
            get => _singleSelected;
            set => SetProperty(ref _singleSelected, value);
        }

        public SelectionMode SelectionMode
        {
            get => _selectionMode;
            set => SetProperty(ref _selectionMode, value);
        }

        public int MaxSelected { get; set; } = Int32.MaxValue;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Service.Navigated(parameters);
        }

        public void CloseDialog(ButtonResult result) => RequestClose?.Invoke(new DialogResult(result));

        public string Title { get; } = "Select panels";
        public event Action<IDialogResult> RequestClose;
    }
}
