namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Enums;
	using Skyline.Protocol.PollingManager.GenericAPI.Exceptions;
	using Skyline.Protocol.PollingManager.GenericAPI.Handlers;

	/// <summary>
	/// Handler for <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public class PollingManager
	{
		private readonly Dictionary<int, ResponseHandler> responseHandlers;

		private readonly Dictionary<string, PollableBase> rows = new Dictionary<string, PollableBase>();

		private readonly int tablePid;

		/// <summary>
		/// Initializes a new instance of the <see cref="PollingManager"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="tablePid">Polling manager table instance.</param>
		/// <param name="configuration.ListRows">Rows to add to the <paramref name="tablePid"/>.</param>
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
						rows[input[2]].Interval = rows[input[2]].SuggestedInterval;
						break;
					}

					foreach (string rowId in input.Skip(2).ToArray())
					{
						rows[rowId].Interval = rows[rowId].SuggestedInterval;
					}

					break;
				default:
					throw new ArgumentException($"Unsupported ContextMenuOption '{option}'.");
			}

			FillTableNoDelete(rows);
		}

		/// <summary>
		/// Handles sets on the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="rowKey">Row key.</param>
		/// <param name="column">Column on which set was performed.</param>
		/// <param name="value">Value that needs to be set.</param>
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
					tableRow = LoadRow(rowKey);
					PollRow(tableRow);
					break;

				default:
					throw new ArgumentException($"Unsupported Column '{column}'.");
			}

			FillTableNoDelete(rows);
		}

		/// <summary>
		/// Retrieves the handler for the <paramref name="triggerId"/>, initiate <see cref="IPollingManagerResponseHandler.Process(SLProtocol)"/> and updates the <see cref="PollStatus"/>.
		/// </summary>
		/// <param name="triggerId">ID of the trigger parameter linked to the handler.</param>
		public void ProcessResponse(int triggerId)
		{
			if (!responseHandlers.TryGetValue(triggerId, out ResponseHandler handler))
			{
				throw new NotImplementedException($"No response handler implemented for id:{triggerId}.");
			}

			try
			{
				if (rows[handler.RowName].AdminStatus.Equals(AdminState.Enabled))
				{
					handler.Process(Protocol);
					UpdatePollingStatus(rows[handler.RowName], PollStatus.Succeeded);
				}
			}
			catch (Exception ex)
			{
				UpdatePollingStatus(rows[handler.RowName], PollStatus.Failed, ex is PollingException ? ex.Message : $"Failed to Process Response. {ex.Message}");
			}

			FillTableNoDelete(rows);
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
		/// Creates the <see cref="PollingmanagerQActionRow"/>.
		/// </summary>
		/// <param name="value">Row to create.</param>
		/// <returns>Instance of <see cref="PollingmanagerQActionRow"/>.</returns>
		private PollingmanagerQActionRow CreateTableRow(PollableBase value)
		{
			value.Description = string.IsNullOrWhiteSpace(value.Description) ? value.Name : value.Description;
			value.Interval = value.Interval.Equals(double.NaN) ? value.SuggestedInterval : value.Interval;

			return new PollingmanagerQActionRow
			{
				Pollingmanager_name = value.Name,
				Pollingmanager_id = value.ID,
				Pollingmanager_description = value.Description,
				Pollingmanager_interval = value.Interval,
				Pollingmanager_suggestedinterval = value.SuggestedInterval,
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
		/// Polls a row.
		/// </summary>
		/// <param name="row">Row to poll.</param>
		/// <returns>True if poll did occur, false otherwise.</returns>
		private bool PollRow(PollableBase row)
		{
			try
			{
				if (row.AdminStatus == AdminState.Disabled)
				{
					return false;
				}

				if (!row.CheckDependencies())
				{
					return false;
				}

				var pollableType = row.InitiatePoll();
				if (pollableType.Equals(PollableType.ProcessInCode))
				{
					row.PollStatus = PollStatus.Succeeded;
					row.LastPoll = DateTime.Now;
					row.PollInfo = "-1";
				}

				return true;
			}
			catch (PollingException e)
			{
				UpdatePollingStatus(row, PollStatus.Failed, e.Message);
				return false;
			}
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

		private void UpdatePollingStatus(IPollable row, PollStatus pollStatus, string lastPollInfo = "-1")
		{
			row.PollStatus = pollStatus;
			row.LastPoll = DateTime.Now;
			row.PollInfo = lastPollInfo;
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
					if (row.Mandatory)
					{
						string message = $"Unable to disable '{row.Name}' because the following row is mandatory.";
						Protocol.ShowInformationMessage(message);
						return;
					}

					if (row.Children.Any(child => child.AdminStatus == AdminState.Enabled))
					{
						ShowChildren(row);
						return;
					}

					row.AdminStatus = AdminState.Disabled;
					row.PollStatus = PollStatus.Disabled;
					row.LastPoll = default;
					row.PollInfo = "-1";
					row.ClearParameters();
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
					if (row.Mandatory)
					{
						string message = $"Unable to disable '{row.Name}' because the following row is mandatory.";
						Protocol.ShowInformationMessage(message);
						return;
					}

					row.AdminStatus = AdminState.Disabled;
					row.PollStatus = PollStatus.Disabled;
					row.LastPoll = default;
					row.PollInfo = "-1";
					row.ClearParameters();
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
	}
}