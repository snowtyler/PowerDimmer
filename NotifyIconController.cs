using System;
using System.Drawing;
using System.Windows.Forms;

using System.Collections.Generic;
using System.Reflection;

namespace PowerDimmer
{
    public class NotifyIconController
    {
        internal Action? ExitClicked;
        public NotifyIcon NotifyIcon;
        private readonly TrayMenuWindow trayMenuWindow;

        public NotifyIconController(ISettings settings)
        {
            NotifyIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)!,
                Text = "PowerDimmer",
                Visible = true
            };

            trayMenuWindow = new TrayMenuWindow(settings);
            trayMenuWindow.ExitRequested += () =>
            {
                trayMenuWindow.AllowClose();
                ExitClicked?.Invoke();
            };

            NotifyIcon.MouseUp += (_, e) =>
            {
                if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
                {
                    return;
                }

                if (trayMenuWindow.IsVisible)
                {
                    trayMenuWindow.Hide();
                    return;
                }

                trayMenuWindow.ShowAtCursor();
            };
        }
    }

    // https://stackoverflow.com/a/24825487
    public class TrackBarWithoutFocus : TrackBar
    {
        private const int WM_SETFOCUS = 0x0007;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SETFOCUS)
            {
                return;
            }

            base.WndProc(ref m);
        }
    }

    // https://stackoverflow.com/questions/4339143/adding-a-trackbar-control-to-a-contextmenu
    public class TrackBarMenuItem : ToolStripControlHost
    {
        private TrackBar trackBar;

        public TrackBarMenuItem(ISettings settings) : base(new ContainerControl())
        {
            BackColor = TrayMenuTheme.Background;
            ForeColor = TrayMenuTheme.Foreground;
            Padding = new Padding(0);

            Control.BackColor = TrayMenuTheme.Background;
            Control.ForeColor = TrayMenuTheme.Foreground;
            Control.Padding = new Padding(8, 6, 8, 8);
            Control.MinimumSize = new Size(220, 72);
            Control.Size = Control.MinimumSize;
            var contentWidth = Control.MinimumSize.Width - Control.Padding.Horizontal;

            var brightnessLabel = new Label()
            {
                Parent = Control,
                Text = "Brightness",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = TrayMenuTheme.FontBold,
                ForeColor = TrayMenuTheme.Foreground,
                AutoSize = false,
                Height = 18,
                Left = Control.Padding.Left,
                Width = contentWidth
            };

            trackBar = new TrackBarWithoutFocus
            {
                Parent = Control,
                Top = 24,
                Left = Control.Padding.Left,
                Width = contentWidth,
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 1,
                SmallChange = 5,
                LargeChange = 20,
                TickStyle = TickStyle.None,
                Value = settings.Brightness,
                BackColor = TrayMenuTheme.Background
            };
            // Hack to restore hover-highlights after interacting
            // with trackbar
            trackBar.Click += (_, _) => Parent?.Focus();

            var valueBox = new TextBox()
            {
                Parent = trackBar,
                Top = 28,
                Left = 1,
                Enabled = false,
                BackColor = TrayMenuTheme.Background,
                ForeColor = TrayMenuTheme.Foreground,
                Font = TrayMenuTheme.Font,
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.None,
                Text = settings.Brightness.ToString()
            };

            trackBar.ValueChanged += (o, s) =>
                {
                    // invert for "brightness" value
                    settings.Brightness = trackBar.Value;
                    valueBox.Text = trackBar.Value.ToString();
                };
        }
    }
}