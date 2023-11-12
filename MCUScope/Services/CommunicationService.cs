
using Communication.Services;
using TrueDriveCommunication;

namespace MCUScope.Services
{
	public class CommunicationService
	{
		public CanService CanService { get; set; }
		public CanbusControl CanbusControl 
		{ 
			get
			{
				return ComPort.CurrentControl as CanbusControl;
			}
		}


		public void Send(byte[] data) 
		{ 
			if(CanService != null) 
			{ 
				CanService.Send(data);
			}
			else if (ComPort.CurrentControl != null)
			{
				CanbusControl.SendMessage(data, 0xAB);
			}
		}
	}
}
