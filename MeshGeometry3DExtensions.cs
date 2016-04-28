namespace ThePriceIsAName
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    public static class MeshGeometry3DExtensions
    {
        public static void ParseTriangleIndices(this MeshGeometry3D geometry, string input)
        {
            var indicies = new Int32Collection();
            foreach (var i in input.Split(new[] {' ', ',' }))
            {
                indicies.Add(int.Parse(i));
            }
            geometry.TriangleIndices = indicies;
        }
        public static void ParsePositions(this MeshGeometry3D geometry, string input)
        {
            var positions = new Point3DCollection();
            foreach (var p in input.Split(' '))
            {
                var xyz = p.Split(',');
                positions.Add(new Point3D(double.Parse(xyz[0]),double.Parse(xyz[1]),double.Parse(xyz[2])));
            }
            geometry.Positions = positions;
        }
    }
}
