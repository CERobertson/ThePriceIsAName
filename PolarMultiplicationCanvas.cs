namespace ThePriceIsAName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public class PolarMultiplicationCanvas : Canvas
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

        static PolarMultiplicationCanvas()
        {
            var resolutionMetadata = new FrameworkPropertyMetadata(OnResolutionChanged);
            var dimensionMetadata = new FrameworkPropertyMetadata(OnDimensionChanged);
            var radiusMetadata = new FrameworkPropertyMetadata();

            ResolutionProperty = DependencyProperty.Register("Resolution", typeof(double), typeof(PolarMultiplicationCanvas), resolutionMetadata);
            DimensionProperty = DependencyProperty.Register("Dimension", typeof(double), typeof(PolarMultiplicationCanvas), dimensionMetadata);
            RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(PolarMultiplicationCanvas), radiusMetadata);
        }
        private static void OnResolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((PolarMultiplicationCanvas)d).Refresh_HashCache();
            ((PolarMultiplicationCanvas)d).Draw();
        }
        private static void OnDimensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((PolarMultiplicationCanvas)d).Draw();
        }
        private void DrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.Refresh_HashCache();
            this.Draw();

            DoubleAnimation dimensionAnimation = new DoubleAnimation();
            dimensionAnimation.From = 0;
            dimensionAnimation.To = 200;
            dimensionAnimation.AutoReverse = true;
            dimensionAnimation.Duration = TimeSpan.FromHours(1);
            this.BeginAnimation(PolarMultiplicationCanvas.DimensionProperty, dimensionAnimation);
        }
        public PolarMultiplicationCanvas()
            : base()
        {
            Loaded += DrawingCanvas_Loaded;
        }
        #endregion

        public void Draw()
        {
            this.ClearVisual();

            DrawingVisual visual;
            
            var center = new Point(0, 0);
            //var center2 = new Point((this.Width / 2) + this.Radius, (this.Height / 2) + this.Radius);

            var background = Brushes.Transparent;
            //var gradient = new LinearGradientBrush(Colors.DarkBlue, Colors.DarkMagenta, 45);
            var gradient = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(Colors.DarkMagenta, 0.0), new GradientStop(Colors.DarkBlue, .25), new GradientStop(Colors.DarkMagenta, 0.75) }));
            var pen = new Pen(gradient, 1);

            //Unit circle
            visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawEllipse(background, pen, center, this.Radius, this.Radius);
            }
            this.AddVisual(visual);

            //Multiplication lines
            for (int i = 0; i < this.Resolution; i++)
            {

                visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen())
                {
                    dc.DrawLine(pen, this.HashCache[i], this.ProjectToCircle(center, i * this.Dimension % this.Resolution * 2 * Math.PI / this.Resolution));
                }
                this.AddVisual(visual);
            }

        }

        private Point[] HashCache;
        public void Refresh_HashCache()
        {
            var sides = (int)Math.Ceiling(this.Resolution);
            var length = (1 / Math.Ceiling(this.Resolution)) * Math.PI * 2;
            var center = new Point(0, 0);

            this.HashCache = new Point[(sides)];
            for (int i = 0; i < this.Resolution; i++)
            {
                this.HashCache[i] = this.ProjectToCircle(center, length * i);
            }
        }

        private Point ProjectToCircle(Point center, double radian)
        {
            return new Point(Math.Cos(radian) * this.Radius + center.X, (Math.Sin(radian) * this.Radius) + center.Y);
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
