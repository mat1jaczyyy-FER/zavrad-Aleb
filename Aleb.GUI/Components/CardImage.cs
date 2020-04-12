using Avalonia.Controls;

namespace Aleb.GUI.Components {
    public class CardImage: Image {
        public CardImage(int card)
            => Source = App.GetImage($"Cards/{card}");
    }
}
