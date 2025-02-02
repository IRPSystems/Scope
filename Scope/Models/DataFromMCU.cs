using System.Collections.Generic;

namespace Scope.Models
{
	public class DataFromMCU_Cahrt
	{
		public List<DataFromMCU_Series> SeriesDataList;
	}

	public class DataFromMCU_Series
	{
		public List<double> DataList { get; set; }
	}
}
