using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThePriceIsAName
{
    /// <summary>
    /// Interaction logic for PolarGridCanvas.xaml
    /// </summary>
    public partial class PolarGridCanvas : Canvas
    {
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }
        public double Theta
        {
            get { return (double)GetValue(ThetaProperty); }
            set { SetValue(ThetaProperty, value); }
        }
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public MeshGeometry3D Model
        {
            get { return (MeshGeometry3D)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        #region Dependency properties for Resolution and Dimension intended for data binding and visual draw on change.
        public static readonly DependencyProperty RadiusProperty;
        public static readonly DependencyProperty ThetaProperty;
        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;
        public static readonly DependencyProperty ModelProperty;

        static PolarGridCanvas()
        {
            var radiusMetadata = new FrameworkPropertyMetadata();
            var ThetaMetadata = new FrameworkPropertyMetadata(OnThetaChanged);
            var XMetadata = new FrameworkPropertyMetadata();
            var YMetadata = new FrameworkPropertyMetadata();
            var modelMetadata = new FrameworkPropertyMetadata();

            RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(PolarGridCanvas), radiusMetadata);
            ThetaProperty = DependencyProperty.Register("Theta", typeof(double), typeof(PolarGridCanvas), ThetaMetadata);
            XProperty = DependencyProperty.Register("X", typeof(double), typeof(PolarGridCanvas), XMetadata);
            YProperty = DependencyProperty.Register("Y", typeof(double), typeof(PolarGridCanvas), YMetadata);
            ModelProperty = DependencyProperty.Register("Model", typeof(MeshGeometry3D), typeof(PolarGridCanvas), modelMetadata);

            PolarGridCanvas.GridGradient = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(Colors.SteelBlue, 0.0), new GradientStop(Colors.RosyBrown, .5) }));
        }
        private static void OnThetaChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            //((PolarGridCanvas)d).Draw();
        }
        private void PolarGridCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            //DoubleAnimation dimensionAnimation = new DoubleAnimation();
            //dimensionAnimation.From = 0;
            //dimensionAnimation.To = 200;
            //dimensionAnimation.AutoReverse = true;
            //dimensionAnimation.Duration = TimeSpan.FromHours(1);
            //this.BeginAnimation(PolarMultiplicationCanvas.DimensionProperty, dimensionAnimation);
            var mesh = new MeshGeometry3D();
            mesh.ParsePositions("0,-1,0 0,0,-1 0,1,0 1,1,0");
            mesh.ParseTriangleIndices("0,2,1 0,1,3 0,2,3 1,2,3");
            this.Model = mesh;
        }
        public PolarGridCanvas()
            : base()
        {
            this.Loaded += PolarGridCanvas_Loaded;
        }
        #endregion


        private Brush background = Brushes.Transparent;
        private static LinearGradientBrush GridGradient;
        private Pen GridPen = new Pen(PolarGridCanvas.GridGradient, 1);
        private void Draw()
        {
            this.ClearVisual();

            DrawingVisual visual;

            var center = new Point(0, 0);

            //Unit circle
            visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawEllipse(this.background, this.GridPen, center, this.Radius, this.Radius);
            }
            this.AddVisual(visual);

            var xLength = X != 0 ? Theta / X : 0;
            for (int i = 0; i <= this.X; i++)
            {
                visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen())
                {
                    dc.DrawLine(this.GridPen, center, this.ProjectToCircle(center, this.ToRadian(this.Theta)));
                }
                this.AddVisual(visual);
            }
        }
        private double ToRadian(double degree)
        {
            var remainder = degree % 360;
            var fraction = remainder / 360;
            return fraction * 2 * Math.PI;
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
