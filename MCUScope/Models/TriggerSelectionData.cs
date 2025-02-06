
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using Entities.Models;
using MCUScope.Enums;
using Newtonsoft.Json;
using Services.Services;
using System.Windows.Controls;

namespace MCUScope.Models
{
	public class TriggerSelectionData: ObservableObject
	{
		#region Constants

		private const string SecUnits = "sec";
		private const string MiliSecUnits = "ms";
		private const string MicroSecUnits = "µ";

		#endregion Constants

		#region Properties

		public uint PhasesFrequency { get; set; }

		public TriggerTypesEnum TriggerType { get; set; }
		public DeviceParameterData TriggerKeyword { get; set; }
		public int TriggerValue { get; set; }
		public TriggerPositionTypesEnum TriggerPosition { get; set; }



		public int RecordGap { get; set; }



		private double _interval;
		[JsonIgnore]
		public double Interval 
		{
			get => _interval;
			set
			{
				_interval = value;


				FromIntervalToSecs();

				if(_intervalInSecs > RecordIntervalMaxInSec && RecordIntervalMaxInSec != 0)
				{
					_interval = RecordIntervalMaxInSec * 1000;

					FromIntervalToSecs();
				}

				if (_intervalInSecs < RecordIntervalMinInSec && RecordIntervalMinInSec != 0)
				{
					_interval = RecordIntervalMinInSec * 1000;

					FromIntervalToSecs();
				}
			}
		}

		private double _intervalInSecs;
		public double IntervalInSecs 
		{
			get => _intervalInSecs; 
			set
			{
				_intervalInSecs = value;

				CalcGap();
				FromSecsToInterval();
			}
		}

		public double RecordIntervalStepInSec { get; set; }
		public double RecordIntervalMinInSec { get; set; }
		public double RecordIntervalMaxInSec { get; set; }


		public string Units { get; set; }

		#endregion Properties

		#region Fields

		private bool _isSettingSecs;
		private bool _isSettingInterval;

		#endregion Fields

		#region Constructor

		public TriggerSelectionData()
		{
			IntervalInSecs = 1;
		}

		#endregion Constructor

		#region Methods

		private void CalcGap()
		{
			double d = _intervalInSecs * PhasesFrequency;
			RecordGap = (int)(d - 1);

			OnPropertyChanged(nameof(RecordGap));
		}

		private void FromIntervalToSecs()
		{
			if (_isSettingSecs)
				return;

			_isSettingInterval = true;

			IntervalInSecs = _interval / 1000; 

			_isSettingInterval = false;
		}

		private void FromSecsToInterval()
		{
			if (_isSettingInterval)
				return;

			_isSettingSecs = true;

			Units = MiliSecUnits;
			Interval = IntervalInSecs * 1000;

			_isSettingSecs = false;
		}

		

		#endregion Methods

	}
}
