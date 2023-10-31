using Communication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Enums;
using DeviceHandler.ViewModels;
using DeviceSimulators.ViewModels;
using MCUScope.ViewModels;
using RunScope.Services;
using RunScope.Views;
using Services.Services;
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


		private MCUSimulatorView _mcuSimulator;

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

			MCUScope = new MCUScopeViewModel();

			_mcuSimulator = new MCUSimulatorView()
			{
				DataContext = new MCUSimulatorMainWindowViewModel(MCUScope.MCUDevice),
			};

			IsScopeEnabled = false;

			_checkCommunication = new CheckCommunicationService("MCU");

			IsShowSimulatorButton = false;
#if DEBUG
			IsShowSimulatorButton = true;
#endif //  Debug
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
			if(MCUScope.CanService != null) 
				MCUScope.Dispose();

			if(_mcuSimulator.DataContext is MCUSimulatorMainWindowViewModel simulator)
				simulator.Dispose();

			_mcuSimulator.Close();

			_checkCommunication.Dispose();

			MCUScope.Close();
		}


		private void Connect()
		{
			if (CanConnect.SelectedAdapter == "PCAN")
			{
				MCUScope.CanService = new CanPCanService(CanConnect.SelectedBaudrate, CanConnect.NodeID, CanConnect.GetSelectedHWId(CanConnect.SelectedHwId));
			}
			else if (CanConnect.SelectedAdapter == "UDP Simulator")
			{
				MCUScope.CanService = new CanUdpSimulationService(CanConnect.SelectedBaudrate, CanConnect.NodeID, CanConnect.RxPort, CanConnect.TxPort, CanConnect.Address);
			}

			MCUScope.CanService.Init(true);

			_checkCommunication.Init(MCUScope.CanService);
			_checkCommunication.CommunicationStateReprotEvent += CommunicationStateReprotEventHandler;

			CanConnect.IsConnectButtonEnabled = !MCUScope.CanService.IsInitialized;
			CanConnect.IsDisconnectButtonEnabled = MCUScope.CanService.IsInitialized;

			IsScopeEnabled = CanConnect.IsDisconnectButtonEnabled;
		}

		private void Disconnect()
		{
			if (MCUScope.CanService != null)
			{
				MCUScope.CanService.Dispose();
			}



			CanConnect.IsConnectButtonEnabled = !MCUScope.CanService.IsInitialized;
			CanConnect.IsDisconnectButtonEnabled = MCUScope.CanService.IsInitialized;

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
			_mcuSimulator.Show();
		}

#endregion Methods

#region Commands

		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand MCUSimulatorCommand { get; private set; }

#endregion Commands
	}
}
