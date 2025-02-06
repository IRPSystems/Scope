
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

				LoggerService.Inforamtion(this, $"*** Interval = {_interval}");


				SetIntervalUnits();
				FromIntervalToSecs();

				if(_intervalInSecs > RecordIntervalMaxInSec && RecordIntervalMaxInSec != 0)
				{
					_interval = RecordIntervalMax;

					SetIntervalUnits();
					FromIntervalToSecs();
				}

				if (_intervalInSecs < RecordIntervalMinInSec && RecordIntervalMinInSec != 0)
				{
					_interval = RecordIntervalMin;

					SetIntervalUnits();
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

		public double RecordIntervalStep { get; set; }
		public double RecordIntervalMin { get; set; }
		public double RecordIntervalMax { get; set; }

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

			switch (Units)
			{
				case SecUnits: IntervalInSecs = _interval; break;
				case MiliSecUnits: IntervalInSecs = _interval / 1000; break;
				case MicroSecUnits: IntervalInSecs = _interval / 1000000; break;
			}

			OnPropertyChanged(nameof(Interval));

			_isSettingInterval = false;
		}

		private void FromSecsToInterval()
		{
			if (_isSettingInterval)
				return;

			_isSettingSecs = true;

			Units = SecUnits;
			Interval = IntervalInSecs;

			_isSettingSecs = false;
		}

		private void SetIntervalUnits()
		{
			if(_interval == 0)
				return;

			switch (Units)
			{
				case SecUnits: 
					if(_interval < 1 && _interval >= 0.001)
					{
						Units = MiliSecUnits;
						_interval = _interval * 1000;

						RecordIntervalStep = RecordIntervalStepInSec * 1000;
						RecordIntervalMin = RecordIntervalMinInSec * 1000;
						RecordIntervalMax = RecordIntervalMaxInSec * 1000;
					}
					if(_interval < 0.001)
					{
						Units = MicroSecUnits;
						_interval = _interval * 1000000;

						RecordIntervalStep = RecordIntervalStepInSec * 1000000;
						RecordIntervalMin = RecordIntervalMinInSec * 1000000;
						RecordIntervalMax = RecordIntervalMaxInSec * 1000000;
					}
					break;
				case MiliSecUnits:
					if (_interval > 1000)
					{
						Units = SecUnits;
						_interval = _interval / 1000;

						RecordIntervalStep = RecordIntervalStepInSec;
						RecordIntervalMin = RecordIntervalMinInSec;
						RecordIntervalMax = RecordIntervalMaxInSec;
					}
					else if (_interval < 1)
					{
						Units = MicroSecUnits;
						_interval = _interval * 1000;

						RecordIntervalStep = RecordIntervalStepInSec * 1000000;
						RecordIntervalMin = RecordIntervalMinInSec * 1000000;
						RecordIntervalMax = RecordIntervalMaxInSec * 1000000;
					}
					break;
				case MicroSecUnits:
					if (_interval >= 1000)
					{
						Units = MiliSecUnits;
						_interval = _interval / 1000;

						RecordIntervalStep = RecordIntervalStepInSec * 1000;
						RecordIntervalMin = RecordIntervalMinInSec * 1000;
						RecordIntervalMax = RecordIntervalMaxInSec * 1000;
					}
					break;
			}

			OnPropertyChanged(nameof(Interval));
		}

		#endregion Methods

	}
}
