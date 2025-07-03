using System.Windows;
using System.Windows.Input;

namespace QuickEditor
{
    /// <summary>
    /// Custom confirmation dialog matching the editor's dark theme
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of the ConfirmDialog
        /// </summary>
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the ConfirmDialog with custom message
        /// </summary>
        /// <param name="message">The message to display in the dialog</param>
        public ConfirmDialog(string message) : this()
        {
            MessageText.Text = message;
        }

        /// <summary>
        /// Handle Yes button click - sets dialog result to true and closes
        /// </summary>
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handle No button click - sets dialog result to false and closes
        /// </summary>
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handle keyboard input for Space and Enter keys
        /// </summary>
        private void ConfirmDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                // Space or Enter triggers the default Yes button
                DialogResult = true;
                Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Shows the confirmation dialog and returns the result
        /// </summary>
        /// <param name="owner">The owner window</param>
        /// <param name="message">The message to display</param>
        /// <returns>True if Yes was clicked, false if No was clicked</returns>
        public static bool ShowConfirmation(Window owner, string message)
        {
            var dialog = new ConfirmDialog(message)
            {
                Owner = owner
            };
            
            return dialog.ShowDialog() == true;
        }
    }
}