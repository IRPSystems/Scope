
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace MCUScope.Models
{
	public class ChartData: ObservableObject
	{
		public string Name { get; set; }
		public List<SeriesData> SeriesesList { get; set; }
	}
}
