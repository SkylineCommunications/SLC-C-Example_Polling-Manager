namespace Skyline.Protocol.PollingManager.GenericAPI.Enums
{
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents columns of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public enum Column
	{
		Name = 0,
		ID= 1,
		Description = 2,
		Interval = 3,
		SuggestedInterval = 4,
		AdminStatus = 5,
		Poll = 6,
		LastPoll = 7,
		PollStatus = 8,
		PollInfo = 9,
		FullDescription = 10,
	}
}
