using Communication.Services;
using CommunityToolkit.Mvvm.Input;
using Controls.ViewModels;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using Entities.Enums;
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
using TrueDriveCommunication;

namespace MCUScope.ViewModels
{
	public class MCUScopeViewModel : DocingBaseViewModel
	{
		public enum ExecuteStateEnum
		{
			None,
			WaitForTrigger,
			Triggered,
			End
		}

		#region Properties

		public TriggerSelectionViewModel TriggerSelection { get; set; }
		public ChartsSelectionViewModel ChartsSelection { get; set; }
		public ScopeViewModel Scope { get; set; }




		#region ExecuteState

		public static readonly DependencyProperty ExecuteStateProperty =
		 DependencyProperty.Register("ExecuteState", typeof(ExecuteStateEnum), typeof(MCUScopeViewModel));

		public ExecuteStateEnum ExecuteState
		{
			get { return (ExecuteStateEnum)GetValue(ExecuteStateProperty); }
			set { SetValue(ExecuteStateProperty, value); }
		}

		#endregion ExecuteState

		#region IsContinuous

		public static readonly DependencyProperty IsContinuousProperty =
		 DependencyProperty.Register("IsContinuous", typeof(bool), typeof(MCUScopeViewModel));

		public bool IsContinuous
		{
			get { return (bool)GetValue(IsContinuousProperty); }
			set { SetValue(IsContinuousProperty, value); }
		}

		#endregion IsContinuous

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
		
		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private CanService _canService;

		private MCU_ParamData _paramPhasesFrequency;

		private List<SelectedParameterData> _recordParamsList;
		private List<List<List<double>>> _dataList;
		private int _paramIndex = 0;

		#endregion Fields

		#region Constroctur

		public MCUScopeViewModel(CanService canService, string jsonPath) :
			base("MCUScopeLayout", "MCUScope")
		{

			Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
				"MjQ2MzU2NkAzMjMwMmUzMzJlMzBOaGhMVVJBelp0Y1c1eXdoNHRTcHI4bGVOdmdxQWNXZkZxeklweENobmdjPQ==");

			ExecuteState = ExecuteStateEnum.None;
			IsContinuous = false;

			if (canService != null)
			{
				_canService = canService;
				_canService.CanMessageReceivedEvent += CanMessageReceivedEventHandler;
				_canService.MessageReceivedEvent += MessageReceivedEventHandler; ;
			}
			else
			{
				ComPort._canbusControl.GetCanDriver().CanService.CanMessageReceivedEvent += CanMessageReceivedEventHandler;
				ComPort._canbusControl.GetCanDriver().CanService.MessageReceivedEvent += MessageReceivedEventHandler;
			}

			_chartIndex = 0;
			_seriesIndex = 0;
			_chartTimeMS = 0;
			_chartPointsCounter = 0;

			SaveSetupCommand = new RelayCommand(SaveSetup);
			LoadSetuptCommand = new RelayCommand(LoadSetup);
			ExecuteCommand = new RelayCommand(Execute);
			ForceTrigCommand = new RelayCommand(ForceTrig);
			ContinuousCommand = new RelayCommand(Continuous);

			DockFill = true;

			MCUDevice = ReadFromMCUJson(jsonPath);

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


			
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;


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

			ReadJson.JsonHelper.OnLoadedParameterFile += JsonHelper_OnLoadedParameterFile;
		}

		#endregion Constroctur

		#region Methods

		private void JsonHelper_OnLoadedParameterFile(object sender, EventArgs e)
		{
			MCUDevice = ReadFromMCUJson(ReadJson.JsonPath);

			TriggerSelection.McuDevice = MCUDevice;
			ChartsSelection.SetMcuDevice(MCUDevice);
		}

		public new void Dispose()
		{
			base.Dispose();

			Scope.Dispose();

			_cancellationTokenSource.Cancel();
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
			Scope.AddChart(chartName, "ms", ChartViewModel.XAxisTypes.Double);
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
			Send(data);
		}

		
		private void Execute()
		{
			

			int numOfParams = ChartsSelection.GetNumOfSerieses();
			byte[] data = _buildRequestMessages.BuildMessage1(
				numOfParams,
				TriggerSelection.TriggerData.RecordGap,
				IsContinuous,
				TriggerSelection.TriggerData.TriggerPosition);
			//_mcu_Communicator.SendMessage(false, 0xAB, data, null);
			Send(data);
			System.Threading.Thread.Sleep(100);

			_recordParamsList = ChartsSelection.GetParamsList();
			foreach (SelectedParameterData param in _recordParamsList)
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


			InitWaitForTrigger();


			_interval = TriggerSelection.TriggerData.Interval / 1000;
			ComPort._canbusControl.GetCanDriver().CanService.CanMessageReceivedEvent += CanMessageReceivedEventHandler;


		}

		private void InitWaitForTrigger()
		{
			_isTriggerReceived = false;
			_chartIndex = 0;
			_seriesIndex = 0;
			_chartTimeMS = 0;
			_chartPointsCounter = 0;

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

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					ExecuteState = ExecuteStateEnum.WaitForTrigger;
				});
			}
		}

		private void MessageReceivedEventHandler(byte[] buffer)
		{
			if (_paramPhasesFrequency == null)
				return;

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
			if (node != 0xAA)
				return;

			

		
			
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

			LoggerService.Inforamtion(this, "Setting chart, value no.: " + _chartPointsCounter);

			
			SetSingleValue(buffer, 0);
			SetSingleValue(buffer, 2);
			SetSingleValue(buffer, 4);
			SetSingleValue(buffer, 6);

		}

		private void EndReceivingData()
		{
			


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

						ExecuteState = ExecuteStateEnum.End;
					});
				}

				System.Threading.Thread.Sleep(1);
			}

			System.Threading.Thread.Sleep(1000);

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					if (IsContinuous)
						InitWaitForTrigger();
					else
						ComPort._canbusControl.GetCanDriver().CanService.CanMessageReceivedEvent -= CanMessageReceivedEventHandler;
				});
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

							ExecuteState = ExecuteStateEnum.Triggered;
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


		private void SetSingleValue(byte[] buffer, int index)
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


				_chartPointsCounter++;
				int val = BitConverter.ToInt16(buffer, 0);
				val = (val << _recordParamsList[_paramIndex++].Scale);
				if (_paramIndex >= _recordParamsList.Count)
					_paramIndex = 0;

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

		private void Continuous()
		{
			ComPort._canbusControl.GetCanDriver().CanService.CanMessageReceivedEvent -= CanMessageReceivedEventHandler;
		}

		#endregion Methods

		#region Commands

		public RelayCommand SaveSetupCommand { get; private set; }
		public RelayCommand LoadSetuptCommand { get; private set; }
		public RelayCommand ExecuteCommand { get; private set; }
		public RelayCommand ForceTrigCommand { get; private set; }

		public RelayCommand ContinuousCommand { get; private set; }

		#endregion Commands

	}
}
