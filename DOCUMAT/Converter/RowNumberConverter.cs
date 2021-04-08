using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DOCUMAT.Converter
{
    class RowNumberConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Object item = values[0];
                DataGrid grdPassedGrid = values[1] as DataGrid;
                int intRowNumber = grdPassedGrid.Items.IndexOf(item) + 1;
                return intRowNumber.ToString();
            }
            catch (Exception ex)
            {
                //ex.ExceptionCatcher();
                return "nd".ToString();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
