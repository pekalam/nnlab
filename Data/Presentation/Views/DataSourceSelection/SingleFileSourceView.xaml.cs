using System.Windows.Controls;
using CommonServiceLocator;
using Data.Application.Views;

namespace Data.Presentation.Views
{
    /// <summary>
    /// Interaction logic for SingleFileValidationViewPart.xaml
    /// </summary>
    public partial class SingleFileSourceView : UserControl, ISingleFileSourceView
    {
        public SingleFileSourceView()
        {
            InitializeComponent();
        }
    }
}
