﻿
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using DeviceHandler.ViewModel;
using Entities.Models;
using Services.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System;
using System.Windows.Media;
using DeviceCommunicators.MCU;
using System.Linq;
using DeviceCommunicators.Models;
using MCUScope.Models;

namespace MCUScope.ViewModels
{
	public class ChartSeriesSelectionViewModel: ObservableObject
	{

		

		#region Properties

		public string ChartName { get; set; }

		public ParametersViewModel FullParametersList { get; set; }

		public ObservableCollection<SelectedParameterData> ParametersList { get; set; }

		public bool IsSelected { get; set; }

		public bool IsPlayEnabled { get; set; }
		public bool IsStopEnabled { get; set; }

		#endregion Properties

		#region Fields

		private const int _maxLoggingParams = 40;




		private DevicesContainer _devicesContainer;



		private System.Collections.IList _selectedItemsList;

		#endregion Fields

		#region Constructor

		public ChartSeriesSelectionViewModel(
			DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			ChartName = "Chart";


			IsPlayEnabled = false;
			IsStopEnabled = false;

			ParametersList = new ObservableCollection<SelectedParameterData>();

			DeletParameterLogListCommand = new RelayCommand<System.Collections.IList>(DeletParameterLogList);


			DragDropData dragDropData = new DragDropData();
			FullParametersList = new ParametersViewModel(dragDropData, _devicesContainer, false);
			FullParametersList.DevicesList[0].IsExpanded = true;
			FullParametersList.ParamDoubleClickedEvent += ParamDoubleClickedEventHandler;
		}

		#endregion Constructor

		#region Methods

		public void Dispose()
		{
		}

		public void AddSeries(string seriesName, Brush color)
		{
			MCU_DeviceData mcuDevce = 
				FullParametersList.DevicesList[0] as MCU_DeviceData;
			DeviceParameterData param = mcuDevce.MCU_FullList.ToList().Find((p) => p.Name == seriesName);
			if (param == null)
				return;

			SelectedParameterData selectedParameterData = new SelectedParameterData()
			{  Parameter = param };

			ParametersList.Add(selectedParameterData);
		}


		private void DeletParameterLogList(System.Collections.IList paramsList)
		{
			List<SelectedParameterData> list = new List<SelectedParameterData>();
			foreach (SelectedParameterData data in paramsList)
				list.Add(data);

			foreach (SelectedParameterData data in list)
			{
				ParametersList.Remove(data);
				DeleteSeriesEvent?.Invoke(ChartName, data.Parameter);
			}

		}



		#region Drop

		private void LoggindList_Drop(DragEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is dropped");

			if (e.Data.GetDataPresent(ParametersViewModel.DragDropFormat))
			{
				if (ParametersList.Count == _maxLoggingParams)
				{
					MessageBox.Show("Only up to 40 parameters are allowed");
					return;
				}

				DeviceParameterData param = e.Data.GetData(ParametersViewModel.DragDropFormat) as DeviceParameterData;
				AddParamToLogList(param);
			}


		}

		private void AddParamToLogList(DeviceParameterData param)
		{
			SelectedParameterData selectedParameterData = ParametersList.ToList().Find((sp) => sp.Parameter == param);
			if (selectedParameterData != null)
			{
				MessageBox.Show("The parameter already exist");
				return;
			}


			ParametersList.Add(new SelectedParameterData() { Parameter = param});
			AddSeriesEvent?.Invoke(ChartName, param);
		}


		private void LoggindList_DragEnter(DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(ParametersViewModel.DragDropFormat))
			{
				e.Effects = DragDropEffects.None;
			}
		}

		#endregion Drop		

		

		private void LoggindList_SelectionChanged(SelectionChangedEventArgs e)
		{
			if (!(e.Source is ListView lv))
				return;

			_selectedItemsList = lv.SelectedItems;
		}

		private void LoggindList_KeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				DeletParameterLogList(_selectedItemsList);
			}
		}

		private void ParamDoubleClickedEventHandler(DeviceParameterData param)
		{
			AddParamToLogList(param);
		}

		#endregion Methods

		#region Commands

		public RelayCommand<System.Collections.IList> DeletParameterLogListCommand { get; private set; }



		#region Drop

		private RelayCommand<DragEventArgs> _LoggindList_DropCommand;
		public RelayCommand<DragEventArgs> LoggindList_DropCommand
		{
			get
			{
				return _LoggindList_DropCommand ?? (_LoggindList_DropCommand =
					new RelayCommand<DragEventArgs>(LoggindList_Drop));
			}
		}

		private RelayCommand<DragEventArgs> _LoggindList_DragEnterCommand;
		public RelayCommand<DragEventArgs> LoggindList_DragEnterCommand
		{
			get
			{
				return _LoggindList_DragEnterCommand ?? (_LoggindList_DragEnterCommand =
					new RelayCommand<DragEventArgs>(LoggindList_DragEnter));
			}
		}

		#endregion Drop



		private RelayCommand<SelectionChangedEventArgs> _LoggindList_SelectionChangedCommand;
		public RelayCommand<SelectionChangedEventArgs> LoggindList_SelectionChangedCommand
		{
			get
			{
				return _LoggindList_SelectionChangedCommand ?? (_LoggindList_SelectionChangedCommand =
					new RelayCommand<SelectionChangedEventArgs>(LoggindList_SelectionChanged));
			}
		}

		private RelayCommand<KeyEventArgs> _LoggindList_KeyDownCommand;
		public RelayCommand<KeyEventArgs> LoggindList_KeyDownCommand
		{
			get
			{
				return _LoggindList_KeyDownCommand ?? (_LoggindList_KeyDownCommand =
					new RelayCommand<KeyEventArgs>(LoggindList_KeyDown));
			}
		}

		#endregion Commands

		#region Events

		public event Action<string, DeviceParameterData> AddSeriesEvent;
		public event Action<string, DeviceParameterData> DeleteSeriesEvent;

		#endregion Events
	}
}
