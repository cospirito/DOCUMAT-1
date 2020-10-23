using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace DOCUMAT.Outils
{
    /// <summary>
    /// Logique d'interaction pour ProgressBarCirculaire.xaml
    /// </summary>
    public partial class ProgressBarCirculaire : UserControl
    {
        double defautPick = 15.71, defaultWidth = 300, defaultHeight = 300, defaultThickness = 50; 

        public static readonly DependencyProperty ProgressIndicatorProperty = DependencyProperty.Register("ProgressIndicator", typeof(string), typeof(ProgressBarCirculaire));
        public string ProgressIndicator
        {
            get { return (string)this.GetValue(ProgressIndicatorProperty); }
            set { this.SetValue(ProgressIndicatorProperty, value); }
        }

        public static readonly DependencyProperty TextIndicatorProperty = DependencyProperty.Register("TextIndicator", typeof(string), typeof(ProgressBarCirculaire));
        public string TextIndicator
        {
            get { return (string)this.GetValue(TextIndicatorProperty); }
            set { this.SetValue(TextIndicatorProperty, value); }
        }

        public ProgressBarCirculaire()
        {
            InitializeComponent();
        }

    }

    [ValueConversion(typeof(double), typeof(double[]))]
    public class IndicatorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double defautPick = 15.71, defaultWidth = 300, defaultHeight = 300, defaultThickness = 50;
            double thick1 = (defautPick * (double)value) / 100;

            return new double[2] { thick1, defautPick };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
