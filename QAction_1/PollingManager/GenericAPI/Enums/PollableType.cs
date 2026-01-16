namespace Skyline.Protocol.PollingManager.GenericAPI.Enums
{
	/// <summary>
	/// Represents the type of a pollable.
	/// </summary>
	public enum PollableType
	{
		/// <summary>
		/// The pollable triggers and action with actionId.
		/// </summary>
		InitTrigger,

		/// <summary>
		/// The pollalbe process the poll fully in Code.
		/// </summary>
		ProcessInCode,
	}
}