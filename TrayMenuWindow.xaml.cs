using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace PowerDimmer
{
    public partial class TrayMenuWindow : Window
    {
        public event Action? ExitRequested;
        private bool allowClose;

        public TrayMenuWindow(ISettings settings)
        {
            InitializeComponent();
            DataContext = settings;
            Deactivated += (_, _) => Hide();
        }

        public void ShowAtCursor()
        {
            if (!IsVisible)
            {
                Show();
            }
            UpdateLayout();
            PositionAtCursor();
            Activate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            base.OnClosing(e);
        }

        private void PositionAtCursor()
        {
            var cursor = System.Windows.Forms.Cursor.Position;
            var dpi = VisualTreeHelper.GetDpi(this);
            var cursorX = cursor.X / dpi.DpiScaleX;
            var cursorY = cursor.Y / dpi.DpiScaleY;

            var windowWidth = ActualWidth > 1 ? ActualWidth : Width;
            var windowHeight = ActualHeight > 1 ? ActualHeight : Height;
            if (double.IsNaN(windowWidth) || windowWidth <= 1)
            {
                windowWidth = 340;
            }
            if (double.IsNaN(windowHeight) || windowHeight <= 1)
            {
                windowHeight = 300;
            }

            var desiredLeft = cursorX - windowWidth + 24;
            var desiredTop = cursorY - windowHeight - 12;

            var workArea = SystemParameters.WorkArea;
            Left = Math.Max(workArea.Left, Math.Min(desiredLeft, workArea.Right - windowWidth));
            Top = Math.Max(workArea.Top, Math.Min(desiredTop, workArea.Bottom - windowHeight));
        }

        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            ExitRequested?.Invoke();
        }

        public void AllowClose()
        {
            allowClose = true;
        }
    }
}
