namespace ThePriceIsAName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    public class Torus : ModelVisual3D
    {
        MeshGeometry3D d;
        public Torus()
        {
            this.Content = new GeometryModel3D();
            var a = new MeshGeometry3D();
            a.TriangleIndices = new Int32Collection();
            a.Positions = new Point3DCollection();
        }
    }
}
