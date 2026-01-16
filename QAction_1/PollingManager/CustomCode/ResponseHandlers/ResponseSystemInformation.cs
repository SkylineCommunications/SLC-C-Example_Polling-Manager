namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.CustomCode.Configuration;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	public class ResponseSystemInformation : ResponseHandler
	{
		public ResponseSystemInformation(PollEntrys entryName) : base(entryName)
		{
		}

		protected override void ProcessResponse(SLProtocol protocol)
		{
			Thread.Sleep(3000); // Simulate some polling delay

			Dictionary<int, object> setParameters = new Dictionary<int, object>
				{
					{ Parameter.systemname_20, "Dummy Name" },
					{ Parameter.serialnumber_21, "DUMMY23430E7W" },
				};
			protocol.SetParameters(setParameters.Keys.ToArray(), setParameters.Values.ToArray());
		}
	}
}