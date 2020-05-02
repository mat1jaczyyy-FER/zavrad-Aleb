using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Start: CircleButton {
        public Start(): base(new Path() {
            StrokeThickness = 4,
            Data = Geometry.Parse("M 20,18 L 20,32 32,25 Z")
        }, true) {}
    }
}
