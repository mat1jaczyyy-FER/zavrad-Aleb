using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

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

            List<MenuItem> items = Items.OfType<MenuItem>().ToList();

            foreach (MenuItem item in items)
                item.IsVisible = !hide.Contains((string)item.Header);

            if (items.Any(i => i.IsVisible))
                base.Open(control);
        }
    }
}