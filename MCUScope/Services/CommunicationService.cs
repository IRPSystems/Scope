
using Communication.Services;
using TrueDriveCommunication;

namespace MCUScope.Services
{
	public class CommunicationService
	{
		public CanService CanService { get; set; }
		public CanbusControl CanbusControl { get; set; }


		public void Send(byte[] data) 
		{ 
			if(CanService != null) 
			{ 
				CanService.Send(data);
			}
			else if (CanbusControl != null)
			{
				// TODO: Smadar
				CanbusControl.SendMessage(data, 0xAB);
			}
		}
	}
}
