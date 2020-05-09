using System;
using System.IO;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using Humanizer;
using Humanizer.Localisation;

namespace Aleb.GUI.Popups {
    public class PreferencesPopup: UserControl {
        void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            App.MainWindow.PopupTitle.Text = "Postavke";

            MiVi = this.Get<ComboBox>("MiVi");
            Notify = this.Get<ComboBox>("Notify");

            DiscordPresence = this.Get<CheckBox>("DiscordPresence");

            CurrentSession = this.Get<TextBlock>("CurrentSession");
            AllTime = this.Get<TextBlock>("AllTime");

            Version = this.Get<TextBlock>("Version");
        }

        ComboBox MiVi, Notify;
        CheckBox DiscordPresence;

        TextBlock CurrentSession, AllTime, Version;
        DispatcherTimer Timer;

        void UpdateTime(object sender, EventArgs e) {
            CurrentSession.Text = $"Trenutna sesija: {Program.TimeSpent.Elapsed.Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, culture: App.Culture)}";

            if (Preferences.Time >= (long)TimeSpan.MaxValue.TotalSeconds) Preferences.BaseTime = 0;

            AllTime.Text = $"Sveukupno: {Preferences.Time.Seconds().Humanize(minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, culture: App.Culture)}";
        }

        public PreferencesPopup() {
            InitializeComponent();

            Version.Text += Program.Version;

            if (App.AvaloniaVersion() != "")
                ToolTip.SetTip(Version, $"Avalonia {App.AvaloniaVersion()}");

            UpdateTime(null, EventArgs.Empty);
            Timer = new DispatcherTimer() {
                Interval = new TimeSpan(0, 0, 1)
            };
            Timer.Tick += UpdateTime;
            Timer.Start();

            DiscordPresence.IsChecked = Preferences.DiscordPresence;

            MiVi.SelectedIndex = Convert.ToInt32(Preferences.MiVi);
            Notify.SelectedIndex = Convert.ToInt32(Preferences.Notify);
        }

        void DiscordPresence_Changed(object sender, RoutedEventArgs e) => Preferences.DiscordPresence = DiscordPresence.IsChecked.Value;
        
        void MiVi_Changed(object sender, SelectionChangedEventArgs e) => Preferences.MiVi = MiVi.SelectedIndex != 0;
        void Notify_Changed(object sender, SelectionChangedEventArgs e) => Preferences.Notify = (Preferences.NotificationType)Notify.SelectedIndex;

        void OpenCrashesFolder(object sender, RoutedEventArgs e) {
            if (!Directory.Exists(Program.UserPath)) Directory.CreateDirectory(Program.UserPath);
            if (!Directory.Exists(Program.CrashDir)) Directory.CreateDirectory(Program.CrashDir);

            App.URL(Program.CrashDir);
        }

        void Loaded(object sender, VisualTreeAttachmentEventArgs e) {}

        void Unloaded(object sender, VisualTreeAttachmentEventArgs e) {
            Timer.Stop();
            Timer.Tick -= UpdateTime;
        }
    }
}
