
using Communication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceHandler.Enums;
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

		private CanService _canService;
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
			CanService canService)
		{
			_canService = canService;

			_buffer = new byte[8];
			using (var md5 = MD5.Create())
			{
				Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes("")), 0, _buffer, 0, 3);
			}
						

			NotifyStatus(CommunicationStateEnum.None, null);




			_canService.CanMessageReceivedEvent += CanMessageReceivedEventHandler;

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
			
			_canService.Send(_buffer, 0xAB, false);
			
			_noResponseCounter++;
		}

		private void NotifyStatus(CommunicationStateEnum status, string errDescription)
		{
			CommunicationStateReprotEvent?.Invoke(status, errDescription);
		}

		private void CanMessageReceivedEventHandler(uint id, byte[] buffer)
		{
			_noResponseCounter = 0;
			NotifyStatus(CommunicationStateEnum.Connected, null);
		}

		#endregion Methods

		#region Events

		public event Action<CommunicationStateEnum, string> CommunicationStateReprotEvent;

		#endregion Events
	}
}
