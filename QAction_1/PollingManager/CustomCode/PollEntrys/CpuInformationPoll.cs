namespace Skyline.Protocol.PollingManager.CustomCode.PollEntrys
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	public class CpuInformationPoll : PollableBase
	{
		public CpuInformationPoll(SLProtocol protocol, string description) : base(protocol, description)
		{
		}

		public CpuInformationPoll(SLProtocol protocol, string description, int triggerID) : base(protocol, description, triggerID)
		{
		}

		protected override void Poll()
		{
			// Add Custom poll logic here if needed
			Protocol.Log($"Polling '{Name}'.");
			int dummyRows = 4;
			List<CpusQActionRow> tableRows = new List<CpusQActionRow>();
			Random random = new Random();

			for (int i = 0; i < dummyRows; i++)
			{
				var row = new CpusQActionRow
				{
					Cpusinstance_101 = $"{i}",
					Cpusdescription_102 = $"Dummy Entry {i}",
					Cpustotal_103 = random.Next(60, 71),
				};

				tableRows.Add(row);
			}

			Protocol.FillArray(Parameter.Cpus.tablePid, tableRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
		}

		protected override void PrePollConfiguration()
		{
			// Do some pre-poll configuration if needed
		}
	}
}