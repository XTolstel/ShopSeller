using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace AutoSellerUltra.AutoWindow
{
    public class CartQtyMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "Qty: 0";
            if (values[0] is not int id) return "Qty: 0";
            if (values[1] is not Dictionary<int, int> dict) return "Qty: 0";

            return dict.TryGetValue(id, out var qty) ? $"Qty: {qty}" : "Qty: 0";
        }

        public object[] ConvertBack(object value,Type[] targetTypes,object parameter,CultureInfo culture){throw new NotSupportedException();}

    }

}
