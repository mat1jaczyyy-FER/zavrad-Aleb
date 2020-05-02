using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Ready: CircleButton {
        static Path Yes => new Path() {
            StrokeThickness = 4,
            Data = Geometry.Parse("M 16,25 L 21,32 22,32 34,18")
        };

        static Path No => new Path() {
            StrokeThickness = 4,
            Data = Geometry.Parse("M 17,17 L 33,33 M 17,33 33,17")
        };

        public Ready(): base(Yes) {}

        public bool State {
            set => SetIllustration(value? No : Yes);
        }
    }
}
