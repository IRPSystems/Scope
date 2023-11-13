using Communication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Services;
using DeviceHandler.Enums;
using DeviceHandler.Models;
using DeviceHandler.ViewModels;
using DeviceSimulators.ViewModels;
using DeviceSimulators.Views;
using Entities.Models;
using MCUScope.ViewModels;
using RunScope.Services;
using Services.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;

namespace RunScope.ViewModels
{
	public class MainViewModel: ObservableObject
	{
		#region Properties and Fields

		public string Version { get; set; }

		public bool IsShowSimulatorButton { get; set; }

		public MCUScopeViewModel MCUScope { get; set; }

		public CanConnectViewModel CanConnect { get; set; }

		public bool IsScopeEnabled { get; set; }

		public Brush CommunicationStateColor { get; set; }
		public Brush CommunicationStateTextColor { get; set; }


		private MCU_Communicator _mcu_Communicator;
		private CheckCommunicationService _checkCommunication;

		#endregion Properties and Fields

		#region Constructor

		public MainViewModel() 
		{
			LoggerService.Init("RunScope.log", Serilog.Events.LogEventLevel.Information);
			LoggerService.Inforamtion(this, "-------------------------------------- Run Scope ---------------------");

			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			MCUSimulatorCommand = new RelayCommand(MCUSimulator);

			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			CanConnect = new CanConnectViewModel(500000, 171, 12223, 12220);
			CanConnect.ConnectEvent += Connect;
			CanConnect.DisconnectEvent += Disconnect;

			_mcu_Communicator = new MCU_Communicator();

			MCUScope = new MCUScopeViewModel(_mcu_Communicator);


			IsScopeEnabled = false;

			_checkCommunication = new CheckCommunicationService("MCU");
			_checkCommunication.CommunicationStateReprotEvent += CommunicationStateReprotEventHandler;

			IsShowSimulatorButton = false;
#if DEBUG
			IsShowSimulatorButton = true;
#endif //  Debug
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
			_mcu_Communicator.Dispose();

			_checkCommunication.Dispose();

			MCUScope.Close();
			MCUScope.Dispose();

		}


		private void Connect()
		{
			_mcu_Communicator.Init(
				CanConnect.SelectedAdapter,
				CanConnect.SelectedBaudrate,
				CanConnect.NodeID,
				CanPCanService.GetHWId(CanConnect.SelectedHwId),
				0xAB,
				0xAA,
				CanConnect.RxPort,
				CanConnect.TxPort,
				CanConnect.Address);

			ObservableCollection<DeviceBase> deviceList = new ObservableCollection<DeviceBase>();
			ReadDevicesFileService readDevicesFileService = new ReadDevicesFileService();
			readDevicesFileService.ReadFromMCUJson(
				"param_defaults.json",
				deviceList,
				"MCU",
				Entities.Enums.DeviceTypesEnum.MCU);
			_mcu_Communicator.InitMessageDict(deviceList[0] as DeviceData);

			CanConnect.IsConnectButtonEnabled = !_mcu_Communicator.IsInitialized;
			CanConnect.IsDisconnectButtonEnabled = _mcu_Communicator.IsInitialized;

			IsScopeEnabled = CanConnect.IsDisconnectButtonEnabled;
		}

		private void Disconnect()
		{
			_mcu_Communicator.Dispose();


			CanConnect.IsConnectButtonEnabled = !_mcu_Communicator.IsInitialized;
			CanConnect.IsDisconnectButtonEnabled = _mcu_Communicator.IsInitialized;

			IsScopeEnabled = CanConnect.IsDisconnectButtonEnabled;
		}

		private void CommunicationStateReprotEventHandler(CommunicationStateEnum state, string errMessage)
		{
			CommunicationStateTextColor = Brushes.White;
			if (state == CommunicationStateEnum.Connected)
				CommunicationStateColor = Brushes.Green;
			else
				CommunicationStateColor = Brushes.Red;
		}


		private void MCUSimulator()
		{
			DevicesContainer devicesContainter = new DevicesContainer();
			DeviceFullData deviceFullData = new DeviceFullData(MCUScope.MCUDevice);

			devicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			devicesContainter.DevicesList = new ObservableCollection<Entities.Models.DeviceData>();
			devicesContainter.TypeToDevicesFullData = new Dictionary<Entities.Enums.DeviceTypesEnum, DeviceFullData>();

			devicesContainter.DevicesFullDataList.Add(deviceFullData);
			devicesContainter.DevicesList.Add(deviceFullData.Device);
			devicesContainter.TypeToDevicesFullData.Add(deviceFullData.Device.DeviceType, deviceFullData);

			DeviceSimulatorsViewModel dsvm = new DeviceSimulatorsViewModel(devicesContainter);
			DeviceSimulatorsView dsv = new DeviceSimulatorsView() { DataContext = dsvm };
			dsv.Show();
		}

#endregion Methods

#region Commands

		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand MCUSimulatorCommand { get; private set; }

#endregion Commands
	}
}
