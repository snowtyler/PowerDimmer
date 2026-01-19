using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ModernNotifyIcon.Theme;

namespace PowerDimmer
{
    // from: https://github.com/Sharp0802/ModernNotifyIcon
    public class NotifyIconBuilder
    {
        private ContextMenuStripBuilder StripBuilder { get; } = new();

        protected NotifyIconBuilder()
        {
        }

        public static NotifyIconBuilder Create()
        {
            return new NotifyIconBuilder();
        }

        public NotifyIconBuilder Configure(Action<ContextMenuStripBuilder> builder)
        {
            builder(StripBuilder);
            return this;
        }

        public NotifyIcon Build(Icon icon)
        {
            return new NotifyIcon
            {
                Icon = icon,
                ContextMenuStrip = StripBuilder.Build()
            };
        }
    }

    public class ContextMenuStripBuilder
    {
        public List<ToolStripItem> Items { get; } = new();

        public ContextMenuStripBuilder AddItem(ToolStripItem item)
        {
            Items.Add(item);
            return this;
        }

        public ContextMenuStripBuilder AddText(string text) => AddItem(new ToolStripMenuItem(text));

        public ContextMenuStripBuilder AddSeparator() => AddItem(new ToolStripSeparator { Margin = new Padding(0, 2, 0, 2) });

        public ContextMenuStripBuilder AddToggle(Action<ToggleGenerateOption> option)
        {
            var toggle = new ToolStripMenuItem();
            var optionRef = new ToggleGenerateOption(toggle);
            option.Invoke(optionRef);
            toggle.Text = optionRef.Text;
            toggle.Checked = optionRef.Checked;
            toggle.Click += (_, _) => optionRef.InvokeHandlers(toggle.Checked = !toggle.Checked);
            return AddItem(toggle);
        }

        public ContextMenuStripBuilder AddButton(Action<ButtonGenerateOption> option)
        {
            var button = new ToolStripMenuItem();
            var optionRef = new ButtonGenerateOption(button);
            option.Invoke(optionRef);
            button.Text = optionRef.Text;
            button.Click += (_, _) => optionRef.InvokeHandlers();
            return AddItem(button);
        }

        public ContextMenuStripBuilder AddSubmenu(string text, Action<ContextMenuStripBuilder> option)
        {
            var optionRef = new ContextMenuStripBuilder();
            option.Invoke(optionRef);
            var button = new ToolStripMenuItem(text)
            {
                DropDown = optionRef.Build()
            };
            return AddItem(button);
        }

        public ThemeReferencedContextMenuStrip Build()
        {
            const int itemVerticalPadding = 6;
            const int itemHorizontalPadding = 12;
            const int edgePadding = 6;

            var strip = new ThemeReferencedContextMenuStrip
            {
                Spacing = 4,
                BackColor = TrayMenuTheme.Background,
                ForeColor = TrayMenuTheme.Foreground,
                Font = TrayMenuTheme.Font,
                ShowImageMargin = false,
                ShowCheckMargin = true,
                Padding = new Padding(edgePadding, edgePadding, edgePadding, edgePadding),
                RenderMode = ToolStripRenderMode.Professional,
                Renderer = new ToolStripProfessionalRenderer(new ModernMenuColorTable())
            };

            var array = Items.ToArray();
            for (var i = 0; i < array.Length; ++i)
            {
                array[i].Padding = new Padding(itemHorizontalPadding, itemVerticalPadding, itemHorizontalPadding, itemVerticalPadding);
                array[i].Margin = new Padding(0, i == 0 ? 2 : 0, 0, i == array.Length - 1 ? 2 : 0);
                array[i].BackColor = TrayMenuTheme.Background;
                array[i].ForeColor = TrayMenuTheme.Foreground;

                if (array[i] is ToolStripMenuItem menuItem)
                {
                    menuItem.Font = TrayMenuTheme.Font;
                    menuItem.ImageScaling = ToolStripItemImageScaling.None;
                }
                else if (array[i] is ToolStripSeparator separator)
                {
                    separator.Margin = new Padding(0, 6, 0, 6);
                }
            }

            strip.Items.AddRange(array);
            return strip;
        }
    }

    internal static class TrayMenuTheme
    {
        public static readonly Color Background = Color.FromArgb(28, 28, 30);
        public static readonly Color Foreground = Color.FromArgb(240, 240, 240);
        public static readonly Color DisabledForeground = Color.FromArgb(140, 140, 140);
        public static readonly Color Border = Color.FromArgb(50, 50, 54);
        public static readonly Color Separator = Color.FromArgb(60, 60, 64);
        public static readonly Color Highlight = Color.FromArgb(45, 45, 50);
        public static readonly Color HighlightBorder = Color.FromArgb(72, 72, 78);
        public static readonly Color Accent = Color.FromArgb(99, 176, 255);

