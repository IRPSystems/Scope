
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace MCUScope.Models
{
	public class SeriesData: ObservableObject
	{
		public string Name { get; set; }
		public Brush Color { get; set; }
	}
}
