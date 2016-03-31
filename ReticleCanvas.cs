namespace ThePriceIsAName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Media3D;

    public class ReticleCanvas : Canvas
    {
        public double Resolution
        {
            get { return (double)GetValue(ResolutionProperty); }
            set { SetValue(ResolutionProperty, value); }
        }
        public double Dimension
        {
            get { return (double)GetValue(DimensionProperty); }
            set { SetValue(DimensionProperty, value); }
        }
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        #region Dependency properties for Resolution and Dimension intended for data binding and visual draw on change.
        public static readonly DependencyProperty ResolutionProperty;
        public static readonly DependencyProperty DimensionProperty;
        public static readonly DependencyProperty RadiusProperty;
        public static readonly DependencyProperty GeometryProperty;

        static ReticleCanvas()
        {
            ReticleCanvas.GridGradient = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(Colors.DarkMagenta, 0.0), new GradientStop(Colors.DarkBlue, .25), new GradientStop(Colors.DarkMagenta, 0.75) }));
            ReticleCanvas.MeshGradient = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(Colors.DarkSeaGreen, 0.0), new GradientStop(Colors.DarkGoldenrod, .25), new GradientStop(Colors.DarkSeaGreen, 0.75) }));
            var dimensionMetadata = new FrameworkPropertyMetadata(OnDimensionChanged);
            var resolutionMetadata = new FrameworkPropertyMetadata(OnResolutionChanged);
            var radiusMetadata = new FrameworkPropertyMetadata();
            var geometryMetadata= new FrameworkPropertyMetadata();

            ResolutionProperty = DependencyProperty.Register("Resolution", typeof(double), typeof(ReticleCanvas), resolutionMetadata);
            DimensionProperty = DependencyProperty.Register("Dimension", typeof(double), typeof(ReticleCanvas), dimensionMetadata);
            RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(ReticleCanvas), radiusMetadata);
            GeometryProperty = DependencyProperty.Register("Geomertry", typeof(MeshGeometry3D), typeof(ReticleCanvas), geometryMetadata);

        }
        private static void OnResolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((ReticleCanvas)d).DrawCombined();
        }
        private static void OnDimensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((ReticleCanvas)d).DrawCombined();
        }
        private void DrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.DrawCombined();

            DoubleAnimation dimensionAnimation = new DoubleAnimation();
            dimensionAnimation.From = 0.0;
            dimensionAnimation.To = this.Dimension;
            dimensionAnimation.Duration = TimeSpan.FromSeconds(10);
            this.BeginAnimation(ReticleCanvas.DimensionProperty, dimensionAnimation);

            DoubleAnimation resolutionAnimation = new DoubleAnimation();
            resolutionAnimation.From = this.Resolution;
            resolutionAnimation.To = this.Resolution;
            resolutionAnimation.Duration = TimeSpan.FromSeconds(10);
            this.BeginAnimation(ReticleCanvas.ResolutionProperty, resolutionAnimation);
        }
        public ReticleCanvas()
            : base()
        {
            Loaded += DrawingCanvas_Loaded;
        }
        #endregion

        private void DrawCombined()
        {
            this.ClearVisual();
            this.Geometry.Positions.Clear();
            this.Geometry.TriangleIndices.Clear();

            if (!this.populated)
            {
                this.PopulateCache();
            }
            if (!this.populate_mesh_cache)
            {
                this.PopulateResolutionCache();
            }
            this.DrawGrid();
            this.DrawMesh();
        }


        private SolidColorBrush background = Brushes.Transparent;

        private static LinearGradientBrush GridGradient;
        private Pen GridPen = new Pen(ReticleCanvas.GridGradient, 1);
        public void DrawGrid()
        {
            var level = (int)this.Dimension;
            var remainder = this.Dimension >= 1 ? this.Dimension % level : this.Dimension;
            for (int i = 0; i < level; i++)
            {
                this.DrawGridLevel(i);
            }

            DrawingVisual visual;
            if (remainder != 0.0)
            {
                var unit_length = this.UnitLength(level);
                foreach (var point in this.QuadrantGrid[level])
                {
                    visual = new DrawingVisual();
                    using (var dc = visual.RenderOpen())
                    {
                        dc.DrawEllipse(this.background, this.GridPen, this.ScaleToCanvas(point), unit_length * this.Radius * (1 - remainder), unit_length * this.Radius);
                        dc.DrawEllipse(this.background, this.GridPen, this.ScaleToCanvas(point), unit_length * this.Radius, unit_length * this.Radius * (1 - remainder));
                    }
                    this.AddVisual(visual);
                }
            }
        }
        private void DrawGridLevel(int level)
        {
            DrawingVisual visual;
            var unit_length = this.UnitLength(level);
            foreach (var point in this.QuadrantGrid[level])
            {
                visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    dc.DrawLine(this.GridPen, this.ScaleToCanvas(new Point(point.X, point.Y + unit_length)), this.ScaleToCanvas(new Point(point.X, point.Y - unit_length)));
                    dc.DrawLine(this.GridPen, this.ScaleToCanvas(new Point(point.X + unit_length, point.Y)), this.ScaleToCanvas(new Point(point.X - unit_length, point.Y)));
                }
                this.AddVisual(visual);
            }
        }
        private Point ScaleToCanvas(Point p)
        {
            return new Point(p.X * this.Radius, p.Y * this.Radius);
        }
        private Point3D ScaleToCanvas(Point scaled_2D, double z)
        {
            return new Point3D(scaled_2D.X, scaled_2D.Y, z * this.Radius);
        }
        private double UnitLength(int level)
        {
            return 1 / Math.Pow(2, level - 1);
        }

        private static LinearGradientBrush MeshGradient;
        private Pen MeshPen = new Pen(ReticleCanvas.MeshGradient, 1);
        public void DrawMesh()
        {
            var level = (int)this.Resolution;
            var remainder = this.Dimension >= 1 ? this.Dimension % level : this.Dimension;
            this.DrawMeshLevel(level);
        }
        private void DrawMeshLevel(int level)
        {
            if (level != 0.0)
            {
                DrawingVisual visual;
                for (int i = 0; i < this.ResolutionCache.Length; i++)
                {
                    visual = new DrawingVisual();
                    using (var dc = visual.RenderOpen())
                    {

                        dc.DrawLine(MeshPen, this.ResolutionCache[i][0], this.ResolutionCache[i][1]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][1], this.ResolutionCache[i][2]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][2], this.ResolutionCache[i][0]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][0], this.ResolutionCache[i][3]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][3], this.ResolutionCache[i][2]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][3], this.ResolutionCache[i][5]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][4], this.ResolutionCache[i][3]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][0], this.ResolutionCache[i][4]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][4], this.ResolutionCache[i][5]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][4], this.ResolutionCache[i][6]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][6], this.ResolutionCache[i][0]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][7], this.ResolutionCache[i][6]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][7], this.ResolutionCache[i][0]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][7], this.ResolutionCache[i][1]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][1], this.ResolutionCache[i][8]);
                        dc.DrawLine(MeshPen, this.ResolutionCache[i][8], this.ResolutionCache[i][7]);
                    }
                    this.AddVisual(visual);
                }
            }
        }
        private MeshGeometry3D Geometry = new MeshGeometry3D();

        private bool populate_mesh_cache = false;
        private Point[][] ResolutionCache;
        public void PopulateResolutionCache()
        {
            var level = (int)this.Resolution;
            if (level != 0.0)
            {
                var unit_length = this.UnitLength(level);
                var origins = this.QuadrantGrid[level - 1];
                var points = new Point[origins.Length][];
                for (int i = 0; i < origins.Length; i++)
                {
                    points[i] = new Point[9];
                    //origin
                    points[i][0] = this.ScaleToCanvas(origins[i]);
                    //top
                    points[i][1] = this.ScaleToCanvas(new Point(origins[i].X, origins[i].Y - 2 * unit_length));
                    //top-right
                    points[i][2] = this.ScaleToCanvas(new Point(origins[i].X + 2 * unit_length, origins[i].Y - 2 * unit_length));
                    //right
                    points[i][3] = this.ScaleToCanvas(new Point(origins[i].X + 2 * unit_length, origins[i].Y));
                    //bottom
                    points[i][4] = this.ScaleToCanvas(new Point(origins[i].X, origins[i].Y + 2 * unit_length));
                    //bottom-right
                    points[i][5] = this.ScaleToCanvas(new Point(origins[i].X + 2 * unit_length, origins[i].Y + 2 * unit_length));
                    //bottom-left
                    points[i][6] = this.ScaleToCanvas(new Point(origins[i].X - 2 * unit_length, origins[i].Y + 2 * unit_length));
                    //left
                    points[i][7] = this.ScaleToCanvas(new Point(origins[i].X - 2 * unit_length, origins[i].Y));
                    //top-right
                    points[i][8] = this.ScaleToCanvas(new Point(origins[i].X - 2 * unit_length, origins[i].Y - 2 * unit_length));
                }
                this.ResolutionCache = points;
                this.populate_mesh_cache = true;
                this.RefreshGeometry();
            }
        }
        private void RefreshGeometry()
        {
            var z = 0.0;
            var partition = 9;
            this.Geometry.Positions.Clear();
            this.Geometry.TriangleIndices.Clear();
            for (int i=0; i< this.ResolutionCache.Length; i++)
            {
                var partition_index = i * partition;
                foreach (var j in this.ResolutionCache[i])
                {
                    this.Geometry.Positions.Add(new Point3D(j.X, j.Y, z));
                }
                var triangle_indicies = new int[]   {   1,8,7,
                                                        7,0,1,
                                                        0,8,6,
                                                        6,0,4,
                                                        4,3,0,
                                                        4,5,3,
                                                        3,2,0,
                                                        0,2,1};
                foreach (var triangle_index in triangle_indicies)
                {
                    this.Geometry.TriangleIndices.Add(triangle_index + partition_index);
                }
            }

        }
        private bool populated = false;
        private Point[][] QuadrantGrid = new Point[1][] { new[] { new Point(0, 0) } };
        public void PopulateCache()
        {
            var rounded_dimension = (int)this.Dimension;
            var points = new Point[rounded_dimension + 1][];
            points[0] = new[] { new Point(0, 0) };
            for (int i = 1; i <= rounded_dimension; i++)
            {
                var unit_length = this.UnitLength(i);
                var sub_group_length = (int)Math.Pow(4, i);
                var sub_group = new Point[sub_group_length];
                var sub_group_index = 0;
                for (int j = 0; j < points[i - 1].Length; j++)
                {
                    sub_group[sub_group_index] = new Point(points[i - 1][j].X + unit_length, points[i - 1][j].Y + unit_length);
                    sub_group[sub_group_index + 1] = new Point(points[i - 1][j].X - unit_length, points[i - 1][j].Y + unit_length);
                    sub_group[sub_group_index + 2] = new Point(points[i - 1][j].X - unit_length, points[i - 1][j].Y - unit_length);
                    sub_group[sub_group_index + 3] = new Point(points[i - 1][j].X + unit_length, points[i - 1][j].Y - unit_length);
                    sub_group_index += 4;
                }
                points[i] = sub_group;
            }
            this.QuadrantGrid = points;
            this.populated = true;
        }

        #region Control of this control's visual children
        private List<Visual> visuals = new List<Visual>();
        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }
        public void ClearVisual()
        {
            while (this.visuals.Count > 0)
            {
                this.DeleteVisual(this.visuals.First());
            }
        }
        public void DeleteVisual(Visual visual)
        {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }
        #endregion
    }
}
