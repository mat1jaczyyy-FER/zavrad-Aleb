using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Aleb.GUI.Components {
    public class CardImage: Image {
        public CardImage(int card)
            => Source = new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>().Open(new Uri($"avares://Aleb.GUI/Images/{card}.png")));
    }
}
