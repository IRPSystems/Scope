using Communication.Services;
using CommunityToolkit.Mvvm.Input;
using Controls.ViewModels;
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using MCUScope.Models;
using MCUScope.Services;
using MCUScope.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using Scope.Models;
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
using TrueDriveCommunication;

namespace MCUScope.ViewModels
{
	public class MCUScopeViewModel : DocingBaseViewModel
	{
		


		#region Properties

		public TriggerSelectionViewModel TriggerSelection { get; set; }
		public ChartsSelectionViewModel ChartsSelection { get; set; }
		public ScopeViewModel Scope { get; set; }

		


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

		#region NumOfSamples

		public static readonly DependencyProperty NumOfSamplesProperty =
		 DependencyProperty.Register("NumOfSamples", typeof(int), typeof(MCUScopeViewModel));

		public int NumOfSamples
		{
			get { return (int)GetValue(NumOfSamplesProperty); }
			set { SetValue(NumOfSamplesProperty, value); }
		}

		#endregion NumOfSamples

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

		private CanService _canService;

		private MCU_ParamData _paramPhasesFrequency;

		private List<DataFromMCU_Cahrt> _dataList;

		#endregion Fields

		#region Constroctur

		public MCUScopeViewModel(CanService canService) :
			base("MCUScopeLayout", "MCUScope")
		{

			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
				"MjQ2MzU2NkAzMjMwMmUzMzJlMzBOaGhMVVJBelp0Y1c1eXdoNHRTcHI4bGVOdmdxQWNXZkZxeklweENobmdjPQ==");


			if (canService != null)
			{
				_canService = canService;
				_canService.CanMessageReceivedEvent += CanMessageReceivedEventHandler;
				_canService.MessageReceivedEvent += MessageReceivedEventHandler; ;
				MCUDevice = ReadFromMCUJson(@"param_defaults.json");
			}
			else
			{
				ComPort._canbusControl.GetCanDriver().CanService.CanMessageReceivedEvent += CanMessageReceivedEventHandler;
				ComPort._canbusControl.GetCanDriver().CanService.MessageReceivedEvent += MessageReceivedEventHandler;
				DeviceFullData deviceFullData = ComPort._canbusControl.DevicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];
				MCUDevice = deviceFullData.Device;
			}

			_chartIndex = 0;
			_seriesIndex = 0;
			_chartTimeMS = 0;
			_chartPointsCounter = 0;

			SaveSetupCommand = new RelayCommand(SaveSetup);
			LoadSetuptCommand = new RelayCommand(LoadSetup);
			ExecuteCommand = new RelayCommand(Execute);
			ForceTrigCommand = new RelayCommand(ForceTrig);

			DockFill = true;

			TriggerSelection = new TriggerSelectionViewModel(MCUDevice);

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


			RecodStateColor = Application.Current.FindResource("MahApps.Brushes.Gray5") as SolidColorBrush;
			RecodStateDescription = "Not recording";

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			_timerRecordTime = new System.Timers.Timer();
			_timerRecordTime.Elapsed += RecordTimeElapsedEventHandler;


			_paramPhasesFrequency = new MCU_ParamData()
			{
				Cmd = "pwmfreq",
				Name = "pwmPhasesFrequency"
			};

			byte[] idBuf = new byte[3];
			byte[] buffer = new byte[8];
			MCU_Communicator.ConvertToData(_paramPhasesFrequency, 0, ref idBuf, ref buffer, false);
			if (_canService == null)
				ComPort._canbusControl.GetCanDriver().CanService.Send(buffer, 0xAB, false);
			else
				_canService.Send(buffer, 0xAB, false);


			// Request the number of samples
			MCU_ParamData param = new MCU_ParamData()
			{
				Cmd = "recbufsize"
			};

