using System;
using System.Globalization;
using System.Windows.Data;

namespace Scope.Converters
{
	public class MarkerXToTimeStringConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TimeSpan time))
				return value;

			return time.ToString(@"hh\:mm\:ss\:fff");
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
