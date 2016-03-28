namespace ThePriceIsAName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

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

        static ReticleCanvas()
        {
            ;
            var dimensionMetadata = new FrameworkPropertyMetadata(OnDimensionChanged);
            var resolutionMetadata = new FrameworkPropertyMetadata(OnResolutionChanged);
            var radiusMetadata = new FrameworkPropertyMetadata();

            ResolutionProperty = DependencyProperty.Register("Resolution", typeof(double), typeof(ReticleCanvas), resolutionMetadata);
            DimensionProperty = DependencyProperty.Register("Dimension", typeof(double), typeof(ReticleCanvas), dimensionMetadata);
            RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(ReticleCanvas), radiusMetadata);
        }
        private static void OnResolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((ReticleCanvas)d).Draw();
        }
        private static void OnDimensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (!((ReticleCanvas)d).populated)
            {
                ((ReticleCanvas)d).PopulateCache();
            }
            ((ReticleCanvas)d).Draw();
        }
        private void DrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.PopulateCache();
            this.Draw();

            DoubleAnimation dimensionAnimation = new DoubleAnimation();
            dimensionAnimation.From = 0;
            dimensionAnimation.To = this.Dimension;
            dimensionAnimation.Duration = TimeSpan.FromMinutes(1);
            this.BeginAnimation(ReticleCanvas.DimensionProperty, dimensionAnimation);
        }
        public ReticleCanvas()
            : base()
        {
            Loaded += DrawingCanvas_Loaded;
        }
        #endregion

        //These will probably need to become dependency properties if I continue with the class.
        private SolidColorBrush background = Brushes.Transparent;
        private static LinearGradientBrush gradient = new LinearGradientBrush(Colors.DarkBlue, Colors.DarkMagenta, 45);
        private Pen pen = new Pen(ReticleCanvas.gradient, 1);
        public void Draw()
        {
            this.ClearVisual();

            var level = (int)this.Dimension;
            var remainder = this.Dimension > 1 ? this.Dimension % level : this.Dimension;
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
                        dc.DrawEllipse(this.background, this.pen, this.ScaleToCanvas(point), unit_length * this.Radius * (1 - remainder), unit_length * this.Radius);
                        dc.DrawEllipse(this.background, this.pen, this.ScaleToCanvas(point), unit_length * this.Radius, unit_length * this.Radius * (1 - remainder));
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
                    dc.DrawLine(this.pen, this.ScaleToCanvas(new Point(point.X, point.Y + unit_length)), this.ScaleToCanvas(new Point(point.X, point.Y - unit_length)));
                    dc.DrawLine(this.pen, this.ScaleToCanvas(new Point(point.X + unit_length, point.Y)), this.ScaleToCanvas(new Point(point.X - unit_length, point.Y)));
                }
                this.AddVisual(visual);
            }
        }
        private Point ScaleToCanvas(Point p)
        {
            return new Point(p.X * this.Radius, p.Y * this.Radius);
        }
        private double UnitLength(int level)
        {
            return 1 / Math.Pow(2, level - 1);
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
