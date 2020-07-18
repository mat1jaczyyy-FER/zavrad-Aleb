using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace Aleb.GUI.Components {
    public class RoundRow: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            IEnumerable<IControl> items = this.Get<Grid>("Root").Children.OfType<Border>().Select(i => i.Child);

            Cells = items.OfType<TextBlock>().ToList();
            Image = items.OfType<Image>().First();
        }
        
        List<TextBlock> Cells;
        Image Image;

        public bool Header {
            set => Dispatcher.UIThread.InvokeAsync(() => {
                Background = value
                    ? (IBrush)Application.Current.FindResource("ThemeForegroundBrush")
                    : null;

                Foreground = value
                    ? (IBrush)Application.Current.FindResource("ThemeBorderLowBrush")
                    : null;

                FontWeight = FontWeight.Bold;

            }, DispatcherPriority.MinValue);
        }

        public HorizontalAlignment Alignment {
            get => Cells[0].HorizontalAlignment;
            set => Cells.ForEach(i => i.HorizontalAlignment = value);
        }

        public string Legend {
            get => Cells[0].Text;
            set {
                Cells[0].Text = value;
                Cells[0].Parent.IsVisible = Cells[0].Text != "";
            }
        }

        public string Icon {
            set {
                Image.Source = (value?? "") != ""? App.GetImage(value) : null;
                Image.Parent.IsVisible = (value?? "") != "";
            }
        }

        public string Left { 
            get => Cells[1].Text;
            set => Cells[1].Text = value != ""? value : " ";
        }

        public string Right { 
            get => Cells[2].Text;
            set => Cells[2].Text = value != ""? value : " ";
        }

        public RoundRow() {
            InitializeComponent();
        }
    }
}