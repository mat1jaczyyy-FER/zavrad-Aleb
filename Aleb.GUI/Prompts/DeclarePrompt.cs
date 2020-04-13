using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

using Aleb.Client;
using Aleb.Common;
using Aleb.GUI.Components;
using Aleb.GUI.Views;

namespace Aleb.GUI.Prompts {
    public class DeclarePrompt: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        bool[] Declared;

        public DeclarePrompt() => throw new InvalidOperationException();

        public DeclarePrompt(bool[] declared) {
            InitializeComponent();

            Declared = declared;
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Skip(object sender, RoutedEventArgs e) 
            => Requests.Declare(null);

        void Call(object sender, RoutedEventArgs e)
            => Requests.Declare(Declared.Select((x, i) => (x, i)).Where(t => t.x).Select(t => t.i).ToList());
    }
}
