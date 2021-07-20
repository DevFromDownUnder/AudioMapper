using System;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace AudioMapper.Resources.Converters
{
    internal class ResourceImageConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return Helper.ConsumeExceptions(() => new BitmapImage(new Uri(string.Format("pack://application:,,,/Resources/{0}", string.Join<object>("", values)))));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}