namespace Skyline.DataMiner.PollingManager
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Configuration for Polling Manager table.
	/// </summary>
	public abstract class PollingManagerConfigurationBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PollingManagerConfigurationBase"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		public PollingManagerConfigurationBase(SLProtocol protocol)
		{
			Protocol = protocol;
			Dependencies = new List<Dependency>();
		}

		public SLProtocol Protocol { get; set; }

		public List<PollableBase> ListRows => Rows.Select(row => row.Value).ToList();

		/// <summary>
		/// Gets or sets collection of rows from the Polling Manager table.
		/// </summary>
		protected abstract Dictionary<string, PollableBase> Rows { get; set; }

		/// <summary>
		/// Gets or sets collection of dependencies between rows and other parameters in the protocol.
		/// </summary>
		protected List<Dependency> Dependencies { get; set; }

		/// <summary>
		/// Initializes relations and dependencies of the Polling Manager table that are defined in <see cref="PollingManagerConfigurationBase.CreateRelations"/> and <see cref="PollingManagerConfigurationBase.CreateDependencies"/>.
		/// </summary>
		public void Create()
		{
			CreateRelations();
			CreateDependencies();
		}

		/// <summary>
		/// Used to create relations between rows in Polling Manager table.
		/// This method gets called by <see cref="PollingManagerConfigurationBase.Create"/> and shouldn't be called directly.
		/// </summary>
		protected virtual void CreateRelations()
		{
		}

		/// <summary>
		/// Used to create dependencies between rows in Polling Manager table and other parameters in the protocol.
		/// This method gets called by <see cref="PollingManagerConfigurationBase.Create"/> and shouldn't be called directly.
		/// </summary>
		protected virtual void CreateDependencies()
		{
		}
	}
}
