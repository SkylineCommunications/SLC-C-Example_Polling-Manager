namespace Skyline.DataMiner.PollingManager
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI;

	public abstract class PollingManagerConfigurationBase
	{
		public PollingManagerConfigurationBase(SLProtocol protocol) => Protocol = protocol;

		public List<PollableBase> ListRows => Rows.Select(row => row.Value).ToList();

		public SLProtocol Protocol { get; set; }

		public abstract Dictionary<int, ResponseHandler> ResponseHandlers { get; set; }

		protected abstract List<Dependency> Dependencies { get; set; }

		protected abstract Dictionary<string, PollableBase> Rows { get; set; }

		public void Create()
		{
			CreateRelations();
			CreateDependencies();
			CreateResponseHandlers();
		}

		protected abstract void CreateDependencies();

		protected abstract void CreateRelations();

		protected abstract void CreateResponseHandlers();
	}
}
