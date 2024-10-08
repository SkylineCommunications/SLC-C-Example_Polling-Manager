namespace Skyline.Protocol.PollingManager.GenericAPI.PollEntrys
{
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	internal class BasicPoll : PollableBase
	{
		private int triggerId;

		public BasicPoll(SLProtocol protocol, string name, int triggerId) : base(protocol, name)
		{
			this.triggerId = triggerId;
		}

		public override bool InitiatePoll()
		{
			Protocol.Log($"Polling '{Name}'.");
			int result = Protocol.CheckTrigger(triggerId);
			return result == 0 ? true : false;
		}
	}
}