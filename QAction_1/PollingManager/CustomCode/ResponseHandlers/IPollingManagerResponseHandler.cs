using Skyline.DataMiner.Scripting;

namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	public interface IPollingManagerResponseHandler
	{
		bool ProcessResponse(SLProtocol protocol);
	}
}
