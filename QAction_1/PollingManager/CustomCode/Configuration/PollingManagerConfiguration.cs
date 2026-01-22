namespace Skyline.Protocol.PollingManager.CustomCode.Configuration
{
	using System.Collections.Generic;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.CustomCode.ClearParameters;
	using Skyline.Protocol.PollingManager.CustomCode.PollEntrys;
	using Skyline.Protocol.PollingManager.CustomCode.ResponseHandlers;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	/// <summary>
	/// Configuration class for the Polling Manager. Defines poll entries, dependencies,
	/// clear parameter relations, response handlers, and row relations.
	/// </summary>
	public class PollingManagerConfiguration : PollingManagerConfigurationBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PollingManagerConfiguration"/> class.
		/// Creates all poll rows and prepares dependency and response handler collections.
		/// </summary>
		/// <param name="protocol">The SLProtocol instance.</param>
		public PollingManagerConfiguration(SLProtocol protocol) : base(protocol)
		{
			Dependencies = new List<Dependency>();
			ResponseHandlers = new Dictionary<int, ResponseHandler>();
			Rows = CreateRows(protocol);
		}

		/// <summary>
		/// Gets the dictionary containing all configured response handlers.
		/// </summary>
		public override Dictionary<int, ResponseHandler> ResponseHandlers { get; }

		/// <summary>
		/// Gets the list of dependency definitions used across poll rows.
		/// </summary>
		protected override List<Dependency> Dependencies { get; }

		/// <summary>
		/// Gets the dictionary containing all poll rows mapped to their corresponding poll entries.
		/// </summary>
		protected override Dictionary<PollEntrys, PollableBase> Rows { get; }

		/// <summary>
		/// Registers clear-parameter behavior for specific poll rows.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown when the API Version row is missing.</exception>
		protected override void CreateClearParameterRelations()
		{
			var system = GetRequiredRow(PollEntrys.System, "System row is required for clear-parameter rules.");
			var vlan = GetRequiredRow(PollEntrys.VLAN, "VLAN row is required for clear-parameter rules.");
			var temperature = GetRequiredRow(PollEntrys.Temperature, "Temperature row is required for clear-parameter rules.");
			var pvst = GetRequiredRow(PollEntrys.PVST, "PVST row is required for clear-parameter rules.");
			var cpu = GetRequiredRow(PollEntrys.CPU, "CPU row is required for clear-parameter rules.");
			var interfacesInfo = GetRequiredRow(PollEntrys.Interfaces, "Interfaces row is required for clear-parameter rules.");

			ClearParametersConfiguration.SystemInfo.ApplyToRow(system);
			ClearParametersConfiguration.VlanInfo.ApplyToRow(vlan);
			ClearParametersConfiguration.TemperatureInfo.ApplyToRow(temperature);
			ClearParametersConfiguration.PvstInfo.ApplyToRow(pvst);
			ClearParametersConfiguration.CpuInfo.ApplyToRow(cpu);
			ClearParametersConfiguration.InterfacesInfo.ApplyToRow(interfacesInfo);
		}

		/// <summary>
		/// Creates and attaches dependency rules for specific poll rows.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown when a required poll row is missing.</exception>
		protected override void CreateDependencies()
		{
			var apiDependency = new Dependency("Version2", true, "Only supported in API version 2.0");

			// Retrieve the required system row; throws if missing
			var systemRow = GetRequiredRow(PollEntrys.System, "System row is missing in Rows dictionary for dependency creation.");

			// Attach the dependency
			systemRow.AddDependency(Parameter.apiversion_5, apiDependency);
		}

		/// <summary>
		/// Defines logical parent-child relationships between poll rows.
		/// </summary>
		/// <exception cref="KeyNotFoundException">Thrown when a required row is missing.</exception>
		protected override void CreateRelations()
		{
			var vlan = GetRequiredRow(PollEntrys.VLAN, "VLAN row is required for relations.");
			var interfacesInfo = GetRequiredRow(PollEntrys.Interfaces, "Interfaces row is required for relations.");
			var pvst = GetRequiredRow(PollEntrys.PVST, "PVST row is required for relations.");

			// Multiple children can be added to a parent pollable
			vlan.AddChildren(pvst);
			interfacesInfo.AddChildren(pvst);
		}

		/// <summary>
		/// Registers response handlers to process incoming data for each pollable parameter.
		/// </summary>
		protected override void CreateResponseHandlers()
		{
			ResponseHandlers.Add(Parameter.processsysteminformation_61001, new ResponseSystemInformation(PollEntrys.System));
			ResponseHandlers.Add(Parameter.processtemperatureinformation_61002, new ResponseTemperature(PollEntrys.Temperature));
		}

		/// <summary>
		/// Creates and initializes all poll rows used by the polling manager.
		/// Each <see cref="PollEntrys"/> enum value is mapped to a corresponding <see cref="PollableBase"/> instance.
		/// </summary>
		/// <param name="protocol">The <see cref="SLProtocol"/> instance used to create poll objects.</param>
		/// <returns>
		/// A <see cref="Dictionary{TKey, TValue}"/> where the key is a <see cref="PollEntrys"/> value
		/// and the value is the corresponding <see cref="PollableBase"/> poll object.
		/// </returns>
		private static Dictionary<PollEntrys, PollableBase> CreateRows(SLProtocol protocol)
		{
			return new Dictionary<PollEntrys, PollableBase>
			{
				{ PollEntrys.APIVersion, new GenericPoll(protocol, "[Mandatory] API Information") { Mandatory = true } },
				{ PollEntrys.System, new GenericPoll(protocol, "[Dependency] System Information (API 2.0)", 60_001) },
				{ PollEntrys.VLAN, new VlanInformation(protocol, "[Parent] VLAN Information") },
				{ PollEntrys.Temperature, new GenericPoll(protocol, "[Fail] Temperature Information", 60_002) },
				{ PollEntrys.CPU, new CpuInformationPoll(protocol, "[Basic] CPU Information") },
				{ PollEntrys.Interfaces, new InterfaceInformation(protocol, "[Parent] Interface Information") },
				{ PollEntrys.PVST, new PvstVlanInformation(protocol, "[Child] VLAN - Interfaces - PVST+") },
			};
		}

		/// <summary>
		/// Retrieves a poll row from the <see cref="Rows"/> dictionary by its <see cref="PollEntrys"/> key.
		/// Throws a <see cref="KeyNotFoundException"/> if the requested row is not found.
		/// </summary>
		/// <param name="entry">The <see cref="PollEntrys"/> enum value representing the poll row to retrieve.</param>
		/// <param name="errorMessage">The error message to include in the exception if the row is missing.</param>
		/// <returns>The <see cref="PollableBase"/> instance associated with the specified <paramref name="entry"/>.</returns>
		/// <exception cref="KeyNotFoundException">
		/// Thrown when the specified <paramref name="entry"/> does not exist in the <see cref="Rows"/> dictionary.
		/// </exception>
		private PollableBase GetRequiredRow(PollEntrys entry, string errorMessage)
		{
			if (!Rows.TryGetValue(entry, out PollableBase row))
			{
				throw new KeyNotFoundException(errorMessage);
			}

			return row;
		}
	}
}