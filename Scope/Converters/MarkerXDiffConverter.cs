using System;
using System.Globalization;
using System.Windows.Data;

namespace Scope.Converters
{

	public class MarkerXDiffConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(values[0] is TimeSpan m1) ||
				!(values[1] is TimeSpan m2))
			{
				return 0;
			}

			if(m2 > m1)
				return (m2 - m1).ToString(@"hh\:mm\:ss\:fff");
			else 
				return (m1 - m2).ToString(@"hh\:mm\:ss\:fff");


		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
