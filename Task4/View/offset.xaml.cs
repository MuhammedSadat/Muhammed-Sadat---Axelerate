using System;
using System.Windows;
using System.Windows.Input;
using Task4.RevitSystem;

namespace Task4.View
{
    /// <summary>
    /// Interaction logic for offset.xaml
    /// </summary>
    public partial class offset : Window
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the offset class.
        /// </summary>
        public offset()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        #endregion

        #region Event Handlers

        #region Window_MouseDown
        /// <summary>
        /// Handles the mouse down event to enable window dragging.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Step 1.1: Check if the left mouse button is pressed
            if (e.ChangedButton == MouseButton.Left)
            {
                // Step 1.2: Enable window dragging
                this.DragMove();
            }
        }
        #endregion

        #region Confirm_btn_MouseDown
        /// <summary>
        /// Handles the mouse down event for the Confirm button.
        /// </summary>
        private void Confirm_btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Step 2.1: Get the text from the TextBox
            string input = Offset_txt.Text;

            // Step 2.2: Try to parse the input to a number
            if (double.TryParse(input, out _))
            {
                // Step 2.3: If parsing is successful, continue with the processing
                App.offsetNum = double.Parse(input);
                App.IfWpfOpened = true;
                this.Close();
            }
            else
            {
                // Step 2.4: If parsing fails, show a message box
                MessageBox.Show("Please enter numbers only.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #endregion
    }
}
