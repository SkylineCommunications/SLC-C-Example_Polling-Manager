namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Enums;

	/// <summary>
	/// Represents the base for a row in the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public interface IPollable
	{
		/// <summary>
		/// Gets or sets the protocol associated with the pollable item.
		/// </summary>
		SLProtocol Protocol { get; set; }

		/// <summary>
		/// Gets or sets the trigger PID ID.
		/// </summary>
		int TriggerId { get; set; }

		/// <summary>
		/// Gets or sets the SNMP State PID ID.
		/// </summary>
		int? StateSnmpPid { get; set; }

		/// <summary>
		/// Gets or sets the unique identifier for the pollable item.
		/// </summary>
		int ID { get; set; }

		/// <summary>
		/// Gets or sets the name of the pollable item.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the description of the pollable item.
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Gets or sets the polling interval in seconds.
		/// </summary>
		double Interval { get; set; }

		/// <summary>
		/// Gets or sets the suggested polling interval in seconds.
		/// </summary>
		double SuggestedInterval { get; set; }

		/// <summary>
		/// Gets or sets the timestamp of the last poll.
		/// </summary>
		DateTime LastPolled { get; set; }

		/// <summary>
		/// Gets or sets the timestamp of the last poll execution.
		/// </summary>
		DateTime LastPollExecuted { get; set; }

		/// <summary>
		/// Gets or sets the current polling status.
		/// </summary>
		PollStatus PollStatus { get; set; }

		/// <summary>
		/// Gets or sets additional polling information.
		/// </summary>
		string PollInfo { get; set; }

		/// <summary>
		/// Gets or sets the administrative state of the pollable item.
		/// </summary>
		AdminState AdminStatus { get; set; }

		/// <summary>
		/// Gets the parent pollable items.
		/// </summary>
		List<IPollable> Parents { get; }

		/// <summary>
		/// Gets the child pollable items.
		/// </summary>
		List<IPollable> Children { get; }

		/// <summary>
		/// Gets the dependencies for the pollable item.
		/// </summary>
		Dictionary<int, Dependency> Dependencies { get; }

		/// <summary>
		/// Gets or sets a value indicating whether polling is mandatory.
		/// </summary>
		bool Mandatory { get; set; }

		/// <summary>
		/// This method gets called by <see cref="PollingManager"/>.
		/// </summary>
		/// <returns cref="PollableType">Will return <see cref="PollableType.InitTrigger"/> when a actionId is specified. Otherwise will return <see cref="PollableType.ProcessInCode"/>.</returns>
		PollableType InitiatePoll();

		/// <summary>
		/// Gets dependent parameters and compares their values with dependencies. Sets <see cref="PollInfo"/> to first condition not satisfied.
		/// </summary>
		/// <returns>False if any condition is not satisfied, otherwise true.</returns>
		bool CheckDependencies();

		/// <summary>
		/// Clears all linked parameters when the polling entry <see cref="AdminStatus"/> is set to Disabled.
		/// </summary>
		void ClearParameters();

		/// <summary>
		/// Link parameter Id's to poll entry, this is then used to clear the parameters when polling is disabled.
		/// </summary>
		/// <param name="singleParameters">A dictionary of single parameters with their IDs and values.</param>
		/// <param name="tableParameters">A list of table parameter IDs.</param>
		void AddParameters(IReadOnlyDictionary<int, object> singleParameters, IReadOnlyList<int> tableParameters);

		/// <summary>
		/// Adds a dependency to the pollable item.
		/// </summary>
		/// <param name="paramId">The parameter ID of the dependency.</param>
		/// <param name="dependency">The dependency details.</param>
		void AddDependency(int paramId, Dependency dependency);

		/// <summary>
		/// Adds parent without creating two way relation between elements. This shouldn't be used directly. Use <see cref="AddParents"/> instead.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <exception cref="InvalidOperationException">Throws if <paramref name="parent"/> is already this elements child.</exception>
		void AddParent(IPollable parent);

		/// <summary>
		/// Adds parents to this element, and adds this element as a child of each parent passed as parameter.
		/// </summary>
		/// <param name="parents">Parent elements.</param>
		/// <exception cref="InvalidOperationException">Throws if any <paramref name="parents"/> element is already this elements child.</exception>
		void AddParents(params IPollable[] parents);

		/// <summary>
		/// Adds child without creating two way relation between elements. This shouldn't be used directly. Use <see cref="AddChildren"/> instead.
		/// </summary>
		/// <param name="child">Child element.</param>
		/// <exception cref="InvalidOperationException">Throws if <paramref name="child"/> is already this elements parent.</exception>
		void AddChild(IPollable child);

		/// <summary>
		/// Adds children to this element, and adds this element as a parent of each child passed as parameter.
		/// </summary>
		/// <param name="children">Child elements.</param>
		/// <exception cref="InvalidOperationException">Throws if any <paramref name="children"/> element is already this elements parent.</exception>
		void AddChildren(params IPollable[] children);
	}
}