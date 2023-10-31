using System;
using System.Globalization;
using System.Windows.Data;

namespace Scope.Converters
{

	public class MarkerYDiffConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(values[0] == null || values[1] == null)
				return null;

			double m1 = System.Convert.ToDouble(values[0]);
			double m2 = System.Convert.ToDouble(values[1]);

			return Math.Abs(m1 - m2).ToString("F3");
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
