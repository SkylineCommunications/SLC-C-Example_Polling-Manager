namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI;
	using SLNetMessages = Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// <see cref="PollingManager"/> container class used to provide singleton on the element level.
	/// </summary>
	public static class PollingManagerContainer
	{
		private readonly static ConcurrentDictionary<string, PollingManager> managers = new ConcurrentDictionary<string, PollingManager>();

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

			if (!managers.ContainsKey(key))
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

				managers.TryAdd(key, manager);
			}

			managers[key].Protocol = protocol;

			return managers[key];
		}

		/// <summary>
		/// Removes the <see cref="PollingManager"/> instance for the element.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <returns>True if the element is successfully found and removed, false otherwise.</returns>
		public static bool RemoveInstance(SLProtocol protocol)
		{
			string key = GetKey(protocol);

			return managers.TryRemove(key, out _);
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

			if (!managers.ContainsKey(key))
			{
				var table = new PollingmanagerQActionTable(protocol, Parameter.Pollingmanager.tablePid, "Polling Manager");

				if (table.RowCount == 0)
				{
					throw new InvalidOperationException($"Polling manager for element [{key}] is not initialized, please call AddManager first.");
				}

				protocol.CheckTrigger(initTrigger);
			}

			managers[key].Protocol = protocol;

			return managers[key];
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

	/// <summary>
	/// Handler for <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public class PollingManager
	{
		private readonly int tablePid;
		private readonly Dictionary<int, ResponseHandler> responseHandlers;
		private readonly Dictionary<string, PollableBase> rows = new Dictionary<string, PollableBase>();


		/// <summary>
		/// Initializes a new instance of the <see cref="PollingManager"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="table">Polling manager table instance.</param>
		/// <param name="rows">Rows to add to the <paramref name="table"/>.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="rows"/> contains duplicate names.</exception>
		/// <exception cref="ArgumentException">Throws if <paramref name="rows"/> contains null values.</exception>
		//public PollingManager(SLProtocol protocol, int tablePid, List<PollableBase> rows)
		//{
		//	Protocol = protocol;
		//	this.tablePid = tablePid;

		//	HashSet<string> names = new HashSet<string>();

		//	for (int i = 0; i < rows.Count; i++)
		//	{
		//		if (!names.Add(rows[i].Name))
		//		{
		//			throw new ArgumentException($"Duplicate name: {rows[i].Name}.");
		//		}

		//		rows[i].ID = i + 1;
		//		this.rows.Add(rows[i].Name, rows[i] ?? throw new ArgumentException("Rows parameter can't contain null values."));
		//	}

		//	if (protocol.RowCount(tablePid) != 0)
		//	{
		//		LoadRows();
		//	}

		//	FillTable(this.rows);
		//}

		/// <summary>
		/// Initializes a new instance of the <see cref="PollingManager"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="table">Polling manager table instance.</param>
		/// <param name="configuration.ListRows">Rows to add to the <paramref name="table"/>.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="configuration.ListRows"/> contains duplicate names.</exception>
		/// <exception cref="ArgumentException">Throws if <paramref name="configuration.ListRows"/> contains null values.</exception>
		public PollingManager(SLProtocol protocol, int tablePid, PollingManagerConfigurationBase configuration)
		{
			Protocol = protocol;
			this.tablePid = tablePid;
			responseHandlers = configuration.ResponseHandlers;
			var rows = configuration.ListRows;

			HashSet<string> names = new HashSet<string>();

			for (int i = 0; i < rows.Count; i++)
			{
				if (!names.Add(rows[i].Name))
				{
					throw new ArgumentException($"Duplicate name: {rows[i].Name}.");
				}

				rows[i].ID = i + 1;
				this.rows.Add(rows[i].Name, rows[i] ?? throw new ArgumentException("Rows parameter can't contain null values."));
			}

			if (protocol.RowCount(tablePid) != 0)
			{
				LoadRows();
			}

			FillTable(this.rows);
		}

		public SLProtocol Protocol { get; set; }

		/// <summary>
		/// Checks <see cref="PollingmanagerQActionTable"/> for rows that are ready to be polled and polls them.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Throws if <see cref="PollableBase.IntervalType"/> is not <see cref="IntervalType.Default"/> or <see cref="IntervalType.Custom"/>.
		/// </exception>
		public void CheckForUpdate()
		{
			bool requiresUpdate = false;

			foreach (KeyValuePair<string, PollableBase> row in rows)
			{
				PollableBase currentRow = row.Value;

				if (currentRow.AdminStatus == AdminState.Disabled)
				{
					continue;
				}

				if (CheckLastPollTime(currentRow.Interval, currentRow.LastPoll))
				{
					PollRow(currentRow);
				}
			}
		}

		/// <summary>
		/// Handles sets on the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="rowKey">Row key.</param>
		/// <param name="column">Column on which set was performed.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="rowKey"/> doesn't exist in the table.</exception>
		/// <exception cref="ArgumentException">
		/// Throws if <paramref name="column"/> is not <see cref="Column.Interval"/>, <see cref="Column.AdminStatus"/> or <see cref="Column.Poll"/>.
		/// </exception>
		public void HandleRowUpdate(string rowKey, Column column, object value)
		{
			if (!rows.ContainsKey(rowKey))
			{
				throw new ArgumentException($"Row key '{rowKey}' doesn't exist in the Polling Manager table.");
			}

			PollableBase tableRow;

			switch (column)
			{
				case Column.Description:
					tableRow = LoadRow(rowKey);
					CheckAndSetUniqueDescription(tableRow, Convert.ToString(value));
					break;
				case Column.Interval:
					tableRow = LoadRow(rowKey);
					tableRow.Interval = Convert.ToInt32(value);
					break;

				case Column.AdminStatus:
					tableRow = LoadRow(rowKey);
					AdminState state = (AdminState)Convert.ToInt32(value);
					UpdateState(tableRow, state);
					break;

				case Column.Poll:
					PollRow(rows[rowKey]);
					break;

				default:
					throw new ArgumentException($"Unsupported Column '{column}'.");
			}

			FillTableNoDelete(rows);
		}

		private void CheckAndSetUniqueDescription(PollableBase row, string value)
		{
			var descriptions = (object[])((object[])Protocol.NotifyProtocol((int)SLNetMessages.NotifyType.NT_GET_TABLE_COLUMNS, Parameter.Pollingmanager.tablePid, new uint[] { Parameter.Pollingmanager.Idx.pollingmanager_description }))[0];
			var fullDescriptions = descriptions.Cast<string>().ToList();
			if (!fullDescriptions.Contains(value))
			{
				row.Description = value;
			}
			else
			{
				string message = $"Unable to set the description to:'{value}' because it is not unique.";
				Protocol.ShowInformationMessage(message);
			}
		}

		/// <summary>
		/// Handles context menu actions for the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="contextMenu">Object that contains information related to the context menu.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="contextMenu"/> is not of type string[].</exception>
		/// <exception cref="ArgumentException">
		/// Throws if second element of converted <paramref name="contextMenu"/> can't be parsed as int.
		/// </exception>
		/// <exception cref="ArgumentException">Throws if <paramref name="contextMenu"/> is missing row keys.</exception>
		/// <exception cref="ArgumentException">Throws if row key doesn't exist in the table.</exception>
		public void HandleContextMenu(object contextMenu)
		{
			var input = contextMenu as string[]
				?? throw new ArgumentException($"Parameter '{nameof(contextMenu)}' can't be converted to string[].");

			if (!int.TryParse(input[1], out int value))
			{
				throw new ArgumentException($"Unable to parse selected option '{input[1]}' from '{nameof(contextMenu)}'.");
			}

			var option = (ContextMenuOption)value;
			if (HasRowKeys(option) && input.Length <= 2)
			{
				throw new ArgumentException("Parameter is missing row keys.");
			}

			switch (option)
			{
				case ContextMenuOption.PollAll:
					PollRows();
					break;

				case ContextMenuOption.Disable:
					if (input.Length == 3)
					{
						UpdateState(rows[input[2]], AdminState.Disabled);
						break;
					}

					foreach (string rowId in input.Skip(2).ToArray())
					{
						UpdateState(rows[rowId], AdminState.Disabled);
					}

					break;

				case ContextMenuOption.Enable:
					if (input.Length == 3)
					{
						UpdateState(rows[input[2]], AdminState.Enabled);
						break;
					}

					foreach (string rowId in input.Skip(2).ToArray())
					{
						UpdateState(rows[rowId], AdminState.Enabled);
					}

					break;

				case ContextMenuOption.ForceDisable:
					foreach (string rowId in input.Skip(2).ToArray())
					{
						UpdateState(rows[rowId], AdminState.ForceDisabled);
					}

					break;

				case ContextMenuOption.ForceEnable:
					foreach (string rowId in input.Skip(2).ToArray())
					{
						UpdateState(rows[rowId], AdminState.ForceEnabled);
					}

					break;

				case ContextMenuOption.DisableAll:
					foreach (KeyValuePair<string, PollableBase> row in rows)
					{
						if (row.Value.AdminStatus != AdminState.Disabled)
						{
							UpdateState(row.Value, AdminState.ForceDisabled);
						}
					}

					break;

				case ContextMenuOption.EnableAll:
					foreach (KeyValuePair<string, PollableBase> row in rows)
					{
						if (row.Value.AdminStatus != AdminState.Enabled)
						{
							UpdateState(row.Value, AdminState.ForceEnabled);
						}
					}

					break;
				case ContextMenuOption.Poll:
					if (input.Length == 3)
					{
						PollRow(rows[input[2]]);
						break;
					}

					foreach (string rowId in input.Skip(2).ToArray())
					{
						PollRow(rows[rowId]);
					}

					break;
				case ContextMenuOption.SuggestedInterval:
					if (input.Length == 3)
					{
						rows[input[2]].Interval = rows[input[2]].DefaultInterval;
						break;
					}

					foreach (string rowId in input.Skip(2).ToArray())
					{
						rows[rowId].Interval = rows[rowId].DefaultInterval;
					}

					break;
				default:
					throw new ArgumentException($"Unsupported ContextMenuOption '{option}'.");
			}

			FillTableNoDelete(rows);
		}

		/// <summary>
		/// Checks whether option with row keys was selected in context menu.
		/// </summary>
		/// <param name="option">Context menu option.</param>
		/// <returns>True if option with row keys was selected, false otherwise.</returns>
		private bool HasRowKeys(ContextMenuOption option)
		{
			switch (option)
			{
				case ContextMenuOption.PollAll:
					return false;

				case ContextMenuOption.DisableAll:
					return false;

				case ContextMenuOption.EnableAll:
					return false;

				default:
					return true;
			}
		}

		/// <summary>
		/// Updates row state.
		/// </summary>
		/// <param name="row">Row to update.</param>
		/// <param name="state">State to update to.</param>
		private void UpdateState(IPollable row, AdminState state)
		{
			switch (state)
			{
				case AdminState.Disabled:
					if (row.Children.Any(child => child.AdminStatus == AdminState.Enabled))
					{
						ShowChildren(row);
						return;
					}

					row.AdminStatus = AdminState.Disabled;
					row.PollStatus = PollStatus.Disabled;
					row.LastPoll = default;
					row.PollInfo = "-1";
					return;

				case AdminState.Enabled:
					if (row.Parents.Any(parent => parent.AdminStatus == AdminState.Disabled))
					{
						ShowParents(row);
						return;
					}

					row.AdminStatus = AdminState.Enabled;
					row.PollStatus = PollStatus.NotPolled;
					return;

				case AdminState.ForceDisabled:
					row.AdminStatus = AdminState.Disabled;
					row.PollStatus = PollStatus.Disabled;
					row.LastPoll = default;
					row.PollInfo = "-1";
					UpdateStates(row.Children, AdminState.ForceDisabled);
					return;

				case AdminState.ForceEnabled:
					row.AdminStatus = AdminState.Enabled;
					row.PollStatus = PollStatus.NotPolled;
					UpdateStates(row.Parents, AdminState.ForceEnabled);
					return;
			}
		}

		/// <summary>
		/// Updates states of the related rows.
		/// </summary>
		/// <param name="collection">List of rows to update.</param>
		/// <param name="state">State to update rows to.</param>
		private void UpdateStates(List<IPollable> collection, AdminState state)
		{
			foreach (IPollable item in collection)
			{
				UpdateState(item, state);
			}
		}

		/// <summary>
		/// Shows information message with parent rows of the row passed as parameter.
		/// </summary>
		/// <param name="row">Row for which to show parents.</param>
		private void ShowParents(IPollable row)
		{
			string parents = string.Join(Environment.NewLine, row.Parents.Where(parent => parent.AdminStatus == AdminState.Disabled).Select(parent => parent.Name));

			string message = $"Unable to enable '{row.Name}' because it depends on the following rows:{Environment.NewLine}" +
				$"{parents}{Environment.NewLine}" +
				$"Please enable them first or use [Force Enable].";

			Protocol.ShowInformationMessage(message);
		}

		/// <summary>
		/// Shows information message with child rows of the row passed as parameter.
		/// </summary>
		/// <param name="row">Row for which to show children.</param>
		private void ShowChildren(IPollable row)
		{
			string children = string.Join(Environment.NewLine, row.Children.Where(child => child.AdminStatus == AdminState.Enabled).Select(child => child.Name));

			string message = $"Unable to disable '{row.Name}' because the following rows are dependent on it:{Environment.NewLine}" +
				$"{children}{Environment.NewLine}" +
				$"Please disable them first or use [Force Disable].";

			Protocol.ShowInformationMessage(message);
		}

		/// <summary>
		/// Checks whether poll period has elapsed.
		/// </summary>
		/// <param name="interval">Poll period.</param>
		/// <param name="lastPoll">Last poll timestamp.</param>
		/// <returns>True if poll period has elapsed, false otherwise.</returns>
		private bool CheckLastPollTime(double interval, DateTime lastPoll)
		{
			return (DateTime.Now - lastPoll).TotalSeconds > interval;
		}

		/// <summary>
		/// Polls a row.
		/// </summary>
		/// <param name="row">Row to poll.</param>
		/// <returns>True if poll did occur, false otherwise.</returns>
		private bool PollRow(PollableBase row)
		{
			if (row.AdminStatus == AdminState.Disabled)
			{
				return false;
			}

			if (!row.CheckDependencies())
			{
				row.PollStatus = PollStatus.Failed;
				row.Update(CreateTableRow(row));
				return false;
			}

			bool pollSucceeded = row.InitiatePoll();

			if (!pollSucceeded)
			{
				row.PollStatus = PollStatus.Failed;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Polls all rows by calling <see cref="PollRow"/> for every row.
		/// </summary>
		private void PollRows()
		{
			foreach (KeyValuePair<string, PollableBase> row in rows)
			{
				PollRow(row.Value);
			}
		}

		/// <summary>
		/// Loads <see cref="PollingmanagerQActionRow"/> and updates internal row with the same <paramref name="rowPk"/>.
		/// </summary>
		/// <param name="rowPk">Row to load and update.</param>
		/// <returns>Updated internal row.</returns>
		/// <exception cref="ArgumentException">Throws if <paramref name="rowPk"/> doesn't exist in the table.</exception>
		private PollableBase LoadRow(string rowPk)
		{
			if (!rows.ContainsKey(rowPk))
			{
				throw new ArgumentException($"Row key '{rowPk}' doesn't exist in the Polling Manager table.");
			}

			object[] tableRow = (object[])Protocol.GetRow(tablePid, rowPk);
			if (tableRow != null && tableRow[0] != null)
			{
				rows[rowPk].Update(tableRow);
			}

			return rows[rowPk];
		}

		/// <summary>
		/// Loads all <see cref="PollingmanagerQActionRow"/> and updates internal rows respectively.
		/// </summary>
		private void LoadRows()
		{
			foreach (KeyValuePair<string, PollableBase> row in rows)
			{
				LoadRow(row.Key);
			}
		}

		/// <summary>
		/// Creates the <see cref="PollingmanagerQActionRow"/>.
		/// </summary>
		/// <param name="rowKey">Row key.</param>
		/// <param name="value">Row to create.</param>
		/// <returns>Instance of <see cref="PollingmanagerQActionRow"/>.</returns>
		private PollingmanagerQActionRow CreateTableRow(PollableBase value)
		{
			return new PollingmanagerQActionRow
			{
				Pollingmanager_name = value.Name,
				Pollingmanager_id = value.ID,
				Pollingmanager_description = string.IsNullOrWhiteSpace(value.Description) ? value.Name : value.Description,
				Pollingmanager_interval = value.Interval.Equals(double.NaN) ? value.DefaultInterval : value.Interval,
				Pollingmanager_suggestedinterval = value.DefaultInterval,
				Pollingmanager_adminstatus = value.AdminStatus,
				Pollingmanager_lastpolltime = value.LastPoll == default ? Convert.ToDouble(PollStatus.NotPolled) : value.LastPoll.ToOADate(),
				Pollingmanager_lastpollstatus = value.AdminStatus == AdminState.Disabled ? PollStatus.Disabled : value.PollStatus,
				Pollingmanager_lastpollstatusinfo = value.PollInfo,
			};
		}

		/// <summary>
		/// Creates the array of the <see cref="PollingmanagerQActionRow"/>.
		/// </summary>
		/// <param name="rows">Rows to create.</param>
		/// <returns>Array of the <see cref="PollingmanagerQActionRow"/>.</returns>
		private PollingmanagerQActionRow[] CreateTableRows(Dictionary<string, PollableBase> rows)
		{
			List<PollingmanagerQActionRow> tableRows = new List<PollingmanagerQActionRow>();

			foreach (KeyValuePair<string, PollableBase> row in rows)
			{
				tableRows.Add(CreateTableRow(row.Value));
			}

			return tableRows.ToArray();
		}

		/// <summary>
		/// Sets the content of the table to the provided content.
		/// </summary>
		/// <param name="rows">Rows to fill the table with.</param>
		private void FillTable(Dictionary<string, PollableBase> rows)
		{
			PollingmanagerQActionRow[] tableRows = CreateTableRows(rows);

			Protocol.FillArray(tablePid, tableRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
		}

		/// <summary>
		/// Add the provided rows to the table.
		/// </summary>
		/// <param name="rows">Rows to add to the table.</param>
		private void FillTableNoDelete(Dictionary<string, PollableBase> rows)
		{
			PollingmanagerQActionRow[] tableRows = CreateTableRows(rows);

			Protocol.FillArray(tablePid, tableRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Partial);
		}

		public ResponseHandler GetResponseHandler(int trigger)
		{
			if (!responseHandlers.TryGetValue(trigger, out ResponseHandler responseHandler))
			{
				throw new NotImplementedException();
			}

			return responseHandler;
		}

		public void ProcessResponse(int trigger)
		{
			if (!responseHandlers.TryGetValue(trigger, out ResponseHandler handler))
			{
				throw new NotImplementedException();
			}

			var succes = handler.ProcessResponse(Protocol);
		}
	}
}