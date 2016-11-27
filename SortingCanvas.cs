﻿namespace ThePriceIsAName {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public class SortingCanvas : Canvas {
        private const double delay = 0.0;
        private const int list_length = 300;
        private double[] listA = new double[list_length];
        private double[] listB = new double[list_length];
        private Random random = new Random();

        public List<Frame> Frames = new List<Frame>();
        public int Frame {
            get { return (int)GetValue(FrameProperty); }
            set { SetValue(FrameProperty, value); }
        }
        public static readonly DependencyProperty FrameProperty;
        static SortingCanvas() {
            var frameMetadata = new FrameworkPropertyMetadata(OnFrameChanged);
            FrameProperty = DependencyProperty.Register("Frame", typeof(int), typeof(SortingCanvas), frameMetadata);
        }
        private static void OnFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var instance = d as SortingCanvas;
            instance.Draw(instance.Frames.ElementAt((int)args.OldValue).List);
        }

        public SortingCanvas()
            : base() {
            Loaded += SortingCanvas_Loaded;
        }

        private void SortingCanvas_Loaded(object sender, RoutedEventArgs e) {
            for (int i = 0; i < list_length; i++) {
                listA[i] = random.NextDouble() * this.Height;
                listB[i] = listA[i];
            }
            //TopDownSplitMerge(listB, 0, list_length, listA);
            MottomUpMergeSort(listA, listB, list_length);

            Int32Animation frameAnimation = new Int32Animation();
            frameAnimation.From = 0;
            frameAnimation.To = Frames.Count + 1;
            frameAnimation.Duration = TimeSpan.FromSeconds(30);
            this.BeginAnimation(SortingCanvas.FrameProperty, frameAnimation);
        }

        void TopDownSplitMerge(double[] B, int begin, int end, double[] A) {
            if (end - begin < 2) {
                return;
            }
            int middle = (end + begin) / 2;
            TopDownSplitMerge(A, begin, middle, B);
            TopDownSplitMerge(A, middle, end, B);
            TopDownMerge(B, begin, middle, end, A);
        }

        void TopDownMerge(double[] A, int begin, int middle, int end, double[] B) {
            int i = begin;
            int j = middle;
            for (int k = begin; k < end; k++) {
                if (i < middle && (j >= end || A[i] <= A[j])) {
                    B[k] = A[i];
                    i = i + 1;
                }
                else {
                    B[k] = A[j];
                    j = j + 1;
                    var frame = new Frame(list_length);
                    B.CopyTo(frame.List, 0);
                    Frames.Add(frame);
                }
            }
            //attempt to get the animations looking nicer.
            for (int k = begin; k < end; k++) {
                A[k] = B[k];
            }
        }


        void MottomUpMergeSort(double[] A, double[] B, int n) {
            double[] temp;
            for (int width = 1; width < n; width = 2 * width) {
                for (int i = 0; i < n; i = i + 2 * width) {
                    BottomUpMerge(A, i, Math.Min(i + width, n), Math.Min(i + 2 * width, n), B);
                }
                temp = A;
                A = B;
                B = temp;
            }
        }

        void BottomUpMerge(double[] A, int left, int right, int end, double[] B) {
            int i = left;
            int j = right;
            for (int k = left; k < end; k++) {
                if (i < right && (j >= end || A[i] <= A[j])) {
                    B[k] = A[i];
                    i = i + 1;
                }
                else {
                    B[k] = A[j];
                    j = j + 1;
                }
                var frame = new Frame(list_length);
                B.CopyTo(frame.List, 0);
                Frames.Add(frame);
            }
            //attempt to get the animations looking nicer.
            for (int k = left; k < end; k++) {
                A[k] = B[k];
            }
        }

        public void Draw(double[] l) {
            double item_width = this.Width / list_length;
            this.ClearVisual();
            DrawingVisual visual;
            var background = Brushes.Transparent;
            var pen = new Pen(Brushes.Black, 1);
            for (int i = 0; i < list_length; i++) {
                visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen()) {
                    dc.DrawRectangle(background, pen, new Rect(i * item_width, 0, item_width, l[i]));
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
    public class Frame {
        public Frame(int list_length) {
            List = new double[list_length];
        }
        public double[] List;
    }
}