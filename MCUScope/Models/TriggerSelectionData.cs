
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Models;
using Entities.Models;
using MCUScope.Enums;
using Newtonsoft.Json;

namespace MCUScope.Models
{
	public class TriggerSelectionData: ObservableObject
	{
		public const int NumOfSampels = 256;
		//	public const int InterruptFreq = 16000;

		public uint PhasesFrequency { get; set; }

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

				double d = _interval * PhasesFrequency;
				RecordGap = (int)(d - 1);

				OnPropertyChanged(nameof(RecordGap));
			}
		}

		public int RecordGap { get; set; }




		public TriggerSelectionData()
		{
			Interval = 1;
		}

	}
}
