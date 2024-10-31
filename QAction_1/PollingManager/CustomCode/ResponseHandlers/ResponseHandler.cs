namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public abstract class ResponseHandler
	{
		public ResponseHandler(string rowName)
		{
			RowName = rowName;
		}

		public string RowName { get; }

		public abstract void ProcessResponse(SLProtocol protocol);
	}

	//public class ResponseHandler
	//{
	//    private IPollingManagerResponseHandler handler;

	//    public string RowName { get; }

	//    public ResponseHandler(IPollingManagerResponseHandler handler, string rowName)
	//    {
	//        this.handler = handler;
	//        RowName = rowName;
	//    }

	//    public bool ProcessResponse(SLProtocol protocol)
	//    {
	//        try
	//        {
	//            if (handler.ProcessResponse(protocol))
	//            {
	//                return true;
	//                // UpdatePollingState(protocol, PollStatus.Succeeded);
	//            }
	//            else
	//            {
	//                return false;
	//                // UpdatePollingState(protocol, PollStatus.Failed);
	//            }
	//        }
	//        catch (System.Exception)
	//        {
	//            protocol.Log($"QA{protocol.QActionID}|ProcessResponse|Failed to process the response for poll entry {RowName}.", LogType.Error, LogLevel.NoLogging);
	//            return false;
	//            //UpdatePollingState(protocol, PollStatus.Failed);
	//        }
	//    }

	//    private void UpdatePollingState(SLProtocol protocol, PollStatus pollStatus)
	//    {
	//        protocol.SetParameterIndexByKey(Parameter.Pollingmanager.tablePid, RowName, Parameter.Pollingmanager.Idx.pollingmanager_lastpollstatus_1009 + 1, (int)pollStatus);
	//    }
	//}
}
