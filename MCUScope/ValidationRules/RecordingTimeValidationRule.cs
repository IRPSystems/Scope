

using System.Globalization;
using System.Windows.Controls;

namespace MCUScope.ValidationRules
{
    public class RecordingTimeValidationRule : ValidationRule
	{
		public RecordingTimeWrapper RecordingTimeWrapper { get; set; }

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)		
		{
			if(RecordingTimeWrapper.RecordingTime <= 0 )
			{
				return new ValidationResult(false, "The record time cannot be zero or lower");
			}


			return ValidationResult.ValidResult;
		}
	}

	

	
}
