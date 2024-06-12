using Autodesk.Revit.UI;
using Axelerate.MVVM.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;

namespace Axelerate.MVVM.View
{
    /// <summary>
    /// Interaction logic for MainUi.xaml
    /// </summary>
    public partial class MainUi : Window
    {
 
        #region Fields

        private readonly ExternalCommandData _commandData;
        private MainUiViewModel _viewModel;
        #endregion

        #region Constructors
        public MainUi()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public MainUi(ExternalCommandData commandData)
        {
            InitializeComponent();
            _commandData = commandData;
            DataContext = new MainUiViewModel(commandData);
        }

        #endregion

        #region Window Movment 
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        #endregion

        #region DataGrid Control
        private void linesDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteFromClipboard();
            }

       
                if (e.Key == Key.Delete)
                {
                    var grid = sender as DataGrid;
                    if (grid != null && grid.SelectedItem != null)
                    {
                        var selectedLine = grid.SelectedItem as DynamicLine;
                        if (selectedLine != null)
                        {
                            var viewModel = DataContext as MainUiViewModel;
                            if (viewModel != null)
                            {
                                viewModel.Lines.Remove(selectedLine);
                            }
                        }
                    }
                }

        }

        private void PasteFromClipboard()
        {
            // Get clipboard data
            string clipboardData = Clipboard.GetText();

            if (string.IsNullOrEmpty(clipboardData))
                return;

            // Split the clipboard data into lines
            string[] lines = clipboardData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Split each line into columns
                string[] columns = line.Split('\t');

                if (columns.Length >= 4)
                {
                    if (double.TryParse(columns[0], out double x1) &&
                        double.TryParse(columns[1], out double y1) &&
                        double.TryParse(columns[2], out double x2) &&
                        double.TryParse(columns[3], out double y2))
                    {
                        // Create a new DynamicLine and add it to the collection
                        var dynamicLine = new DynamicLine
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2
                        };

                        var viewModel = DataContext as MainUiViewModel;
                        viewModel?.Lines.Add(dynamicLine);
                    }
                }
            }
        }

        #endregion

    }
}
