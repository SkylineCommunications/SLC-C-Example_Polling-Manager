namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using Skyline.DataMiner.Scripting;

	public class ResponseBasicFailDataSet : ResponseHandler
	{
		public ResponseBasicFailDataSet(string rowName) : base(rowName)
		{
		}

		public override void ProcessResponse(SLProtocol protocol)
		{
			throw new PollingException("Incorrect requirements.");
		}
	}
}
