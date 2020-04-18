using System;
using System.Timers;

namespace Aleb.Common {
    public class Courier<T>: Timer {
        public T Info { get; private set; }
        Action<T> Handler;

        public Courier(int time, T info, Action<T> handler) {
            AutoReset = false;

            Interval = time;
            Info = info;
            Handler = handler;

            Elapsed += Tick;
            Start();
        }

        void Tick(object sender, EventArgs e) {
            Courier<T> source = (Courier<T>)sender;
            source.Elapsed -= Tick;

            Handler?.Invoke(source.Info);
        }
    }
}