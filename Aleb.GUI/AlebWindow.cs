using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Aleb.GUI.Views;

namespace Aleb.GUI {
    class AlebWindow: Window {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            TitleText = this.Get<TextBlock>("Title");
            
            LeftHand = this.Get<StackPanel>("LeftHand");
            RightHand = this.Get<StackPanel>("RightHand");

            view = this.Get<Border>("View");
        }

        public TextBlock TitleText;
        public StackPanel LeftHand, RightHand;
        Border view;

        public IControl View {
            get => view.Child;
            set => view.Child = value;
        }

        public AlebWindow() {
            InitializeComponent();
            #if DEBUG
                this.AttachDevTools();
            #endif
        }

        void Loaded(object sender, EventArgs e) {
            Position = new PixelPoint(Position.X, Math.Max(0, Position.Y));

            View = new ConnectingView();
        }

        void Unloaded(object sender, CancelEventArgs e) {

        }

        void Window_KeyDown(object sender, KeyEventArgs e) {

        }

        void BringToTop() {
            Topmost = false;
            Topmost = true;
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