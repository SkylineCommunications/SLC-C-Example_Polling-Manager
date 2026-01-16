namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Concurrent;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.CustomCode.Configuration;

	/// <summary>
	/// <see cref="PollingManager"/> container class used to provide singleton on the element level.
	/// </summary>
	public static class PollingManagerContainer
	{
		private static readonly ConcurrentDictionary<string, PollingManager> Managers = new ConcurrentDictionary<string, PollingManager>();

		/// <summary>
		/// Gets the <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns><see cref="PollingManager"/> instance with updated <see cref="PollableBase.Protocol"/>.</returns>
		public static PollingManager GetManager(SLProtocol protocol)
		{
			if (!TryGetManager(protocol, out PollingManager manager))
			{
				return AddManager(protocol, new PollingManagerConfiguration(protocol));
			}

			manager.Protocol = protocol;
			return manager;
		}

		/// <summary>
		/// Initiates the <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns><see cref="PollingManager"/> instance with updated <see cref="PollableBase.Protocol"/>.</returns>
		public static PollingManager InitiateManagerAfterStartup(SLProtocol protocol)
		{
			if (TryGetManager(protocol, out _))
			{
				protocol.Log($"Polling manager for element already exists. Reinitializing manager", LogType.Information, LogLevel.NoLogging);
				TryRemoveInstance(protocol);
			}

			return AddManager(protocol, new PollingManagerConfiguration(protocol));
		}

		/// <summary>
		/// Removes the <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns>True if the element is successfully found and removed, false otherwise.</returns>
		public static bool TryRemoveInstance(SLProtocol protocol)
		{
			var instanceKey = GetKey(protocol);
			return Managers.TryRemove(instanceKey, out _);
		}

		/// <summary>
		/// Creates instance of <see cref="PollingManager"/> and adds it to <see cref="PollingManagerContainer"/>.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="configuration">Configuration object.</param>
		/// <returns>
		/// Newly created instance of <see cref="PollingManager"/>, if it doesn't exist, or existing instance of <see cref="PollingManager"/> with updated <see cref="PollableBase.Protocol"/>.
		/// </returns>
		/// <exception cref="ArgumentException">Throws if creation of <see cref="PollingManager"/> fails.</exception>
		private static PollingManager AddManager(SLProtocol protocol, PollingManagerConfigurationBase configuration)
		{
			string key = GetKey(protocol);

			if (!Managers.ContainsKey(key))
			{
				PollingManager manager;
				try
				{
					configuration.Create();
					manager = new PollingManager(protocol, Parameter.Pollingmanager.tablePid, configuration);
				}
				catch (ArgumentException ex)
				{
					protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|PollingManagerContainer.AddManager|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
					throw new ArgumentException("Failed to create PollingManager.");
				}

				Managers.TryAdd(key, manager);
			}

			Managers[key].Protocol = protocol;

			return Managers[key];
		}

		/// <summary>
		/// Creates unique key based on DataMinerID and ElementID.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns>Key in format DataMinerID/ElementID.</returns>
		private static string GetKey(SLProtocol protocol)
		{
			return string.Join("/", protocol.DataMinerID, protocol.ElementID);
		}

		/// <summary>
		/// Retrieves the <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="manager"><see cref="PollingManager"/> instance.</param>
		/// <returns>True if the <see cref="PollingManager"/> instance is successfully found, false otherwise.</returns>
		private static bool TryGetManager(SLProtocol protocol, out PollingManager manager)
		{
			var instanceKey = GetKey(protocol);
			return Managers.TryGetValue(instanceKey, out manager);
		}
	}
}