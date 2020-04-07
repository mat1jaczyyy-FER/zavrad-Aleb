using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Aleb.GUI {
    class AlebWindow: Window {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            TitleText = this.Get<TextBlock>("Title");
            TitleCenter = this.Get<TextBlock>("TitleCenter");
            
            CenteringLeft = this.Get<StackPanel>("CenteringLeft");
            CenteringRight = this.Get<StackPanel>("CenteringRight");
        }

        TextBlock TitleText, TitleCenter;
        StackPanel CenteringLeft, CenteringRight;

        public AlebWindow() {
            InitializeComponent();
            #if DEBUG
                this.AttachDevTools();
            #endif
        }

        void Loaded(object sender, EventArgs e) {
            Position = new PixelPoint(Position.X, Math.Max(0, Position.Y));


        }

        void Unloaded(object sender, CancelEventArgs e) {

        }

        void Window_KeyDown(object sender, KeyEventArgs e) {
            List<Window> windows = App.Windows.ToList();
            

            
            if (windows.SequenceEqual(App.Windows) && FocusManager.Instance.Current?.GetType() != typeof(TextBox))
                this.Focus();
        }

        void BringToTop() {
            Topmost = false;
            Topmost = true;
            Activate();
        }

        void Window_Focus(object sender, PointerPressedEventArgs e) => Focus();

        void MoveWindow(object sender, PointerPressedEventArgs e) {
            if (e.ClickCount == 2) Maximize(null);
            else BeginMoveDrag(e);

            BringToTop();
        }
        
        void Minimize() => WindowState = WindowState.Minimized;

        void Maximize(PointerEventArgs e) {
            WindowState = (WindowState == WindowState.Maximized)? WindowState.Normal : WindowState.Maximized;
            
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
    }
}