        public static readonly Font Font = ResolveFont(9.5f, FontStyle.Regular);
        public static readonly Font FontBold = ResolveFont(9.5f, FontStyle.Bold);

        private static Font ResolveFont(float size, FontStyle style)
        {
            try
            {
                return new Font("Segoe UI Variable Text", size, style);
            }
            catch
            {
                var fallback = SystemFonts.MessageBoxFont;
                if (fallback != null)
                {
                    return new Font(fallback.FontFamily, fallback.Size, style);
                }

                return new Font(FontFamily.GenericSansSerif, size, style);
            }
        }
    }

    internal sealed class ModernMenuColorTable : ProfessionalColorTable
    {
        public ModernMenuColorTable()
        {
            UseSystemColors = false;
        }

        public override Color ToolStripDropDownBackground => TrayMenuTheme.Background;
        public override Color MenuBorder => TrayMenuTheme.Border;
        public override Color MenuItemBorder => TrayMenuTheme.HighlightBorder;
        public override Color MenuItemSelected => TrayMenuTheme.Highlight;
        public override Color MenuItemSelectedGradientBegin => TrayMenuTheme.Highlight;
        public override Color MenuItemSelectedGradientEnd => TrayMenuTheme.Highlight;
        public override Color MenuItemPressedGradientBegin => TrayMenuTheme.Highlight;
        public override Color MenuItemPressedGradientEnd => TrayMenuTheme.Highlight;
        public override Color MenuStripGradientBegin => TrayMenuTheme.Background;
        public override Color MenuStripGradientEnd => TrayMenuTheme.Background;
        public override Color ImageMarginGradientBegin => TrayMenuTheme.Background;
        public override Color ImageMarginGradientMiddle => TrayMenuTheme.Background;
        public override Color ImageMarginGradientEnd => TrayMenuTheme.Background;
        public override Color SeparatorDark => TrayMenuTheme.Separator;
        public override Color SeparatorLight => TrayMenuTheme.Separator;
        public override Color CheckBackground => TrayMenuTheme.Highlight;
        public override Color CheckPressedBackground => TrayMenuTheme.Highlight;
        public override Color CheckSelectedBackground => TrayMenuTheme.Highlight;
        public override Color ImageMarginRevealedGradientBegin => TrayMenuTheme.Background;
        public override Color ImageMarginRevealedGradientMiddle => TrayMenuTheme.Background;
        public override Color ImageMarginRevealedGradientEnd => TrayMenuTheme.Background;
    }
    public class GenerateOption<T> where T : GenerateOption<T>
    {
        public delegate void ConfigureItemHandler(ToolStripMenuItem item);

        public ToolStripMenuItem Item { get; private set; }

        public GenerateOption(ToolStripMenuItem item)
        {
            Item = item;
        }

        public T ConfigureItem(ConfigureItemHandler handler)
        {
            handler(Item);
            return (T)this;
        }
    }

    public sealed class ButtonGenerateOption : GenerateOption<ButtonGenerateOption>
    {
        public string? Text { get; private set; }

        public event Action? Toggled;
        public ButtonGenerateOption(ToolStripMenuItem item) : base(item)
        {
        }

        public ButtonGenerateOption SetText(string text)
        {
            Text = text;
            return this;
        }

        public ButtonGenerateOption AddHandler(Action handler)
        {
            Toggled += handler;
            return this;
        }

        internal void InvokeHandlers()
        {
            Toggled?.Invoke();
        }
    }

    public sealed class ToggleGenerateOption : GenerateOption<ToggleGenerateOption>
    {
        public delegate void ToggleEventHandler(bool toggled);

        public string? Text { get; private set; }

        public bool Checked { get; private set; } = false;


        public event ToggleEventHandler? Toggled;

        public ToggleGenerateOption(ToolStripMenuItem item) : base(item)
        {
        }

        public ToggleGenerateOption SetText(string text)
        {
            Text = text;
            return this;
        }

        public ToggleGenerateOption SetChecked(bool _checked)
        {
            Checked = _checked;
            return this;
        }

        public ToggleGenerateOption AddHandler(ToggleEventHandler handler)
        {
            Toggled += handler;
            return this;
        }

        internal void InvokeHandlers(bool check)
        {
            Toggled?.Invoke(check);
        }
    }
}