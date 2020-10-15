using System;
using System.Threading.Tasks;
using NNLib;
using System.Windows;
using System.Windows.Threading;
using MathNet.Numerics.LinearAlgebra;
using NNLib.ActivationFunction;
using NNLib.MLP;
using SharedUI.MatrixPreview;

namespace MatrixPreviewTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var net = new MLPNetwork(new PerceptronLayer[]
            {
                new PerceptronLayer(1, 8, new LinearActivationFunction()),
                new PerceptronLayer(8, 16, new LinearActivationFunction()),
                new PerceptronLayer(16, 2, new LinearActivationFunction()),
            });
            InitializeComponent();
            DataContext = Vm;
            Vm.CanRemoveItem = true;

            var mat = Matrix<double>.Build.Dense(10, 2);
            Vm.Controller.AssignMatrix(mat, new []{"x", "y"}, i => i.ToString());

            // Vm.Controller.AssignNetwork(net);
            //
            // Task.Run(async () =>
            // {
            //     var rnd = new Random();
            //     while (true)
            //     {
            //         for (int i = 0; i < net.Layers.Count; i++)
            //         {
            //             for (int j = 0; j < net.Layers[i].NeuronsCount; j++)
            //             {
            //                 for (int k = 0; k < net.Layers[i].InputsCount; k++)
            //                 {
            //                     net.Layers[i].Weights[j, k] = rnd.NextDouble();
            //                 }
            //             }
            //         }
            //
            //         Vm.Controller.Update();
            //         Vm.Controller.ApplyUpdate();
            //
            //         await Task.Delay(100);
            //     }
            // });
        }

        public MatrixPreviewViewModel Vm { get; set; } = new MatrixPreviewViewModel();
    }
}
