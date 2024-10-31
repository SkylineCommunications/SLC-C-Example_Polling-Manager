using Skyline.DataMiner.Scripting;

namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	public interface IPollingManagerResponseHandler
	{
		void ProcessResponse(SLProtocol protocol);
	}
}
