namespace Skyline.Protocol.PollingManager.CustomCode.Configuration
{
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class BasicPoll : PollableBase
	{
		public BasicPoll(SLProtocol protocol, string description, int actionID) : base(protocol, description)
		{
			ActionId = actionID;
		}

		protected override void PollConfiguration()
		{
			Protocol.Log($"Polling '{Name}'.");
		}
	}
}