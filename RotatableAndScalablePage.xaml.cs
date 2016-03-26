namespace ThePriceIsAName
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class RotatableAndScalablePage : Page
    {
        public RotatableAndScalablePage()
        {
            InitializeComponent();

            this.ScalarSlider.Maximum = double.MaxValue;
            this.RotationSlider.Maximum = 360;
            this.RotationSlider.Minimum = 0;
        }

        private bool middle_mouse_pressed = false;

        private void Page_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.middle_mouse_pressed = e.MiddleButton == MouseButtonState.Pressed;
        }
        private void Page_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released && this.middle_mouse_pressed)
            {
                this.middle_mouse_pressed = false;
                this.ScalarSlider.Value = 1.0;
                this.RotationSlider.Value = 0;
            }
        }
        private void Page_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var rotate = (Keyboard.Modifiers & ModifierKeys.Shift) > 0;
            if (e.Delta > 0)
                if (rotate)
                    this.RotationSlider.Value = (this.RotationSlider.Value + 10) % this.RotationSlider.Maximum;
                else
                    this.ScalarSlider.Value *= 1.1;
            else
                if (rotate)
                {
                    var proposed_value = (this.RotationSlider.Value - 10);
                    if (proposed_value > 0)
                        this.RotationSlider.Value = proposed_value;
                    else
                        this.RotationSlider.Value = this.RotationSlider.Maximum + proposed_value;
                }
                else
                    this.ScalarSlider.Value *= 0.9;

        }
    }
}
