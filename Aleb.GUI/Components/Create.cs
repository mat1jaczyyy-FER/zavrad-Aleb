using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Create: CircleButton {
        public Create(): base(new Path() {
            StrokeThickness = 4,
            Data = Geometry.Parse("M 16,25 L 34,25 M 25,16 L 25,34")
        }) {}
    }
}
