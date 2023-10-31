using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Scope.Converters
{

	public class IsRecordingToBrushConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool isRecording))
				return Brushes.LightGray;

			if(isRecording)
				return Brushes.Red;
			else
				return Brushes.LightGray;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
