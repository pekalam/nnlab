using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NNControl;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.ViewModels;

namespace NeuralNetwork.Presentation.Views
{
    /// <summary>
    /// Interaction logic for NetDisplayView.xaml
    /// </summary>
    public partial class NetDisplayView : UserControl
    {
        public NetDisplayView()
        {
            InitializeComponent();
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(neuralNetworkControl);

            var n = neuralNetworkControl.Controller.FindNeuronAt((float) p.X, (float) p.Y);

            int ind = 0;

            foreach (var controllerLayer in neuralNetworkControl.Controller.Layers)
            {
                if(controllerLayer.Neurons.Contains(n)) break;
                ind++;
            }

            if (n == null)
            {
                (DataContext as NetDisplayViewModel)!.Controller.AreaClicked.Execute();
            }
            else
            {
                (DataContext as NetDisplayViewModel)!.Controller.NeuronClickCommand.Execute(ind);
            }
        }
    }
}
