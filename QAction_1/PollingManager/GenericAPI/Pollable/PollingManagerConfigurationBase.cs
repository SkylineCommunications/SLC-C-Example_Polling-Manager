namespace Skyline.DataMiner.PollingManager
{
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	/// <summary>
	/// Base class for polling configurations.
	/// </summary>
	public abstract class PollingManagerConfigurationBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PollingManagerConfigurationBase"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		public PollingManagerConfigurationBase(SLProtocol protocol) => Protocol = protocol;

		public List<PollableBase> ListRows
		{
			get
			{
				// Update Value.Name to row.Key before returning the list
				Rows.ForEach(row => row.Value.Name = row.Key);
				return Rows.Select(row => row.Value).ToList();
			}
		}

		public SLProtocol Protocol { get; set; }

		public abstract Dictionary<int, ResponseHandler> ResponseHandlers { get; set; }

		protected abstract List<Dependency> Dependencies { get; set; }

		protected abstract Dictionary<string, PollableBase> Rows { get; set; }


		/// <summary>
		/// Creates the polling configuration.
		/// </summary>
		public void Create()
		{
			CreateRelations();
			CreateDependencies();
			CreateResponseHandlers();
			CreateParameterRelations();
		}

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by <see cref="Create"/>.
		/// </summary>
		protected abstract void CreateDependencies();

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by <see cref="Create"/>.
		/// </summary>
		protected abstract void CreateRelations();

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by <see cref="Create"/>.
		/// </summary>
		protected abstract void CreateResponseHandlers();

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by <see cref="Create"/>.
		/// </summary>
		protected abstract void CreateParameterRelations();
	}
}
