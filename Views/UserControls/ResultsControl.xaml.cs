using System.Windows.Controls;
using LessonScheduler.ViewModels;

namespace LessonScheduler.Views.UserControls
{
    public partial class ResultsControl : UserControl
    {
        public ResultsControl()
        {
            InitializeComponent();
        }

        public void SetMode(string mode)
        {
            if (DataContext is ResultsViewModel viewModel)
            {
                viewModel.Mode = mode;
            }
        }
    }
}