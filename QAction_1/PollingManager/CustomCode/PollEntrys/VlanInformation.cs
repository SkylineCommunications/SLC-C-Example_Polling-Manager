namespace Skyline.Protocol.PollingManager.CustomCode.PollEntrys
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class VlanInformation : PollableBase
	{
		public VlanInformation(SLProtocol protocol, string description) : base(protocol, description)
		{
		}

		public VlanInformation(SLProtocol protocol, string description, int triggerID) : base(protocol, description, triggerID)
		{
		}

		protected override void Poll()
		{
			// Add Custom poll logic here if needed
			Protocol.Log($"Polling '{Name}'.");
			int dummyRows = 5;
			List<VlantableQActionRow> tableRows = new List<VlantableQActionRow>();

			for (int i = 0; i < dummyRows; i++)
			{
				var row = new VlantableQActionRow
				{
					Vlanid_301 = $"{i + 1000}",
					Vlanname_302 = $"VLAN{i + 1000}",
				};

				tableRows.Add(row);
			}

			Protocol.FillArray(Parameter.Vlantable.tablePid, tableRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
		}

		protected override void PrePollConfiguration()
		{
			// Do some pre-poll configuration if needed
		}
	}
}