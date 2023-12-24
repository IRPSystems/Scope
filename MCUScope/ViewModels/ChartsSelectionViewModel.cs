
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using Entities.Models;
using MCUScope.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MCUScope.ViewModels
{
	public class ChartsSelectionViewModel:ObservableObject
	{

		#region Properties

		public ObservableCollection<ChartSeriesSelectionViewModel> ChartsSelectionsList { get; set; }

		#endregion Properties

		#region Fields

		private DevicesContainer _devicesContainter;

		#endregion Fields

		#region Constructor

		public ChartsSelectionViewModel(DeviceData mcuDevice)
		{
			AddNewChartCommand = new RelayCommand(AddNewChart);
			DeleteCommand = new RelayCommand(Delete);

			ChartsSelectionsList = new ObservableCollection<ChartSeriesSelectionViewModel>();

			InitDeviceContainter(mcuDevice);
		}

		#endregion Constructor

		#region Methods

		private void InitDeviceContainter(DeviceData mcuDevice)
		{
			_devicesContainter = new DevicesContainer();
			_devicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			_devicesContainter.DevicesList = new ObservableCollection<DeviceData>();
			_devicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();
			DeviceFullData deviceFullData = DeviceFullData.Factory(mcuDevice);
			_devicesContainter.DevicesFullDataList.Add(deviceFullData);
			_devicesContainter.DevicesList.Add(mcuDevice);
			_devicesContainter.TypeToDevicesFullData.Add(mcuDevice.DeviceType, deviceFullData);
		}

		public void AddNewChart(ChartData chartData)
		{
			AddNewChart();
			ChartSeriesSelectionViewModel chartSeriesSelection =
				ChartsSelectionsList[ChartsSelectionsList.Count - 1];
			foreach (SeriesData seriesData in chartData.SeriesesList)
			{
				chartSeriesSelection.AddSeries(seriesData.Name, seriesData.Color);
			}
		}

		private void AddNewChart()
		{
			ChartSeriesSelectionViewModel chartSeriesSelection = 
				new ChartSeriesSelectionViewModel(_devicesContainter);
			ChartsSelectionsList.Add(chartSeriesSelection);
			chartSeriesSelection.AddSeriesEvent += AddSeriesEventHandler;
			chartSeriesSelection.DeleteSeriesEvent += DeleteSeriesEventHandler;

			SetChartsName();

			AddChartEvent?.Invoke(chartSeriesSelection.ChartName);
		}

		private void Delete()
		{
			ChartSeriesSelectionViewModel selectedItem = null;
			foreach (ChartSeriesSelectionViewModel item in ChartsSelectionsList)
			{
				if(item.IsSelected)
				{
					selectedItem = item;
					break;
				}
			}

			if (selectedItem == null)
				return;

			ChartsSelectionsList.Remove(selectedItem);

			SetChartsName();

			DeleteChartEvent?.Invoke(selectedItem.ChartName);
		}

		private void SetChartsName()
		{
			int counter = 1;
			foreach (ChartSeriesSelectionViewModel item in ChartsSelectionsList)
			{
				item.ChartName = "Chart " + counter++;
			}
		}


		private void AddSeriesEventHandler(string chartName, DeviceParameterData param)
		{
			AddSeriesEvent?.Invoke(
				chartName,
				param.Name);
		}

		private void DeleteSeriesEventHandler(string chartName, DeviceParameterData param)
		{
			DeleteSeriesEvent?.Invoke(
				chartName,
				param.Name);
		}


		public int GetNumOfSerieses()
		{
			int numOfSerieses = 0;
			foreach(ChartSeriesSelectionViewModel chart in ChartsSelectionsList)
			{
				numOfSerieses += chart.ParametersList.Count;
			}

			return numOfSerieses;
		}

		public List<DeviceParameterData> GetParamsList()
		{
			List<DeviceParameterData> paramsList = new List<DeviceParameterData>();
			foreach (ChartSeriesSelectionViewModel chart in ChartsSelectionsList)
			{
				foreach(DeviceParameterData param in chart.ParametersList)
					paramsList.Add(param);
			}

			return paramsList;
		}

		#endregion Methods

		#region Commands

		public RelayCommand AddNewChartCommand { get; private set; }
		public RelayCommand DeleteCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action<string> AddChartEvent;
		public event Action<string> DeleteChartEvent;
		public event Action<string, string> AddSeriesEvent;
		public event Action<string, string> DeleteSeriesEvent;

		#endregion Events
	}
}
