using System;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;

using Aleb.GUI.Components;
using Aleb.GUI.Views;

namespace Aleb.GUI {
    class AlebWindow: Window {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            Root = this.Get<Grid>("Root");
            Canvas = this.Get<Canvas>("Canvas");
            ContentRoot = this.Get<Grid>("ContentRoot");

            TitleText = this.Get<TextBlock>("Title");
            PopupTitle = this.Get<TextBlock>("PopupTitle");
            
            PreferencesButton = this.Get<PreferencesButton>("PreferencesButton");
            PinButton = this.Get<PinButton>("PinButton");
            PopupClose = this.Get<Close>("PopupClose");

            view = this.Get<Border>("View");
            popup = this.Get<Border>("Popup");
            
            PopupContainer = this.Get<Grid>("PopupContainer");
        }

        Grid Root, ContentRoot;
        Canvas Canvas;

        TextBlock TitleText;
        public TextBlock PopupTitle;

        PreferencesButton PreferencesButton;
        PinButton PinButton;
        public Close PopupClose;

        Border view, popup;
        Grid PopupContainer;

        double VirtualWidth {
            set {
                Canvas.Width = value;
                ContentRoot.Width = value;
            }
        }

        double VirtualHeight {
            set {
                Canvas.Height = value;
                ContentRoot.Height = value;
            }
        }

        public Control View {
            get => (Control)view.Child;
            set => view.Child = value;
        }

        public Control Popup {
            get => (Control)popup.Child;
            set {
                popup.Child = value;

                PreferencesButton.Enabled = Popup == null;
                PinButton.Enabled = Popup == null;

                PopupClose.IsEnabled = true;
                PopupContainer.IsVisible = Popup != null;
            }
        }

        public new string Title {
            set => TitleText.Text = base.Title = value == ""? "Aleb" : $"Aleb - {value}";
        }

        public AlebWindow() {
            InitializeComponent();
            #if DEBUG
                this.AttachDevTools();
            #endif
            
            BringToTop();
        }

        IDisposable observable;

        void Loaded(object sender, EventArgs e) {
            Position = new PixelPoint(Position.X, Math.Max(0, Position.Y));

            View = new ConnectingView(true);

            observable = this.GetObservable(ClientSizeProperty).Subscribe(SizeUpdated);
        }

        void Unloaded(object sender, CancelEventArgs e) {
            observable?.Dispose();
        }

        void SizeUpdated(Size size) {
            double target = 960 / 540.0;
            double aspect = size.Width / size.Height;
            
            VirtualWidth = (aspect <= target)? 960 : (540 * aspect);
            VirtualHeight = (aspect >= target)? 540 : (960 / aspect);

            double scale = Math.Min(size.Width / 960, size.Height / 540);
            Root.RenderTransform = new ScaleTransform(scale, scale);
        }

        void Window_KeyDown(object sender, KeyEventArgs e) {

        }

        public void BringToTop() {
            Topmost = true;
            Topmost = Preferences.Topmost;
            Activate();
        }

        void MoveWindow(object sender, PointerPressedEventArgs e) {
            if (e.ClickCount == 2) Maximize(null);
            else BeginMoveDrag(e);

            BringToTop();
        }
        
        void Minimize() => WindowState = WindowState.Minimized;

        void Maximize(PointerEventArgs e) {
            WindowState = (WindowState == WindowState.Maximized)? WindowState.Normal : WindowState.Maximized;
            
            if (e?.KeyModifiers == App.ControlKey && WindowState == WindowState.Maximized) {
                Screen result = null;

                foreach (Screen screen in Screens.All)
                    if (screen.Bounds.Contains(Position)) {
                        result = screen;
                        break;
                    }

                if (result != null) {
                    Width = result.Bounds.Width;
                    Height = result.Bounds.Height;
                }
            }

            BringToTop();
        }

        void ResizeNorthWest(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.NorthWest, e);
        void ResizeNorth(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.North, e);
        void ResizeNorthEast(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.NorthEast, e);
        void ResizeWest(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.West, e);
        void ResizeEast(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.East, e);
        void ResizeSouthWest(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.SouthWest, e);
        void ResizeSouth(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.South, e);
        void ResizeSouthEast(object sender, PointerPressedEventArgs e) => BeginResizeDrag(WindowEdge.SouthEast, e);

        void ClosePopup() => Popup = null;
    }
}