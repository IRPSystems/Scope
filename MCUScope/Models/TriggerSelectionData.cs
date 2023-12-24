
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Models;
using Entities.Models;
using MCUScope.Enums;
using Newtonsoft.Json;

namespace MCUScope.Models
{
	public class TriggerSelectionData: ObservableObject
	{
		public const int NumOfSampels = 512;
		public const int InterruptFreq = 16000;


		public TriggerTypesEnum TriggerType { get; set; }
		public DeviceParameterData TriggerKeyword { get; set; }
		public int TriggerValue { get; set; }
		public TriggerPositionTypesEnum TriggerPosition { get; set; }

		private double _interval;
		[JsonIgnore]
		public double Interval 
		{
			get => _interval;
			set
			{
				_interval = value;

				_recordGap = (int)((_interval / NumOfSampels) * InterruptFreq);

				OnPropertyChanged(nameof(RecordGap));
			}
		}

		private int _recordGap;
		public int RecordGap
		{
			get => _recordGap;
			set
			{
				_recordGap = value;

				_interval = ((double)_recordGap / InterruptFreq) * NumOfSampels;

				OnPropertyChanged(nameof(Interval));
			}
		}




		public TriggerSelectionData()
		{
			Interval = 1;
		}

	}
}
