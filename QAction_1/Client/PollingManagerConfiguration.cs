namespace Skyline.PollingManager.Client
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Scripting;

    using Skyline.PollingManager.Enums;
    using Skyline.PollingManager.Pollable;
    using Skyline.PollingManager.Structs;

    public class PollingManagerConfiguration : PollingManagerConfigurationBase
	{
		public PollingManagerConfiguration(SLProtocol protocol) : base(protocol)
		{
			Rows = new Dictionary<string, PollableBase>()
			{
				// Parent of CEO, CFO, CTO
				// Child of -
				{ "Owner", new PollableA(Protocol, "Owner - A") },

				// Parent of CFO, CTO, Expert Hub Lead
				// Child of Owner
				{ "CEO", new PollableA(Protocol, "CEO - A") },

				// Parent of -
				// Child of Owner, CEO
				{ "CFO", new PollableB(Protocol, "CFO - B", 15) },

				// Parent of Expert Hub Lead
				// Child of Owner, CEO
				{ "CTO", new PollableB(Protocol, "CTO - B", 20) },

				// Parent of Principal 1, Principal 2, Senior 1
				// Child of CEO, CTO
				{ "Expert Hub Lead", new PollableB(Protocol, "Expert Hub Lead - B") },

				// Parent of Senior 2, Senior 3
				// Child of CTO, Expert Hub Lead
				{ "Principal 1", new PollableB(Protocol, "Principal 1 - B") },

				// Parent of Senior 1
				// Child of CTO, Expert Hub Lead
				{ "Principal 2", new PollableC(Protocol, "Principal 2 - C", 30, 60, PeriodType.Default) },

				// Parent of -
				// Child of Expert Hub Lead, Principal 2
				{ "Senior 1", new PollableA(Protocol, "Senior 1 - A") },

				// Parent of -
				// Child of Principal 1
				{ "Senior 2", new PollableB(Protocol, "Senior 2 - B") },

				// Parent of -
				// Child of Principal 1
				{ "Senior 3", new PollableC(Protocol, "Senior 3 - C") },
			};

			Dependencies = new List<Dependency>()
			{
				new Dependency(1, true, "Must Be On is not on!"),
				new Dependency(3, false, "Must Not Be Vacation is on vacation!"),
				new Dependency("Working", true, "Must Equal Working is not working!"),
			};
		}

		protected override Dictionary<string, PollableBase> Rows { get; set; }

		protected override List<Dependency> Dependencies { get; set; }

		protected override void CreateDependencies()
		{
			Rows["Owner"].AddDependency(10, Dependencies[0]);
			Rows["Owner"].AddDependency(20, Dependencies[1]);
			Rows["Owner"].AddDependency(30, Dependencies[2]);
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
		}
	}
}
