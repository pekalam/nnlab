using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NNLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Regions;
using Unity;

namespace NeuralNetwork.Application.ViewModels
{
    public class LayerListItemModel
    {
        public enum InsertModes
        {
            InsertBefore, InsertAfter
        }

        public int LayerIndex { get; set; }
        public bool IsOutputLayer { get; set; }
        public bool IsAddLayerItem { get; set; }
        public bool IsFirstLayer { get; set; }
        public ICommand? AddLayer { get; set; }
        public ICommand? RemoveLayer { get; set; }
        public ICommand? EditLayer { get; set; }
        public ICommand? InsertBefore { get; set; }
        public ICommand? InsertAfter { get; set; }
        public string LayerTopText => $"Layer {LayerIndex + 1}";

        public string LayerBottomText
        {
            get
            {
                if (IsOutputLayer)
                {
                    return "(output)";
                }

                return "";
            }
        }
    }

    public class LayerListViewModel : ViewModelBase<LayerListViewModel>
    {
        private ObservableCollection<LayerListItemModel> _layers = new ObservableCollection<LayerListItemModel>();
        private LayerListItemModel? _selectedLayer;

                
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public LayerListViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        [InjectionConstructor]
        public LayerListViewModel(ILayerListController controller)
        {
            Controller = controller;
            controller.Initialize(this);
        }

        public ILayerListController Controller { get; }

        public ObservableCollection<LayerListItemModel> Layers
        {
            get => _layers;
            set => SetProperty(ref _layers, value);
        }

        public LayerListItemModel? SelectedLayer
        {
            get => _selectedLayer;
            set => SetProperty(ref _selectedLayer, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("PreviousSelected"))
            {
                Controller.NavigatedFromOpened((int) navigationContext.Parameters["PreviousSelected"]);
            }
        }
    }
}
