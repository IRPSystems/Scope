
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models;
using DeviceHandler.ViewModel;
using Entities.Models;
using Services.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System;
using System.Windows.Media;
using DeviceCommunicators.MCU;
using System.Linq;
using DeviceCommunicators.Models;
using MCUScope.Models;
using System.Collections;

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

			DeleteParameterLogListCommand = new RelayCommand<IList>(DeleteParameterLogList);


			DragDropData dragDropData = new DragDropData();
			FullParametersList = new ParametersViewModel(dragDropData, _devicesContainer, false);
			FullParametersList.DevicesList[0].IsExpanded = true;
			FullParametersList.ParamDoubleClickedEvent += ParamDoubleClickedEventHandler;
		}

		#endregion Constructor

		#region Methods

		public void LoadMcuDevice()
		{
			FullParametersList.BuildDevicesList();
		}

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


		private void DeleteParameterLogList(IList selectedParams)
		{
			List<SelectedParameterData> list = new List<SelectedParameterData>();
			foreach (var param in selectedParams)
				list.Add(param as SelectedParameterData);


			foreach(var param in list)
			{
				ParametersList.Remove(param);
				DeleteSeriesEvent?.Invoke(ChartName, param.Parameter);
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

				var data = e.Data.GetData(ParametersViewModel.DragDropFormat);
				Type type = data.GetType();
				if(e.Data.GetData(ParametersViewModel.DragDropFormat) is DeviceParameterData param)
					AddParamToLogList(param);
				else if (e.Data.GetData(ParametersViewModel.DragDropFormat) is List<object> list)
					AddParamToLogList(list[0] as DeviceParameterData);

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


		private void ParamDoubleClickedEventHandler(DeviceParameterData param)
		{
			AddParamToLogList(param);
		}

		#endregion Methods

		#region Commands

		public RelayCommand<IList> DeleteParameterLogListCommand { get; private set; }


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

		#endregion Commands

		#region Events

		public event Action<string, DeviceParameterData> AddSeriesEvent;
		public event Action<string, DeviceParameterData> DeleteSeriesEvent;

		#endregion Events
	}
}
