
using System.ComponentModel;

namespace MCUScope.Enums
{
	public enum TriggerPositionTypesEnum
	{
		[Description("Show only data before trigger happened")]
		ShowOnlyDataBeforeTriggerHappened,

		[Description("Show data 50% before and 50% after trigger")]
		ShowData50BeforeAnd50AfterTrigger,

		[Description("Show data 25% before and 75% after trigger")]
		ShowData25BeforeAnd75AfterTrigger,

		[Description("Show data 12.5% before and 87.5% after trigger")]
		ShowData12_5BeforeAnd87_5AfterTrigger,

		[Description("Show data 6.25% before and 93.75% after trigger")]
		ShowData6_25BeforeAnd93_75AfterTrigger,

	}
}
