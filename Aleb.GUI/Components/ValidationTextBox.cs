using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aleb.GUI.Components {
    public class ValidationTextBox: TextBox, IStyleable {
        Type IStyleable.StyleKey => typeof(TextBox);

        Func<string, bool> _validator;
        public Func<string, bool> Validator {
            get => _validator;
            set {
                _validator = value;
                Validate(Text);
            }
        }

        IDisposable observable;

        public delegate void ValidatedEventHandler(bool state);
        public event ValidatedEventHandler Validated;
        
        public void Validate(string text) {
            if (text == null) return;

            bool state = Validator?.Invoke(text) != false;

            Foreground = App.GetResource<IBrush>(state? "ThemeForegroundBrush" : "ErrorBrush");

            Validated?.Invoke(state);
        }
        
        public delegate void ReturnEventHandler();
        public event ReturnEventHandler Return;

        public ValidationTextBox() {
            observable = this.GetObservable(TextProperty).Subscribe(Validate);

            DetachedFromVisualTree += (_, __) => observable?.Dispose();

            KeyUp += (_, e) => {
                if (e.Key == Key.Return) Return?.Invoke();
            };

            Text = "";
        }
    }
}
