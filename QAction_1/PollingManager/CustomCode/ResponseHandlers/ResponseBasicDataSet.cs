namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using Skyline.DataMiner.Scripting;

	public class ResponseBasicDataSet : IPollingManagerResponseHandler
	{
		public ResponseBasicDataSet()
		{
		}

		bool IPollingManagerResponseHandler.ProcessResponse(SLProtocol protocol)
		{
			return true;
		}
	}
}
