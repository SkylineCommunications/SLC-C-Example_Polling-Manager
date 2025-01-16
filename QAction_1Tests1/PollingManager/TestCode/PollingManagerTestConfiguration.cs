namespace Skyline.DataMiner.PollingManager.Tests.Configuration
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	public class PollingManagerTestConfiguration : PollingManagerConfigurationBase
	{
		public PollingManagerTestConfiguration(SLProtocol protocol) : base(protocol)
		{
			Rows = new Dictionary<string, PollableBase>()
			{
			};

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
			throw new ArgumentException("Exception has occured.");
		}

		protected override void CreateParameterRelations()
		{
		}

		protected override void CreateRelations()
		{
		}

		protected override void CreateResponseHandlers()
		{
		}
	}
}
