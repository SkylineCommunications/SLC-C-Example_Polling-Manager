namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using Skyline.DataMiner.Scripting;

	public class ResponseBasicFailDataSet : IPollingManagerResponseHandler
	{
		public ResponseBasicFailDataSet()
		{
		}

		bool IPollingManagerResponseHandler.ProcessResponse(SLProtocol protocol)
		{
			return false;
		}
	}
}
