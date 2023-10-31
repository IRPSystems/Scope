
using System.Globalization;
using System.Windows.Data;
using System;
using MCUScope.Models;

namespace MCUScope.Converters
{
	public class RecordingGapToIntervalConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is int gap))
				return null;

			return (double)gap / (double)TriggerSelectionData.InterruptFreq;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
