
using System.Windows;

namespace MCUScope.ValidationRules
{
	public class RecordingTimeWrapper : DependencyObject
	{
		public static readonly DependencyProperty RecordingTimeProperty =
			 DependencyProperty.Register("RecordingTime", typeof(double),
			 typeof(RecordingTimeWrapper));

		public double RecordingTime
		{
			get { return (double)GetValue(RecordingTimeProperty); }
			set { SetValue(RecordingTimeProperty, value); }
		}
	}
}
