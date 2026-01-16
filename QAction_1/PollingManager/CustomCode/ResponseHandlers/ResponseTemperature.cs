namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.CustomCode.Configuration;
	using Skyline.Protocol.PollingManager.GenericAPI.Exceptions;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	public class ResponseTemperature : ResponseHandler
	{
		public ResponseTemperature(PollEntrys entryName) : base(entryName)
		{
		}

		protected override void ProcessResponse(SLProtocol protocol)
		{
			throw new PollingException("Failed to parse data.");
		}
	}
}