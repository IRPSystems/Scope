
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace MCUScope.Models
{
	public class ScopeSetupData: ObservableObject
	{
		public TriggerSelectionData TriggerData { get; set; }
		public List<ChartData> ChartsList { get; set; }
	}
}
