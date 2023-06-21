using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FlexMold.Convertor
{
    public class DistinctConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            var values = value as IEnumerable;
            if (values == null)
                return null;

            return values.Cast<object>().Distinct();
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}