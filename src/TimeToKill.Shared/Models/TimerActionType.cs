using System.ComponentModel;

namespace TimeToKill.Models;

public enum TimerActionType
{
	[Description("Kill")]
	Kill,
	[Description("Force Kill")]
	KillForce,
	[Description("Suspend")]
	Suspend,
	[Description("Demote Priority")]
	DemotePriority
}

public static class TimerActionTypeExtensions
{
	public static string GetDescription(this TimerActionType actionType)
	{
		var field = actionType.GetType().GetField(actionType.ToString());
		var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
		return attr?.Description ?? actionType.ToString();
	}
}
