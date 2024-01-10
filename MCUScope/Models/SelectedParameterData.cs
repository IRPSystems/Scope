using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Models;

namespace MCUScope.Models
{
	public class SelectedParameterData: ObservableObject
	{
		public DeviceParameterData Parameter { get; set; }
		public byte Scale { get; set; }
	}
}
