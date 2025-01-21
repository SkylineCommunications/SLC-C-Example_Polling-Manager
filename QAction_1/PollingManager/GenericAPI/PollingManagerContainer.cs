namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Concurrent;
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// <see cref="PollingManager"/> container class used to provide singleton on the element level.
	/// </summary>
	public static class PollingManagerContainer
	{
		private static readonly ConcurrentDictionary<string, PollingManager> Managers = new ConcurrentDictionary<string, PollingManager>();

		/// <summary>
		/// Creates instance of <see cref="PollingManager"/> and adds it to <see cref="PollingManagerContainer"/>.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="configuration">Configuration object.</param>
		/// <returns>
		/// Newly created instance of <see cref="PollingManager"/>, if it doesn't exist, or existing instance of <see cref="PollingManager"/> with updated <see cref="PollableBase.Protocol"/>.
		/// </returns>
		/// <exception cref="ArgumentException">Throws if creation of <see cref="PollingManager"/> fails.</exception>
		public static PollingManager AddManager(SLProtocol protocol, PollingManagerConfigurationBase configuration)
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
		/// Gets the <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="initTrigger">Id of the trigger that initializes <see cref="PollingManager"/>.</param>
		/// <returns><see cref="PollingManager"/> instance with updated <see cref="PollableBase.Protocol"/>.</returns>
		/// <exception cref="InvalidOperationException">Throws if <see cref="PollingManager"/> for this element is not initialized.</exception>
		public static PollingManager GetManager(SLProtocol protocol, int initTrigger)
		{
			string key = GetKey(protocol);

			if (!Managers.ContainsKey(key))
			{
				var table = new PollingmanagerQActionTable(protocol, Parameter.Pollingmanager.tablePid, "Polling Manager");

				if (table.RowCount == 0)
				{
					throw new InvalidOperationException($"Polling manager for element [{key}] is not initialized, please call AddManager first.");
				}

				//protocol.CheckTrigger(initTrigger);
			}

			Managers[key].Protocol = protocol;

			return Managers[key];
		}

		/// <summary>
		/// Removes the <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns>True if the element is successfully found and removed, false otherwise.</returns>
		public static bool RemoveInstance(SLProtocol protocol)
		{
			string key = GetKey(protocol);

			return Managers.TryRemove(key, out _);
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
	}
}