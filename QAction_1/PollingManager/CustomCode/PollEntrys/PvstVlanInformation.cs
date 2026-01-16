namespace Skyline.Protocol.PollingManager.CustomCode.PollEntrys
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class PvstVlanInformation : PollableBase
	{
		public PvstVlanInformation(SLProtocol protocol, string description) : base(protocol, description)
		{
		}

		public PvstVlanInformation(SLProtocol protocol, string description, int triggerID) : base(protocol, description, triggerID)
		{
		}

		protected override void Poll()
		{
			// Add Custom poll logic here if needed
			Protocol.Log($"Polling '{Name}'.");
			int dummyRows = 5;
			List<VlanpvsttableQActionRow> tableRows = new List<VlanpvsttableQActionRow>();
			Random random = new Random();

			for (int i = 0; i < dummyRows; i++)
			{
				var row = new VlanpvsttableQActionRow
				{
					Vlanpvstid_401 = $"{i + 1000}",
					Vlanpvststatus = random.Next(0, 2),
				};

				tableRows.Add(row);
			}

			Protocol.FillArray(Parameter.Vlanpvsttable.tablePid, tableRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
		}

		protected override void PrePollConfiguration()
		{
			// Do some pre-poll configuration if needed
		}
	}
}