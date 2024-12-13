namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Exceptions;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	public class ResponseBasicFailDataSet : ResponseHandler
	{
		public ResponseBasicFailDataSet(string rowName) : base(rowName)
		{
		}

		protected override void ProcessResponse(SLProtocol protocol)
		{
			throw new PollingException("Incorrect requirements.");
		}
	}
}
