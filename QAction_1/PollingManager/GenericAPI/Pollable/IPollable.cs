namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents base for a row in the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public interface IPollable
	{
		SLProtocol Protocol { get; set; }

		int ID { get; set; }

		string Name { get; set; }

		string Description { get; set; }

		double Interval { get; set; }

		double DefaultInterval { get; set; }

		DateTime LastPoll { get; set; }

		PollStatus PollStatus { get; set; }

		string PollInfo { get; set; }

		AdminState AdminStatus { get; set; }

		List<IPollable> Parents { get; set; }

		List<IPollable> Children { get; set; }

		Dictionary<int, Dependency> Dependencies { get; set; }

		bool InitiatePoll();

		bool CheckDependencies();

		void AddDependency(int paramId, Dependency dependency);

		void AddParent(IPollable parent);

		void AddParents(params IPollable[] parents);

		void AddChild(IPollable child);

		void AddChildren(params IPollable[] children);
	}
}
