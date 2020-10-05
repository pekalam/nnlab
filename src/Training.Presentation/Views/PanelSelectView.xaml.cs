using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Training.Application;
using Training.Application.ViewModels;

namespace Training.Presentation.Views
{
    /// <summary>
    /// Interaction logic for PanelSelectView
    /// </summary>
    public partial class PanelSelectView : UserControl
    {
        private PanelSelectViewModel? _vm;

        public PanelSelectView()
        {
            InitializeComponent();
        }

        private void SetSelectedItems(List<PanelSelectModel> items)
        {
            foreach (var item in items)
            {
                List.SelectedItems.Add(item);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.Property.Name)
            {
                case nameof(UserControl.DataContext):
                    if (DataContext is PanelSelectViewModel vm)
                    {
                        vm.SetSelectedItems = SetSelectedItems;
                        _vm = vm;
                    }
                    break;

            }
        }

        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (List.SelectedItems.Count <= _vm!.MaxSelected)
            {
                _vm.Selected = List.SelectedItems.Cast<PanelSelectModel>().ToList();
            }
            else
            {
                foreach (var item in e.AddedItems)
                {
                    List.SelectedItems.Remove(item);
                }
            }
        }
    }
}
