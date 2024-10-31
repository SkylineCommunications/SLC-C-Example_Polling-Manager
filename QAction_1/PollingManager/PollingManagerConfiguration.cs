namespace Skyline.Protocol.PollingManager
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers;
	using Skyline.Protocol.PollingManager.GenericAPI.PollEntrys;

	public class PollingManagerConfiguration : PollingManagerConfigurationBase
	{
		public PollingManagerConfiguration(SLProtocol protocol) : base(protocol)
		{
			Rows = new Dictionary<string, PollableBase>()
			{
				{ "Basic", new BasicPoll(Protocol, "Basic Dataset", 60_001) },
				{ "Fail", new BasicPoll(Protocol, "Failing Dataset", 60_002) },

				// Parent of CEO, CFO, CTO
				// Child of -
				{ "Owner", new Pollable(Protocol, "Owner - A") },

				// Parent of CFO, CTO, Expert Hub Lead
				// Child of Owner
				{ "CEO", new Pollable(Protocol, "CEO - A") },

				// Parent of -
				// Child of Owner, CEO
				{ "CFO", new Pollable(Protocol, "CFO - B") },

				// Parent of Expert Hub Lead
				// Child of Owner, CEO
				{ "CTO", new Pollable(Protocol, "CTO - B") },

				// Parent of Principal 1, Principal 2, Senior 1
				// Child of CEO, CTO
				{ "Expert Hub Lead", new Pollable(Protocol, "Expert Hub Lead - B") },

				// Parent of Senior 2, Senior 3
				// Child of CTO, Expert Hub Lead
				{ "Principal 1", new Pollable(Protocol, "Principal 1 - B") },

				// Parent of Senior 1
				// Child of CTO, Expert Hub Lead
				{ "Principal 2", new Pollable(Protocol, "Principal 2 - C") },

				// Parent of -
				// Child of Expert Hub Lead, Principal 2
				{ "Senior 1", new Pollable(Protocol, "Senior 1 - A") },

				// Parent of -
				// Child of Principal 1
				{ "Senior 2", new Pollable(Protocol, "Senior 2 - B") },

				// Parent of -
				// Child of Principal 1
				{ "Senior 3", new Pollable(Protocol, "Senior 3 - C") },

				// Parent of VLANs, Counters, Alarms
				// Child of -
				{ "Interfaces", new Pollable(Protocol, "Interfaces") },

				// Parent of Port Overview, Static
				// Child of Interfaces
				{ "VLANs", new Pollable(Protocol, "VLANs - Interfaces") },

				// Parent of -
				// Child of VLANs
				{ "Port Overview", new Pollable(Protocol, "Port Overview - VLANs") },

				// Parent of -
				// Child of VLANs
				{ "Static", new Pollable(Protocol, "Static - VLANs") },

				// Parent of -
				// Child of Interfaces
				{ "Counters", new Pollable(Protocol, "Counters - Interfaces") },

				// Parent of CPU, Processes, Alarms
				// Child of -
				{ "System", new Pollable(Protocol, "System") },

				// Parent of -
				// Child of System
				{ "CPU", new Pollable(Protocol, "CPU - System") },

				// Parent of -
				// Child of System
				{ "Processes", new Pollable(Protocol, "Processes - System") },

				// Parent of -
				// Child of System, Interfaces
				{ "Alarms", new Pollable(Protocol, "Alarms - System - Interfaces") },
			};

			Rows["Alarms"].DefaultInterval = 55;
			Rows["Basic"].DefaultInterval = 16;
			Rows["Basic"].Interval = 11;

			Dependencies = new List<Dependency>()
			{
			};

			ResponseHandlers = new Dictionary<int, ResponseHandler>()
			{ };
		}

		public override Dictionary<int, ResponseHandler> ResponseHandlers { get; set; }

		protected override List<Dependency> Dependencies { get; set; }

		protected override Dictionary<string, PollableBase> Rows { get; set; }

		protected override void CreateDependencies()
		{
		}

		protected override void CreateRelations()
		{
			Rows["Owner"].AddChildren(Rows["CEO"], Rows["CFO"], Rows["CTO"]);

			Rows["CEO"].AddChildren(Rows["CFO"], Rows["CTO"], Rows["Expert Hub Lead"]);

			Rows["CTO"].AddChildren(Rows["Expert Hub Lead"]);

			Rows["Principal 1"].AddParents(Rows["CTO"], Rows["Expert Hub Lead"]);
			Rows["Principal 1"].AddChildren(Rows["Senior 2"], Rows["Senior 3"]);

			Rows["Principal 2"].AddParents(Rows["CTO"], Rows["Expert Hub Lead"]);
			Rows["Principal 2"].AddChildren(Rows["Senior 1"]);

			Rows["Senior 1"].AddParents(Rows["Expert Hub Lead"]);

			Rows["Interfaces"].AddChildren(Rows["VLANs"], Rows["Counters"], Rows["Alarms"]);
			Rows["VLANs"].AddChildren(Rows["Port Overview"], Rows["Static"]);
			Rows["System"].AddChildren(Rows["CPU"], Rows["Processes"], Rows["Alarms"]);
		}

		protected override void CreateResponseHandlers()
		{
			ResponseHandlers.Add(61001, new ResponseBasicDataSet(Rows["Basic"].Name));
			ResponseHandlers.Add(61002, new ResponseBasicFailDataSet(Rows["Fail"].Name));
		}
	}
}
