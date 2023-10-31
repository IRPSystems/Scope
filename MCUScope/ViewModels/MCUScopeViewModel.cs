using Communication.Services;
using CommunityToolkit.Mvvm.Input;
using Controls.ViewModels;
using DeviceCommunicators.MCU;
using Entities.Enums;
using Entities.Models;
using MCUScope.Models;
using MCUScope.Services;
using MCUScope.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using Scope.ViewModels;
using Scope.Views;
using Services.Services;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MCUScope.ViewModels
{
	public class MCUScopeViewModel : DocingBaseViewModel
	{
		#region Properties

		public TriggerSelectionViewModel TriggerSelection { get; set; }
		public ChartsSelectionViewModel ChartsSelection { get; set; }
		public ScopeViewModel Scope { get; set; }

		private CanService _canService;
		public CanService CanService
		{
			get => _canService;
			set
			{
				_canService = value;
				_canService.CanMessageReceivedEvent += MessageReceivedEventHandler;
			}
		}


		#region RecodStateColor

		public static readonly DependencyProperty RecodStateColorProperty =
		 DependencyProperty.Register("RecodStateColor", typeof(Brush), typeof(MCUScopeViewModel));

		public Brush RecodStateColor
		{
			get { return (Brush)GetValue(RecodStateColorProperty); }
			set { SetValue(RecodStateColorProperty, value); }
		}

		#endregion RecodStateColor

		#region RecodStateDescription

		public static readonly DependencyProperty RecodStateDescriptionProperty =
		 DependencyProperty.Register("RecodStateDescription", typeof(string), typeof(MCUScopeViewModel));

		public string RecodStateDescription
		{
			get { return (string)GetValue(RecodStateDescriptionProperty); }
			set { SetValue(RecodStateDescriptionProperty, value); }
		}

		#endregion RecodStateDescription

		#region DataPercentage

		public static readonly DependencyProperty DataPercentageProperty =
		 DependencyProperty.Register("DataPercentage", typeof(double), typeof(MCUScopeViewModel));

		public double DataPercentage
		{
			get { return (double)GetValue(DataPercentageProperty); }
			set { SetValue(DataPercentageProperty, value); }
		}

		#endregion DataPercentage

		#endregion Properties

		#region Fields

		
		private BuildRequestMessagesService _buildRequestMessages;

		private ContentControl _triggerSelectionWindow;
		private ContentControl _chartsSelectionWindow;
		private ContentControl _scopeWindow;

		private int _chartIndex;
		private int _seriesIndex;
		private double _chartTimeMS;
		private int _chartPointsCounter;

		private bool _isTriggerReceived;
		private double _interval;


		public DeviceData MCUDevice;

		private System.Timers.Timer _timerRecordTime;
		private double _percentageBeforTrigger;
		private double _intervalPercentage;
		
		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		#endregion Fields

		#region Constroctur

		public MCUScopeViewModel() :
			base("MCUScopeLayout", "MCUScope")
		{

			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
				"MjQ2MzU2NkAzMjMwMmUzMzJlMzBOaGhMVVJBelp0Y1c1eXdoNHRTcHI4bGVOdmdxQWNXZkZxeklweENobmdjPQ==");

			_chartIndex = 0;
			_seriesIndex = 0;
			_chartTimeMS = 0;
			_chartPointsCounter = 0;

			SaveSetupCommand = new RelayCommand(SaveSetup);
			LoadSetuptCommand = new RelayCommand(LoadSetup);
			ExecuteCommand = new RelayCommand(Execute);
			ForceTrigCommand = new RelayCommand(ForceTrig);

			DockFill = true;

			MCUDevice = ReadFromMCUJson(@"param_defaults.json");

			TriggerSelection = new TriggerSelectionViewModel(MCUDevice);
			TriggerSelection.ContinuousEvent += ContinuousEventHandler;

			ChartsSelection = new ChartsSelectionViewModel(MCUDevice);
			ChartsSelection.AddChartEvent += AddChartEventHandler;
			ChartsSelection.DeleteChartEvent += DeleteChartEventHandler;
			ChartsSelection.AddSeriesEvent += AddSeriesEventHandler;
			ChartsSelection.DeleteSeriesEvent += DeleteSeriesEventHandler;

			Scope = new ScopeViewModel();

			ScopeView scopeView = new ScopeView() { DataContext = Scope };
			CreateWindow(scopeView, "Scope", out _scopeWindow);

			ChartsSelectionView chartsSelectionView = new ChartsSelectionView() { DataContext = ChartsSelection };
			CreateWindow(chartsSelectionView, "CreateCharts", out _chartsSelectionWindow);


			TriggerSelectionView triggerView = new TriggerSelectionView() { DataContext = TriggerSelection };
			CreateWindow(triggerView, "Trigger", out _triggerSelectionWindow);



			_buildRequestMessages = new BuildRequestMessagesService();


			RecodStateColor = Application.Current.MainWindow.FindResource("MahApps.Brushes.Gray5") as SolidColorBrush;
			RecodStateDescription = "Not recording";

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			_timerRecordTime = new System.Timers.Timer();
			_timerRecordTime.Elapsed += RecordTimeElapsedEventHandler;

		}

		#endregion Constroctur

		#region Methods

		public new void Dispose()
		{
			base.Dispose();

			_timerRecordTime.Stop();
			_cancellationTokenSource.Cancel();
			CanService.Dispose(); 
		}

		private DeviceData ReadFromMCUJson(string path)
		{
			MCU_DeviceData deviceData = new MCU_DeviceData("MCU", DeviceTypesEnum.MCU);
			MCU_ListHandlerService mcu_ListHandler = new MCU_ListHandlerService();
			mcu_ListHandler.ReadMCUDeviceData(path, deviceData);
			return deviceData;
		}

		private void CreateWindow(
			UserControl userControl,
			string name,
			out ContentControl window)
		{
			window = new ContentControl();
			window.Name = name;
			window.Content = userControl;


			SetHeader(window, name);
			SetCanClose(window, false);
			Children.Add(window);
		}

		private void AddChartEventHandler(string chartName)
		{
			Scope.AddChart(chartName, "sec");
		}

		private void DeleteChartEventHandler(string chartName)
		{
			Scope.DeleteChart(chartName);
		}

		private void AddSeriesEventHandler(string chartName, string series)
		{
			Scope.AddSeriesToChart(chartName, series);
		}

		private void DeleteSeriesEventHandler(string chartName, string series)
		{
			Scope.DeleteSeriesFromChart(
				chartName,
				series);
		}


		#region Save / Load

		private void SaveSetup()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "Scope Setup files (*.setup)|*.setup";
			bool? result = saveFileDialog.ShowDialog();
			if (result != true)
				return;

			string path = saveFileDialog.FileName;


			ScopeSetupData scopeSetup = new ScopeSetupData();
			scopeSetup.TriggerData = TriggerSelection.TriggerData;
			scopeSetup.ChartsList = new List<ChartData>();

			foreach (ChartViewModel chart in Scope.ChartsList)
			{
				ChartData chartData = new ChartData();
				chartData.Name = chart.Name;
				chartData.SeriesesList = new List<SeriesData>();

				foreach (ChartSeries series in chart.Chart.Series)
				{
					SeriesData seriesData = new SeriesData();
					seriesData.Name = series.Label;
					seriesData.Color = series.Interior;
					chartData.SeriesesList.Add(seriesData);
				}

				scopeSetup.ChartsList.Add(chartData);
			}

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(scopeSetup, settings);
			File.WriteAllText(path, sz);
		}

		private void LoadSetup()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Scope Setup files (*.setup)|*.setup";
			bool? result = openFileDialog.ShowDialog();
			if (result != true)
				return;

			string path = openFileDialog.FileName;

			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			ScopeSetupData scopeSetup = JsonConvert.DeserializeObject(jsonString, settings) as ScopeSetupData;

			TriggerSelection.TriggerData = scopeSetup.TriggerData;
			foreach (ChartData chart in scopeSetup.ChartsList)
			{

				ChartsSelection.AddNewChart(chart);

				if (chart.SeriesesList == null)
					continue;


				foreach (SeriesData series in chart.SeriesesList)
				{
					Scope.AddSeriesToChart(chart.Name, series.Name, series.Color);
				}
			}
		}

		#endregion Save / Load

		private void ForceTrig()
		{
			byte[] data = _buildRequestMessages.BuildForceTriggerMessage();
			CanService.Send(data);
		}

		private List<List<List<double>>> _dataList;
		private void Execute()
		{
			_isTriggerReceived = false;
			_chartIndex = 0;
			_seriesIndex = 0;
			_chartTimeMS = 0;
			_chartPointsCounter = 0;

			int numOfParams = ChartsSelection.GetNumOfSerieses();
			byte[] data = _buildRequestMessages.BuildMessage1(
				numOfParams,
				TriggerSelection.TriggerData.RecordGap,
				TriggerSelection.IsContinuous,
				TriggerSelection.TriggerData.TriggerPosition);
			CanService.Send(data);
			System.Threading.Thread.Sleep(100);

			List<DeviceParameterData> paramsList = ChartsSelection.GetParamsList();
			foreach (DeviceParameterData param in paramsList)
			{
				data = _buildRequestMessages.BuildMessage2(param);
				if (data == null)
					return;

				CanService.Send(data);
				System.Threading.Thread.Sleep(100);
			}

			data = _buildRequestMessages.BuildMessage3(
				TriggerSelection.TriggerData.TriggerKeyword,
				TriggerSelection.TriggerData.TriggerType);
			if (data == null)
				return;
			CanService.Send(data);
			System.Threading.Thread.Sleep(100);

			data = _buildRequestMessages.BuildMessage4(
				TriggerSelection.TriggerData.TriggerValue);
			if (data == null)
				return;
			CanService.Send(data);
			System.Threading.Thread.Sleep(100);

			_dataList = new List<List<List<double>>>();
			foreach (ChartViewModel chart in Scope.ChartsList)
			{
				List<List<double>> chartDataList = new List<List<double>>();
				foreach (string series in chart.SeriesesList)
				{
					chartDataList.Add(new List<double>());
				}

				_dataList.Add(chartDataList);
			}

			RecodStateColor = Brushes.Red;
			RecodStateDescription = "Waiting for trigger";




			switch (TriggerSelection.TriggerData.TriggerPosition)
			{
				case Enums.TriggerPositionTypesEnum.ShowOnlyDataBeforeTriggerHappened: _percentageBeforTrigger = 100; break;
				case Enums.TriggerPositionTypesEnum.ShowData50BeforeAnd50AfterTrigger: _percentageBeforTrigger = 50; break;
				case Enums.TriggerPositionTypesEnum.ShowData25BeforeAnd75AfterTrigger: _percentageBeforTrigger = 25; break;
				case Enums.TriggerPositionTypesEnum.ShowData12_5BeforeAnd87_5AfterTrigger: _percentageBeforTrigger = 12.5; break;
				case Enums.TriggerPositionTypesEnum.ShowData6_25BeforeAnd93_75AfterTrigger: _percentageBeforTrigger = 6.25; break;
			}

			_interval = (double)TriggerSelection.TriggerData.RecordGap /
				(double)TriggerSelectionData.InterruptFreq;

			int timerInterval = 50;
			_intervalPercentage = (double)timerInterval * (_interval * 1000);
			_intervalPercentage *= (100.0 / (double)TriggerSelectionData.NumOfSampels);

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					DataPercentage = 0;
				});
			}

			_timerRecordTime.Interval = timerInterval;
			_timerRecordTime.Start();
		}

		private void ContinuousEventHandler(bool isContinuous)
		{
		}



		private void MessageReceivedEventHandler(uint canID, byte[] buffer)
		{
			if (canID != 0xAA)
				return;

			if (!_isTriggerReceived)
			{
				//_chartPointsCounter += 2;
				HeaderMessageReceived(buffer);
				return;
			}

			LoggerService.Inforamtion(this, "Setting chart, value no.: " + _chartPointsCounter);

			_chartPointsCounter++;
			int val = BitConverter.ToInt32(buffer, 0);
			SetSingleValue(val);

			_chartPointsCounter++;
			val = BitConverter.ToInt32(buffer, 4);
			SetSingleValue(val);

			if (_chartPointsCounter == TriggerSelectionData.NumOfSampels)
			{
				_timerRecordTime.Stop();

				
				for (int i = 0; i < Scope.ChartsList.Count; i++)
				{
					if (Application.Current != null)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							Scope.UpdateChart(
								_interval,
								_dataList[i],
								Scope.ChartsList[i].Name);
						});
					}

					System.Threading.Thread.Sleep(1);
				}

				
			}

		}

		private void HeaderMessageReceived(byte[] buffer)
		{
			Task.Run(() =>
			{
				ulong header = BitConverter.ToUInt64(buffer, 0);
				if (header == 0xABCDABCDABCDABCD)
				{
					_isTriggerReceived = true;

					

					if (Application.Current != null)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{

							RecodStateColor = Brushes.Green;
							RecodStateDescription = "Trigger found";
						});
					}

					

				}
			}, _cancellationToken);
		}


		private void SetSingleValue(int val)
		{
			try
			{
				if (_dataList == null)
					return;

				if (_chartIndex < 0 || _chartIndex >= _dataList.Count)
					return;

				List<List<double>> chartDataList = _dataList[_chartIndex];
				while (chartDataList.Count == 0)
				{
					HandleIndeces(chartDataList);
					chartDataList = _dataList[_chartIndex];
				}

				List<double> seriesDataList = null;
				seriesDataList = chartDataList[_seriesIndex];
				seriesDataList.Add(val);

				

				LoggerService.Inforamtion(this, "Setting series data, value no.: " + _chartPointsCounter + " Time: " + _chartTimeMS);

				HandleIndeces(chartDataList);
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set chart value", "Recording Error", ex);
			}
		}

		private void HandleIndeces(List<List<double>> chartDataList)
		{
			_seriesIndex++;
			if (_seriesIndex >= chartDataList.Count)
			{
				_seriesIndex = 0;
				_chartIndex++;

				if (_chartIndex >= _dataList.Count)
				{
					_chartIndex = 0;
					_chartTimeMS += 1000;
				}
			}



		}


		private void RecordTimeElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			if (Application.Current == null)
				return;

			


			Application.Current.Dispatcher.Invoke(() =>
			{
				if (!_isTriggerReceived)
				{
					if (DataPercentage < _percentageBeforTrigger)
						DataPercentage += _intervalPercentage;

					if (DataPercentage >= _percentageBeforTrigger)
						DataPercentage = _percentageBeforTrigger;
				}
				else
				{
					if (DataPercentage < 100)
						DataPercentage += _intervalPercentage;

					if (DataPercentage >= 100)
						DataPercentage = 100;
				}
			});

			
		}

		#endregion Methods

		#region Commands

		public RelayCommand SaveSetupCommand { get; private set; }
		public RelayCommand LoadSetuptCommand { get; private set; }
		public RelayCommand ExecuteCommand { get; private set; }
		public RelayCommand ForceTrigCommand { get; private set; }

		#endregion Commands

	}
}
