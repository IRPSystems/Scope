﻿
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

		public bool IsContinuous { get; set; }

		public DeviceData McuDevice { get; set; }


		private bool isKeywordRadioChecked = false;

		#endregion Propeties and Fields


		#region Constructor

		public TriggerSelectionViewModel(DeviceData mcuDevice)
		{
			McuDevice = mcuDevice;

			ContinuousCommand = new RelayCommand(Continuous);
			ExpandAllCommand = new RelayCommand(ExpandAll);
			CollapseAllCommand = new RelayCommand(CollapseAll);

			TriggerData = new TriggerSelectionData();

			IsContinuous = false;

			TriggerData.RecordIntervalStepInSec = 1;
		}

		#endregion Constructor

		#region Methods

		private void Continuous()
		{
			ContinuousEvent?.Invoke(IsContinuous);
		}

		public void SetPhasesFrequency(uint phasesFrequency)
		{
			TriggerData.PhasesFrequency = phasesFrequency;

			TriggerData.RecordIntervalStepInSec = 1.0 / TriggerData.PhasesFrequency;
			TriggerData.RecordIntervalMinInSec = TriggerData.RecordIntervalStepInSec;
			TriggerData.RecordIntervalMaxInSec = 255.0 / TriggerData.PhasesFrequency;

			TriggerData.IntervalInSecs = TriggerData.RecordIntervalStepInSec;
		}


		#region Search parameter

		private void DeviceParamSearch_TextChanged(TextChangedEventArgs e)
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

		public RelayCommand ContinuousCommand { get; private set; }

		public RelayCommand ExpandAllCommand { get; private set; }
		public RelayCommand CollapseAllCommand { get; private set; }

		private RelayCommand<TextChangedEventArgs> _DeviceParamSearch_TextChangedCommand;
		public RelayCommand<TextChangedEventArgs> DeviceParamSearch_TextChangedCommand
		{
			get
			{
				return _DeviceParamSearch_TextChangedCommand ?? (_DeviceParamSearch_TextChangedCommand =
					new RelayCommand<TextChangedEventArgs>(DeviceParamSearch_TextChanged));
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

		#region Event

		public event Action<bool> ContinuousEvent;

		#endregion Event
	}
}
