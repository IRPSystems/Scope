
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using MCUScope.Models;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MCUScope.ViewModels
{
	public class TriggerSelectionViewModel: ObservableObject
	{
		#region Propeties and Fields

		private TriggerSelectionData _triggerData;
		public TriggerSelectionData TriggerData 
		{
			get => _triggerData;
			set
			{
				_triggerData = value;
				if(_triggerData != null) 
				{
					TriggerData_PropertyChangedEventHandler();
				}
			}
		}

		

		public DeviceData McuDevice { get; set; }

		public double RecordIntervalStep { get; set; }
		public double RecordIntervalMin { get; set; }
		public double RecordIntervalMax { get; set; }



		//private uint _phasesFrequency;

		#endregion Propeties and Fields


		#region Constructor

		public TriggerSelectionViewModel(DeviceData mcuDevice)
		{
			McuDevice = mcuDevice;

			
			ExpandAllCommand = new RelayCommand(ExpandAll);
			CollapseAllCommand = new RelayCommand(CollapseAll);

			TriggerData = new TriggerSelectionData();

			
			RecordIntervalStep = 1;
		}

		#endregion Constructor

		#region Methods

		

		public void SetPhasesFrequency(uint phasesFrequency)
		{
			TriggerData.PhasesFrequency = phasesFrequency;

			RecordIntervalStep = (1.0 * 1000) / (double)TriggerData.PhasesFrequency;
			RecordIntervalMin = RecordIntervalStep;
			RecordIntervalMax = (255.0 * 1000) / (double)TriggerData.PhasesFrequency;

			TriggerData.Interval = RecordIntervalStep;
		}


		#region Search parameter

		private void DeviceParamSearch_Text(TextChangedEventArgs e)
		{
			if (!(e.Source is TextBox tb))
				return;

			SetSearchedTest(tb.Text);
		}

		private void SetSearchedTest(string text)
		{
				HideShowParameters(McuDevice.ParemetersList, text);

				if (McuDevice is MCU_DeviceData mcu_Device)
					mcu_Device.HideNotVisibleGroups();
			
		}

		private void HideShowParameters(
			ObservableCollection<MCU_ParamData> list,
			string text)
		{
			foreach (DeviceParameterData data in list)
			{
				if (data is ParamGroup group)
				{
					HideShowParameters(group.ParamList, text);
					continue;
				}

				if (data.Name.ToLower().Contains(text.ToLower()))
					data.Visibility = Visibility.Visible;
				else
					data.Visibility = Visibility.Collapsed;
			}
		}

		private void HideShowParameters(
			ObservableCollection<DeviceParameterData> list,
			string text)
		{
			foreach (DeviceParameterData data in list)
			{
				if (data is ParamGroup group)
				{
					HideShowParameters(group.ParamList, text);
					continue;
				}

				if (data.Name.ToLower().Contains(text.ToLower()))
					data.Visibility = Visibility.Visible;
				else
					data.Visibility = Visibility.Collapsed;
			}
		}



		#endregion Search parameter


		#region Expand/Collapse

		private void ExpandAll()
		{
			LoggerService.Inforamtion(this, "Expanding all the devices");

			if (!(McuDevice is MCU_DeviceData mcu_Device))
				return;

			foreach (ParamGroup group in mcu_Device.MCU_GroupList)
				group.IsExpanded = true;
		}

		private void CollapseAll()
		{
			LoggerService.Inforamtion(this, "Collapsing all the devices");

			if (!(McuDevice is MCU_DeviceData mcu_Device))
				return;

			foreach (ParamGroup group in mcu_Device.MCU_GroupList)
				group.IsExpanded = false;
		}

		#endregion Expand/Collapse

		private bool isKeywordRadioChecked = false;
		private void KeywordRadioChecked(DeviceParameterData selectedParameter)
		{
			isKeywordRadioChecked = true;
			TriggerData.TriggerKeyword = selectedParameter;
			isKeywordRadioChecked = false;
		}

		private void TriggerData_PropertyChangedEventHandler()
		{
			if (isKeywordRadioChecked)
				return;

			if (TriggerData.TriggerKeyword == null)
				return;

			CollapseAll();

			if (!(McuDevice is MCU_DeviceData mcu_Device))
				return;

			foreach (ParamGroup group in mcu_Device.MCU_GroupList)
			{
				DeviceParameterData param = group.ParamList.ToList().Find((p) => p.Name == TriggerData.TriggerKeyword.Name);
				if(param != null) 
				{ 
					param.IsSelected = true;
					group.IsExpanded = true;
					break;
				}
			}

			OnPropertyChanged(nameof(McuDevice));

		}



		#endregion Methods

		#region Commands


		public RelayCommand ExpandAllCommand { get; private set; }
		public RelayCommand CollapseAllCommand { get; private set; }

		private RelayCommand<TextChangedEventArgs> _DeviceParamSearch_TextChanged;
		public RelayCommand<TextChangedEventArgs> DeviceParamSearch_TextChanged
		{
			get
			{
				return _DeviceParamSearch_TextChanged ?? (_DeviceParamSearch_TextChanged =
					new RelayCommand<TextChangedEventArgs>(DeviceParamSearch_Text));
			}
		}

		private RelayCommand<DeviceParameterData> _KeywordRadioCheckedCommand;
		public RelayCommand<DeviceParameterData> KeywordRadioCheckedCommand
		{
			get
			{
				return _KeywordRadioCheckedCommand ?? (_KeywordRadioCheckedCommand =
					new RelayCommand<DeviceParameterData>(KeywordRadioChecked));
			}
		}

		#endregion Commands
	}
}
