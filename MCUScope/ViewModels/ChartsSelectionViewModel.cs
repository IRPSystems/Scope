
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
using System.Windows;

namespace MCUScope.ViewModels
{
	public class ChartsSelectionViewModel:ObservableObject
	{

		#region Properties

		public ObservableCollection<ChartSeriesSelectionViewModel> ChartsSelectionsList { get; set; }

		#endregion Properties

		#region Fields

		private DevicesContainer _devicesContainter;

		private int _chartsCounter;

		#endregion Fields

		#region Constructor

		public ChartsSelectionViewModel(DeviceData mcuDevice)
		{
			_chartsCounter = 1;

			AddNewChartCommand = new RelayCommand(AddNewChart);
			DeleteCommand = new RelayCommand(Delete);
			ClearAllChartsCommand = new RelayCommand(ClearAllCharts);

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
			bool isAllowAddChart = IsAllowAddChartParameter();
			if(isAllowAddChart == false)
			{
				MessageBox.Show("There are already 2 parameters requested", "Error");
				return;
			}

			ChartSeriesSelectionViewModel chartSeriesSelection = 
				new ChartSeriesSelectionViewModel(_devicesContainter, IsAllowAddChartParameter);
			ChartsSelectionsList.Add(chartSeriesSelection);
			chartSeriesSelection.AddSeriesEvent += AddSeriesEventHandler;
			chartSeriesSelection.DeleteSeriesEvent += DeleteSeriesEventHandler;

			chartSeriesSelection.ChartName = "Chart " + _chartsCounter++;

			SetChartsName();

			AddChartEvent?.Invoke(
				chartSeriesSelection.ChartName, 
				chartSeriesSelection.ChartDisplayName);
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

		private void ClearAllCharts()
		{
			List<ChartSeriesSelectionViewModel> list =
				new List<ChartSeriesSelectionViewModel>();
			foreach (ChartSeriesSelectionViewModel item in ChartsSelectionsList)
				list.Add(item);

			foreach (ChartSeriesSelectionViewModel item in list)
			{
				ChartsSelectionsList.Remove(item);

				DeleteChartEvent?.Invoke(item.ChartName);
			}
		}

		private void SetChartsName()
		{
			int counter = 1;
			foreach (ChartSeriesSelectionViewModel item in ChartsSelectionsList)
			{
				item.ChartDisplayName = "Chart " + counter++;
			}
		}


		private void AddSeriesEventHandler(
			string chartName, 
			string chartDisplayName,
			DeviceParameterData param)
		{
			AddSeriesEvent?.Invoke(
				chartName,
				chartDisplayName,
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

		public List<SelectedParameterData> GetParamsList()
		{
			List<SelectedParameterData> paramsList = new List<SelectedParameterData>();
			foreach (ChartSeriesSelectionViewModel chart in ChartsSelectionsList)
			{
				foreach(SelectedParameterData param in chart.ParametersList)
					paramsList.Add(param);
			}

			return paramsList;
		}

		private bool IsAllowAddChartParameter()
		{
			int paramsCounter = 0;
			foreach (ChartSeriesSelectionViewModel chart in ChartsSelectionsList)
			{
				foreach (SelectedParameterData param in chart.ParametersList)
					paramsCounter++;
			}

			if (paramsCounter >= 2)
				return false;

			return true;
		}

		#endregion Methods

		#region Commands

		public RelayCommand AddNewChartCommand { get; private set; }
		public RelayCommand DeleteCommand { get; private set; }
		public RelayCommand ClearAllChartsCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action<string, string> AddChartEvent;
		public event Action<string> DeleteChartEvent;
		public event Action<string, string, string> AddSeriesEvent;
		public event Action<string, string> DeleteSeriesEvent;

		#endregion Events
	}
}
