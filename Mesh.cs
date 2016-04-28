namespace ThePriceIsAName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    public class Mesh
    {
        public Int32Collection TriangleIndices { get; set; }
        public Point3DCollection Positions { get; set; }
    }
    [ValueConversion(typeof(Mesh), typeof(MeshGeometry3D))]
    public class MeshConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mesh = new MeshGeometry3D();
            mesh.Positions = ((Mesh)value).Positions;
            mesh.TriangleIndices = ((Mesh)value).TriangleIndices;
            return mesh;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mesh = new Mesh();
            mesh.Positions = ((MeshGeometry3D)value).Positions;
            mesh.TriangleIndices = ((MeshGeometry3D)value).TriangleIndices;
            return mesh;
        }
    }
}
