namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using System;
	using Skyline.DataMiner.Scripting;

	public class ResponseBasicDataSet : ResponseHandler
	{
		public ResponseBasicDataSet(string rowName) : base(rowName)
		{
		}

		public override void ProcessResponse(SLProtocol protocol)
		{
			return;
		}
	}
}
