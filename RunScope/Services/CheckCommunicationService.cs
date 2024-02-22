
using Communication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using Entities.Models;
using Services.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace RunScope.Services
{
	public class CheckCommunicationService: ObservableObject, IDisposable
	{
		#region Fields

		

		private System.Timers.Timer _timerSendMessage;

		public string Name;

		private MCU_Communicator _mcu_Communicator;
		private byte[] _buffer;

		#endregion Fields

		#region Constructor

		public CheckCommunicationService(
			string name) 
		{
			Name = name;





			_timerSendMessage = new System.Timers.Timer(1000);
			_timerSendMessage.Elapsed += SendMessageElapsedEventHandler;

		}

		#endregion Constructor

		#region Methods

		public void Init(
			MCU_Communicator mcu_Communicator)
		{
			_mcu_Communicator = mcu_Communicator;

			_buffer = new byte[8];
			using (var md5 = MD5.Create())
			{
				Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes("")), 0, _buffer, 0, 3);
			}
						

			NotifyStatus(CommunicationStateEnum.None, null);

			_timerSendMessage.Start();


		}

		public void Dispose()
		{
			_timerSendMessage.Stop();
			NotifyStatus(CommunicationStateEnum.Disconnected, null);

		}

		private int _noResponseCounter = 0;
		private void SendMessageElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			if(_noResponseCounter > 3)
			{
				NotifyStatus(CommunicationStateEnum.Disconnected, null);
				_noResponseCounter = 0;
			}

			_mcu_Communicator.GetParamValue(
				new MCU_ParamData() { Cmd = "" },
				Callback);
			
			_noResponseCounter++;
		}

		private void Callback(DeviceParameterData param, CommunicatorResultEnum result, string errorDescription)
		{
			_noResponseCounter = 0;
			NotifyStatus(CommunicationStateEnum.Connected, null);
		}

		private void NotifyStatus(CommunicationStateEnum status, string errDescription)
		{
			CommunicationStateReprotEvent?.Invoke(status, errDescription);
		}

		#endregion Methods

		#region Events

		public event Action<CommunicationStateEnum, string> CommunicationStateReprotEvent;

		#endregion Events
	}
}