			ComPort._canbusControl.SendAndResponse(param, null, Callback);
		}

		

		#endregion Constroctur

		#region Methods

		public new void Dispose()
		{
			base.Dispose();

			_timerRecordTime.Stop();
			_cancellationTokenSource.Cancel();
		}

		private void Callback(DeviceParameterData param, CommunicatorResultEnum status, string errDescription)
		{
			if (param.Value == null)
				return;

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					NumOfSamples = Convert.ToInt32(param.Value);
				});
			}
			
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

		private void AddChartEventHandler(string chartName, string chartDisplayName)
		{
			Scope.AddChart(chartName, chartDisplayName, "sec", ChartViewModel.XAxisTypes.Double);
		}

		private void DeleteChartEventHandler(string chartName)
		{
			Scope.DeleteChart(chartName);
		}

		private void AddSeriesEventHandler(
			string chartName,
			string chartDisplayName,
			string series)
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
			//_mcu_Communicator.SendMessage(false, 0xAB, data, null);
			Send(data);
		}

		
		private void Execute()
		{
			_isTriggerReceived = false;
			_chartTimeMS = 0;
			_chartPointsCounter = 0;

			int numOfParams = ChartsSelection.GetNumOfSerieses();
			byte[] data = _buildRequestMessages.BuildMessage1(
				numOfParams,
				TriggerSelection.TriggerData.RecordGap,
				TriggerSelection.IsContinuous,
				TriggerSelection.TriggerData.TriggerPosition);
			//_mcu_Communicator.SendMessage(false, 0xAB, data, null);
			Send(data);
			System.Threading.Thread.Sleep(100);

			List<SelectedParameterData> paramsList = ChartsSelection.GetParamsList();
			foreach (SelectedParameterData param in paramsList)
			{
				data = _buildRequestMessages.BuildMessage2(param);
				if (data == null)
					return;

				//_mcu_Communicator.SendMessage(false, 0xAB, data, null);
				Send(data);
				System.Threading.Thread.Sleep(100);
			}

			data = _buildRequestMessages.BuildMessage3(
				TriggerSelection.TriggerData.TriggerKeyword,
				TriggerSelection.TriggerData.TriggerType);
			if (data == null)
				return;
			//_mcu_Communicator.SendMessage(false, 0xAB, data, null);
			Send(data);
			System.Threading.Thread.Sleep(100);

			data = _buildRequestMessages.BuildMessage4(
				TriggerSelection.TriggerData.TriggerValue);
			if (data == null)
				return;
			//_mcu_Communicator.SendMessage(false, 0xAB, data, null);
			Send(data);
			System.Threading.Thread.Sleep(100);

			//if (_dataList == null)
				InitDataList();

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

			_interval = TriggerSelection.TriggerData.Interval;


			if (NumOfSamples <= 0)
				NumOfSamples = 128;
			double timerInterval = NumOfSamples * _interval; // In secs
			timerInterval *= 1000; // In MS

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					DataPercentage = 0;
				});
			}






			_timerRecordTime.Interval = timerInterval / 100;
			_timerRecordTime.Start();
		}

		private void InitDataList()
		{

			_chartIndex = 0;
			_seriesIndex = 0;

			_dataList = new List<DataFromMCU_Cahrt>();
			foreach (ChartViewModel chart in Scope.ChartsList)
			{
				DataFromMCU_Cahrt chartData = new DataFromMCU_Cahrt() 
					{ SeriesDataList = new List<DataFromMCU_Series>() };

				foreach (string series in chart.SeriesesList)
				{
					DataFromMCU_Series seriesData = new DataFromMCU_Series()
					{ DataList = new List<double>() };
					chartData.SeriesDataList.Add(seriesData);
				}

				_dataList.Add(chartData);
			}
		}

		private void MessageReceivedEventHandler(byte[] buffer)
		{
			byte[] id = new byte[3];
			_paramPhasesFrequency.GetMessageID(ref id);

			if (id[0] == buffer[0] && id[1] == buffer[1] && id[2] == buffer[2])
			{
				byte[] valueBuff = new byte[4];
				Array.Copy(buffer, 4, valueBuff, 0, 4);
				Array.Reverse(valueBuff);
				uint phasesFrequency = BitConverter.ToUInt32(valueBuff, 0);

				TriggerSelection.SetPhasesFrequency(phasesFrequency);
			}
		}

		private void CanMessageReceivedEventHandler(uint node, byte[] buffer)
		{
			AsyncMessageReceivedEventHandler(buffer);
		}

		private void AsyncMessageReceivedEventHandler(byte[] buffer)
		{
			
			if (!_isTriggerReceived)
			{
				HeaderMessageReceived(buffer);
				return;
			}

			bool isFooterReceived = IsFooterMessageReceived(buffer);
			if (isFooterReceived) 
			{
				EndReceivingData();
				return;
			}

			_timerRecordTime.Stop();

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					DataPercentage = 100;
				});
			}

			LoggerService.Inforamtion(this, "Setting chart, value no.: " + _chartPointsCounter);

			_chartPointsCounter++;
			int val = BitConverter.ToInt16(buffer, 0);
			SetSingleValue(val);

			_chartPointsCounter++;
			val = BitConverter.ToInt16(buffer, 2);
			SetSingleValue(val);

			_chartPointsCounter++;
			val = BitConverter.ToInt16(buffer, 4);
			SetSingleValue(val);

			_chartPointsCounter++;
			val = BitConverter.ToInt16(buffer, 6);
			SetSingleValue(val);

		}

		private void EndReceivingData()
		{
			//_timerRecordTime.Stop();
			
			//if (Application.Current != null)
			//{
			//	Application.Current.Dispatcher.Invoke(() =>
			//	{
			//		DataPercentage = 100;
			//	});
			//}


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

			if(TriggerSelection.IsContinuous) 
				InitDataList();

			_isTriggerReceived = false;
		}

		private void HeaderMessageReceived(byte[] buffer)
		{
			Task.Run(() =>
			{
				ulong header = BitConverter.ToUInt64(buffer, 0);
				if (header == 0xABCDABCDABCDABCD) // TODO buffer size
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

		private bool IsFooterMessageReceived(byte[] buffer)
		{			
			ulong header = BitConverter.ToUInt64(buffer, 0);
			return (header == 0xDCBADCBADCBADCBA);
		}


		private void SetSingleValue(int val)
		{
			try
			{
				if (_dataList == null)
					return;

				if (_chartIndex < 0 || _chartIndex >= _dataList.Count)
					return;

				DataFromMCU_Cahrt chartDataList = _dataList[_chartIndex];
				while (chartDataList.SeriesDataList.Count == 0)
				{
					HandleIndeces(chartDataList);
					chartDataList = _dataList[_chartIndex];
				}

				List<double> seriesDataList = null;
				seriesDataList = chartDataList.SeriesDataList[_seriesIndex].DataList;
				seriesDataList.Add(val);

				

				LoggerService.Inforamtion(this, "Setting series data, value no.: " + _chartPointsCounter + " Time: " + _chartTimeMS);

				HandleIndeces(chartDataList);
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set chart value", "Recording Error", ex);
			}
		}

		private void HandleIndeces(DataFromMCU_Cahrt chartDataList)
		{
			_seriesIndex++;
			if (_seriesIndex >= chartDataList.SeriesDataList.Count)
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
						DataPercentage++;

					if (DataPercentage >= _percentageBeforTrigger)
						DataPercentage = _percentageBeforTrigger;
				}
				else
				{
					if (DataPercentage < 100)
						DataPercentage++;

					if (DataPercentage >= 100)
						DataPercentage = 100;
				}
			});

			
		}

		private void Send(byte[] data)
		{
			if(_canService != null)
				_canService.Send(data);
			else
			{
				ComPort._canbusControl.GetCanDriver().CanService.Send(
					data, 
					ComPort._canbusControl.mailboxId, 
					false);
			}
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
