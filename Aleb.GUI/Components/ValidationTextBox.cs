using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aleb.GUI.Components {
    public class ValidationTextBox: TextBox, IStyleable {
        Type IStyleable.StyleKey => typeof(TextBox);

        public Func<string, bool> Validator;
        IDisposable observable;

        public delegate void ValidatedEventHandler(bool state);
        public event ValidatedEventHandler Validated;
        
        public delegate void ReturnEventHandler();
        public event ReturnEventHandler Return;

        public ValidationTextBox() {
            observable = this.GetObservable(TextProperty).Subscribe(x => {
                bool state = Validator?.Invoke(x) != false;

                Foreground = (IBrush)Application.Current.Styles.FindResource(state? "ThemeForegroundBrush" : "ErrorBrush");

                Validated?.Invoke(state);
            });

            DetachedFromVisualTree += (_, __) => observable?.Dispose();

            KeyUp += (_, e) => {
                if (e.Key == Key.Return) Return?.Invoke();
            };
        }
    }
}
