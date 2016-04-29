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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThePriceIsAName
{
    /// <summary>
    /// Interaction logic for HealthBarPage.xaml
    /// </summary>
    public partial class HealthBarPage : Page
    {
        public HealthBarPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hero.Subtract(15, new Actor { Color = new SolidColorBrush(Colors.Aqua) });
        }
    }
}
