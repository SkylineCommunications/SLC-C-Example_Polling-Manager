namespace Skyline.Protocol.PollingManager.CustomCode.Configuration
{
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class GenericPoll : PollableBase
	{
		public GenericPoll(SLProtocol protocol, string description) : base(protocol, description)
		{
		}

		public GenericPoll(SLProtocol protocol, string description, int triggerID) : base(protocol, description, triggerID)
		{
		}

		protected override void Poll()
		{
			// Add Custom poll logic here if needed
			Protocol.Log($"Polling '{Name}'.");
		}

		protected override void PrePollConfiguration()
		{
			// Do some pre-poll configuration if needed
		}
	}
}