namespace ThePriceIsAName
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public struct Actor
    {
        public Brush Color;
    }
    public class HealthBar : ReversibleDoubleCanvas<Actor>
    {
        public double Normal
        {
            get { return (double)GetValue(NormalProperty); }
            set { SetValue(NormalProperty, value); }
        }
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public static readonly DependencyProperty RadiusProperty;
        public static readonly DependencyProperty NormalProperty;

        static HealthBar()
        {
            var normalMetadata = new FrameworkPropertyMetadata(1.0);
            var radiusMetadata = new FrameworkPropertyMetadata();

            NormalProperty = DependencyProperty.Register("Normal", typeof(double), typeof(HealthBar), normalMetadata);
            RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(HealthBar), radiusMetadata);
        }

        #region Test objects, shouldnt be here. We be solved with better Xaml asset work but unimportant for the moment.
        double bar_height = 1.0 / 8.0;
        double pen_thickness = 1;
        Brush positive_brush = new SolidColorBrush(Colors.Black);
        Brush negative_brush = new SolidColorBrush(Colors.LightGray);
        Actor protagonist = new Actor { Color = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(Colors.DarkMagenta, 0.0), new GradientStop(Colors.DarkBlue, .5) })) };
        Actor antagonist = new Actor { Color = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(Colors.LawnGreen, 0.0), new GradientStop(Colors.IndianRed, .5) })) };
        Actor powers_tha_be = new Actor { Color = new LinearGradientBrush(new GradientStopCollection(new[] { new GradientStop(Colors.DarkBlue, 0.0), new GradientStop(Colors.LightBlue, .5) })) };
        #endregion
        public HealthBar()
            : base()
        {
            Loaded += HealthBar_Loaded;
            Updated += HealthBar_Updated;
        }

        private void HealthBar_Loaded(object sender, RoutedEventArgs e)
        {
            this.Add(this.Normal, new Actor { Color = positive_brush });
            this.Draw();
        }

        private void HealthBar_Updated(object sender, RoutedEventArgs e)
        {
            this.Draw();
        }

        public void Draw()
        {
            this.ClearVisual();

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                var normal_quantity = this.Quantity > 0 ? this.Quantity / this.Normal : 0.0;
                var positive_pen = normal_quantity <= 0 ? new Pen(this.negative_brush, this.pen_thickness) : new Pen(this.positive_brush, this.pen_thickness);
                var negative_pen = normal_quantity >= 1 ? new Pen(this.positive_brush, this.pen_thickness) : new Pen(this.negative_brush, this.pen_thickness);

                var p1 = this.ProjectToLine(new Point(0, this.bar_height));
                var p2 = this.ProjectToLine(new Point(0, this.bar_height * -1));
                dc.DrawLine(positive_pen, p1, p2);

                p1 = this.ProjectToLine(new Point(0, 0));
                p2 = this.ProjectToLine(new Point(this.Quantity, 0));
                dc.DrawLine(positive_pen, p1, p2);

                p1 = this.ProjectToLine(new Point(this.Quantity, 0));
                p2 = this.ProjectToLine(new Point(this.Normal, 0));
                dc.DrawLine(negative_pen, p1, p2);

                p1 = this.ProjectToLine(new Point(this.Normal, this.bar_height));
                p2 = this.ProjectToLine(new Point(this.Normal, this.bar_height * -1));
                dc.DrawLine(negative_pen,p1 ,p2);
            }
            this.AddVisual(visual);
        }

        public Point ProjectToLine(Point p)
        {
            if (p.X > 0)
            {
                var n = p.X / this.Normal;
                return new Point((.5 - n) * -2 * this.Radius, p.Y * this.Radius);
            }
            else
            {
                return new Point(-1 * this.Radius, p.Y * this.Radius);
            }
        }

    }
}
