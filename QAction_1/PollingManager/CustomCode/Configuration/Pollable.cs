namespace Skyline.Protocol.PollingManager.CustomCode.Configuration
{
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class Pollable : PollableBase
	{
		public Pollable(SLProtocol protocol, string description) : base(protocol, description)
		{
		}

		protected override void PollConfiguration()
		{
			Protocol.Log($"Polling '{Name}'.");
		}
	}
}
