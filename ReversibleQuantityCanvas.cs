namespace ThePriceIsAName
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// An abstract class containing the machinary to store an abstract
    /// quantity and record changes to that quantity from an an abstract
    /// source. This may seem a little abstract, so I will bring it to earth.
    /// 
    /// Example 1
    /// class ReversibleDoubleAnimation : ReversibleQuantity'double,string' ...  //side-note, can't (yet) use angled brackets in this comment
    /// 
    /// This class will only contain the graphics needed to perform the task
    /// of visualizing the double. Even in this example the 'Animation' might be
    /// to vague, pehaps 'Bar' or 'Graph'. Regardless, the work of setting up
    /// the DependencyProperties, tracking the changes, and managing the 
    /// memory of the double will be this class, ReversibleQuantityCanvas'T,S'
    /// </summary>
    /// <typeparam name="T">The type of quantity to store.</typeparam>
    /// <typeparam name="S">The type of the source that makes a change on the quantity.</typeparam>
    public abstract class ReversibleQuantityCanvas<T, S> : Canvas
    {
        public struct Entry
        {
            public DateTime Time;
            public int Direction;
            public T Amount;
            public S Source;
        }
        public struct History
        {
            public DateTime Time;
            public int UnledgerX;
            public int UnledgerY;
            public Entry[] Unledger;
        }
        public static readonly RoutedEvent LedgerEvent;
        static ReversibleQuantityCanvas()
        {
            ReversibleQuantityCanvas<T, S>.LedgerEvent = EventManager.RegisterRoutedEvent("Updated", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(ReversibleQuantityCanvas<T, S>));
        }
        public event RoutedEventHandler Updated
        {
            add
            {
                base.AddHandler(ReversibleQuantityCanvas<T, S>.LedgerEvent, value);
            }
            remove
            {
                base.RemoveHandler(ReversibleQuantityCanvas<T, S>.LedgerEvent, value);
            }
        }
        private List<Entry> ledger = new List<Entry>();
        private List<Entry> unledger = new List<Entry>();
        private List<History> histories = new List<History>();
        private int Depth = 0;

        public IEnumerable<Entry> Ledger { get { return ledger.AsEnumerable(); } }
        public void Add(T value, S source)
        {
            this.ledger.Add(new Entry
            {
                Time = DateTime.Now,
                Direction = 1,
                Amount = value,
                Source = source
            });
            this.save_history();
            this.raise_update_event();
        }
        public void Subtract(T value, S source)
        {
            this.ledger.Add(new Entry
            {
                Time = DateTime.Now,
                Direction = -1,
                Amount = value,
                Source = source
            });
            this.save_history();
            this.raise_update_event();
        }
        public void Undo()
        {
            if (this.ledger.Count > 0)
            {
                var e = this.ledger.Last();
                this.ledger.Remove(e);
                this.unledger.Add(e);
                this.raise_update_event();
            }
        }
        public void Redo()
        {
            if (this.unledger.Count > 0)
            {
                var e = this.unledger.Last();
                this.unledger.Remove(e);
                this.ledger.Add(e);
                this.raise_update_event();
            }
        }

        private void save_history()
        {
            if (this.unledger.Count > 0)
            {
                var e = new Entry[this.unledger.Count];
                this.unledger.CopyTo(e);
                this.histories.Add(new History
                {
                    Time = DateTime.Now,
                    Unledger = e,
                    UnledgerX = this.ledger.Count + 1,
                    UnledgerY = Depth
                });
                this.Depth++;
            }
        }

        private void raise_update_event()
        {
            var args = new RoutedEventArgs(ReversibleQuantityCanvas<T, S>.LedgerEvent, this);
            base.RaiseEvent(args);
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

    public class ReversibleDoubleCanvas<S> : ReversibleQuantityCanvas<double, S>
    {
        public double Quantity
        {
            get { return (double)GetValue(QuantityProperty); }
            set { SetValue(QuantityProperty, value); }
        }
        public static readonly DependencyProperty QuantityProperty;

        static ReversibleDoubleCanvas()
        {
            var quantityMetadata = new FrameworkPropertyMetadata();
            QuantityProperty = DependencyProperty.Register("Quantity", typeof(double), typeof(ReversibleDoubleCanvas<S>), quantityMetadata);
        }

        private void ReversibleDoubleCanvas_Updated(object sender, RoutedEventArgs e)
        {
            this.Quantity = this.LedgerQuantity;
        }
        public ReversibleDoubleCanvas()
            : base()
        {
            Updated += ReversibleDoubleCanvas_Updated;
        }
        public double LedgerQuantity
        {
            get
            {
                double result = 0;
                foreach (var e in this.Ledger)
                {
                    result += (e.Amount * e.Direction);
                }
                return result;
            }
        }
    }


}
