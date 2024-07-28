
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Models;
using Entities.Models;
using MCUScope.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

		
		[JsonIgnore]
		public double Interval 
		{
			get
			{
				return GetValueInUnits(_interval);
			}
			set
			{
				_interval = GetValueInSecs(value);

				if(_interval < _recordIntervalMin)
					_interval = _recordIntervalMin;
				if(_interval > _recordIntervalMax)
					_interval = _recordIntervalMax;

				SetUnits();

				double d = _interval * PhasesFrequency;
				RecordGap = (int)(d - 1);

				OnPropertyChanged(nameof(RecordGap));
			}
		}

		[JsonIgnore]
		public string IntervalUnits { get; set; }

		[JsonIgnore]
		public double RecordIntervalStep 
		{
			get
			{
				return GetValueInUnits(_recordIntervalStep);
			}
			set
			{
				_recordIntervalStep = value;
			}
		}

		[JsonIgnore]
		public double RecordIntervalMin
		{
			get
			{
				return GetValueInUnits(_recordIntervalMin);
			}
			set
			{
				_recordIntervalMin = value;
			}
		}

		[JsonIgnore]
		public double RecordIntervalMax
		{
			get
			{
				return GetValueInUnits(_recordIntervalMax);
			}
			set
			{
				_recordIntervalMax = value;
			}
		}


		public int RecordGap { get; set; }


		private double _interval;
		private double _recordIntervalStep;
		private double _recordIntervalMin;
		private double _recordIntervalMax;

		public TriggerSelectionData()
		{
			Interval = 1;
			IntervalUnits = "sec";
		}

		private double GetValueInUnits(double value)
		{
			double scaledValue = value;
			if (IntervalUnits == "ms")
				scaledValue = value * 1000;
			else if (IntervalUnits == "µs")
				scaledValue = value * 1000000;
			
			return scaledValue;
		}

		private double GetValueInSecs(double value)
		{
			if (IntervalUnits == "ms")
				return value / 1000;
			else if (IntervalUnits == "µs")
				return value / 1000000;

			return value;
		}

		private void SetUnits()
		{
			double scaledValue = _interval;

			if (_interval >= 1)
			{
				IntervalUnits = "sec";
			}
			else if (_interval < 1 && _interval >= (1 / 1000))
			{
				IntervalUnits = "ms";
			}
			else if ((_interval * 1000) < 1)
			{
				IntervalUnits = "µs";
			}

			OnPropertyChanged(nameof(Interval));
			OnPropertyChanged(nameof(RecordIntervalStep));
			OnPropertyChanged(nameof(RecordIntervalMin));
			OnPropertyChanged(nameof(RecordIntervalMax));
		}
	}
}
