namespace Skyline.Protocol.PollingManager.CustomCode.PollEntrys
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class InterfaceInformation : PollableBase
	{
		public InterfaceInformation(SLProtocol protocol, string description) : base(protocol, description)
		{
		}

		public InterfaceInformation(SLProtocol protocol, string description, int triggerID) : base(protocol, description, triggerID)
		{
		}

		protected override void Poll()
		{
			// Add Custom poll logic here if needed
			Protocol.Log($"Polling '{Name}'.");
			int dummyRows = 10;
			List<InterfacesQActionRow> tableRows = new List<InterfacesQActionRow>();
			Random random = new Random();

			for (int i = 0; i < dummyRows; i++)
			{
				var row = new InterfacesQActionRow
				{
					Interfacesinstance_201 = $"{i}",
					Interfacesdescription_202 = $"Ethernet{i}",
					Interfacesstatus_203 = random.Next(0, 2),
				};

				tableRows.Add(row);
			}

			Protocol.FillArray(Parameter.Interfaces.tablePid, tableRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
		}

		protected override void PrePollConfiguration()
		{
			// Do some pre-poll configuration if needed
		}
	}
}