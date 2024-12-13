namespace Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Exceptions;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	public class ResponseBasicDataSet : ResponseHandler
	{
		public ResponseBasicDataSet(string rowName) : base(rowName)
		{
		}

		protected override void ProcessResponse(SLProtocol protocol)
		{
			Dictionary<int, object> setParameters = new Dictionary<int, object>
				{
					{ Parameter.systemname_20, "Dummy Name" },
					{ Parameter.serialNumber_21, "DUMMY23430E7W" },
				};
			protocol.SetParameters(setParameters.Keys.ToArray(), setParameters.Values.ToArray());

			int dummyRows = 10;
			List<DummydataQActionRow> tableRows = new List<DummydataQActionRow>();
			Random random = new Random();

			for (int i = 0; i < dummyRows; i++)
			{
				var row = new DummydataQActionRow
				{
					Dummydatainstance_101 = $"{i}",
					Dummydatadescription_102 = $"Dummy Entry {i}",
					Dummydatastatus_103 = random.Next(0, 2),
				};

				tableRows.Add(row);
			}

			protocol.FillArray(100, tableRows.ToArray().Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
		}
	}
}
