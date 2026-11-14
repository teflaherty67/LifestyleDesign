using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace LifestyleDesign
{
    /// <summary>
    /// The kind of message being shown. Controls the icon and its color.
    /// </summary>
    public enum MessageKind { Information, Warning, Error }

    /// <summary>
    /// Styled replacement for MessageBox and the Revit TaskDialog.
    /// Call it through the helpers in Common.Utils rather than directly.
    /// </summary>
    public partial class frmMessage : Window
    {
        public frmMessage(string title, string message, MessageKind kind, bool showCancel)
        {
            InitializeComponent();

            tbkTitle.Text = title;
            tbkMessage.Text = message;

            // set the icon glyph and color to match the kind of message
            switch (kind)
            {
                case MessageKind.Warning:
                    tbkIcon.Text = "!";
                    ellIcon.Fill = (System.Windows.Media.Brush)FindResource("WarningBrush");
                    break;

                case MessageKind.Error:
                    tbkIcon.Text = "×";
                    ellIcon.Fill = (System.Windows.Media.Brush)FindResource("ErrorBrush");
                    break;

                default:
                    tbkIcon.Text = "i";
                    ellIcon.Fill = (System.Windows.Media.Brush)FindResource("InformationBrush");
                    break;
            }

            if (showCancel)
            {
                // confirmation: OK and Cancel
                btnCancel.Visibility = Visibility.Visible;
            }
            else
            {
                // notification: a single Close button that answers to both Enter and Esc
                btnCancel.Visibility = Visibility.Collapsed;
                btnOK.Content = "Close";
                btnOK.IsCancel = true;
            }

            Loaded += (s, e) => btnOK.Focus();
        }

        /// <summary>
        /// Shows the dialog on top of Revit and waits for the user.
        /// </summary>
        /// <returns>True if the user clicked OK or Close, false if they cancelled.</returns>
        internal static bool ShowMessage(string title, string message, MessageKind kind, bool showCancel)
        {
            frmMessage curForm = new frmMessage(title, message, kind, showCancel);

            // make Revit the owner so the dialog cannot fall behind it
            IntPtr revitHandle = Process.GetCurrentProcess().MainWindowHandle;

            if (revitHandle != IntPtr.Zero)
            {
                new WindowInteropHelper(curForm).Owner = revitHandle;
                curForm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            return curForm.ShowDialog() == true;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
