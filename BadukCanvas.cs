using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace ThePriceIsAName {
    public class BadukCanvas : FunctionCanvas {
        public BadukCanvas()
            : base() {
            Initialized += BadukCanvas_Initialized;
        }

        private void BadukCanvas_Initialized(object sender, EventArgs e) {
            for (int i = 0; i < list_length; i++) {
                listA[i] = i;
                listB[i] = listA[i];
            }
        }

        protected override void Function() {
            for (int n = 0; n < 5; n++) {
                for (int j = 0; j < list_length; j++) {
                    listB[j] = listB[j] + Math.Pow(-1, n) * Math.Pow(listA[j], 2 * n + 1) / Factorial(2 * n);
                }
                var frame = new Frame(list_length);
                listB.CopyTo(frame.List, 0);
                Frames.Add(frame);
            }
        }
        private double Factorial(int x) {
            double r = 1;
            for (int i = 1; i <= x; i++) {
                r = r * i;
            }
            return r;
        }
    }
    public abstract class FunctionCanvas : Canvas {
        protected const double delay = 0.0;
        protected const int list_length = 15;
        protected double[] listA = new double[list_length];
        protected double[] listB = new double[list_length];

        public List<Frame> Frames = new List<Frame>();
        public int Frame {
            get { return (int)GetValue(FrameProperty); }
            set { SetValue(FrameProperty, value); }
        }
        public static readonly DependencyProperty FrameProperty;
        static FunctionCanvas() {
            var frameMetadata = new FrameworkPropertyMetadata(OnFrameChanged);
            FrameProperty = DependencyProperty.Register("Frame", typeof(int), typeof(FunctionCanvas), frameMetadata);
        }
        private static void OnFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var instance = d as FunctionCanvas;
            instance.Draw(instance.Frames.ElementAt((int)args.OldValue).List);
        }

        public FunctionCanvas()
            : base() {
            Loaded += FunctionCanvas_Loaded;
        }

        private void FunctionCanvas_Loaded(object sender, RoutedEventArgs e) {
            Function();

            Int32Animation frameAnimation = new Int32Animation();
            frameAnimation.From = 0;
            frameAnimation.To = Frames.Count;
            frameAnimation.Duration = TimeSpan.FromSeconds(5);
            this.BeginAnimation(FunctionCanvas.FrameProperty, frameAnimation);
        }
        protected abstract void Function();
        public void Draw(double[] l) {
            double item_width = this.Width / list_length;
            this.ClearVisual();
            DrawingVisual visual;
            var background = Brushes.Transparent;
            var pen = new Pen(Brushes.Black, 1);
            for (int i = 0; i < list_length; i++) {
                visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen()) {
                    dc.DrawRectangle(background, pen, new Rect(new Point(i * item_width, 0), new Point((i* item_width) + item_width, l[i])));
                }
                this.AddVisual(visual);
            }
        }

        #region Control of this control's visual children
        private List<Visual> visuals = new List<Visual>();
        protected override int VisualChildrenCount {
            get { return visuals.Count; }
        }
        protected override Visual GetVisualChild(int index) {
            return visuals[index];
        }
        public void AddVisual(Visual visual) {
            visuals.Add(visual);

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }
        public void ClearVisual() {
            while (this.visuals.Count > 0) {
                this.DeleteVisual(this.visuals.First());
            }
        }
        public void DeleteVisual(Visual visual) {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }
        #endregion
    }
}
