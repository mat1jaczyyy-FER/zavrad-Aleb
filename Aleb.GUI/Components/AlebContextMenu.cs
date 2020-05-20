using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aleb.GUI.Components {
    public class AlebContextMenu: ContextMenu, IStyleable {
        Type IStyleable.StyleKey => typeof(ContextMenu);

        public delegate void MenuActionEventHandler(string action);
        public event MenuActionEventHandler MenuAction;

        public AlebContextMenu() {
            AvaloniaXamlLoader.Load(this);

            this.AddHandler(MenuItem.ClickEvent, Selected);
        }

        void Selected(object sender, RoutedEventArgs e) {
            if (e.Source is MenuItem menuItem)
                MenuAction?.Invoke((string)menuItem.Header);
        }

        public void Open(Control control, string[] hide = null) {
            hide = hide?? new string[0];

            foreach (MenuItem item in Items.OfType<MenuItem>())
                item.IsVisible = !hide.Contains((string)item.Header);

            base.Open((Window)control.GetVisualRoot());
        }
    }
}