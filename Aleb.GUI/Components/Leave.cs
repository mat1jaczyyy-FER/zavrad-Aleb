using System.Collections.Generic;

using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Aleb.GUI.Components {
    public class Leave: CircleButton {
        public Leave(): base(new List<Path>() {
            new Path() {
                StrokeThickness = 1,
                Data = Geometry.Parse("M 25,16 L 33,18 33,32 25,34 Z M 23,17 L 21,17 Z M 21,17 L 21,20.5 Z M 21,29.5 L 21,33 Z M 21,33 L 23,33 Z M 22,22.5 L 18,22.5 18,20.5 14,25 18,29.5 18,27.5 22,27.5 Z")
            }
        }, true) {}
    }
}
