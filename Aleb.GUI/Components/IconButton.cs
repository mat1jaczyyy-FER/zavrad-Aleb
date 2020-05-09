using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public abstract class IconButton: UserControl {
        public delegate void ClickedEventHandler();
        public event ClickedEventHandler Clicked;

        protected abstract IBrush Fill { get; set; }

        protected bool AllowRightClick = false;

        bool _enabled = true;
        public bool Enabled {
            get => _enabled;
            set {
                _enabled = value;

                mouseHeld = false;

                Fill = (IBrush)Application.Current.Styles.FindResource(Enabled
                    ? (mouseOver
                        ? OverBrush
                        : EnabledBrush
                    ) : DisabledBrush
                );
            }
        }

        string EnabledBrush, DisabledBrush, OverBrush, DownBrush;

        protected IconButton(string enabled = "ThemeButtonEnabledBrush", string disabled = "ThemeButtonDisabledBrush", string over = "ThemeButtonOverBrush", string down = "ThemeButtonDownBrush") {
            EnabledBrush = enabled;
            DisabledBrush = disabled;
            OverBrush = over;
            DownBrush = down;
        }

        protected virtual void Unloaded(object sender, VisualTreeAttachmentEventArgs e) => Clicked = null;

        bool mouseHeld = false;
        bool mouseOver = false;

        protected void MouseEnter(object sender, PointerEventArgs e) {
            if (Enabled) Fill = (IBrush)Application.Current.Styles.FindResource(mouseHeld? DownBrush : OverBrush);
            mouseOver = true;
        }

        protected void MouseLeave(object sender, PointerEventArgs e) {
            if (Enabled) Fill = (IBrush)Application.Current.Styles.FindResource(EnabledBrush);
            mouseOver = false;
        }

        protected void MouseDown(object sender, PointerPressedEventArgs e) {
            PointerUpdateKind MouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind;

            e.Pointer.Capture(null);

            if (MouseButton == PointerUpdateKind.LeftButtonPressed || (AllowRightClick && MouseButton == PointerUpdateKind.RightButtonPressed)) {
                mouseHeld = true;

                if (Enabled) Fill = (IBrush)Application.Current.Styles.FindResource(DownBrush);
            }
        }

        protected void MouseUp(object sender, PointerReleasedEventArgs e) {
            PointerUpdateKind MouseButton = e.GetCurrentPoint(this).Properties.PointerUpdateKind;

            if (mouseHeld && (MouseButton == PointerUpdateKind.LeftButtonReleased || (AllowRightClick && MouseButton == PointerUpdateKind.RightButtonReleased))) {
                mouseHeld = false;

                MouseEnter(sender, null);

                if (Enabled) Click(e);
            }
        }

        protected virtual void Click(PointerReleasedEventArgs e) => Clicked?.Invoke();
    }
}
