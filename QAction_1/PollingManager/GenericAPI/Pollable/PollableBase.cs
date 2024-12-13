namespace Skyline.DataMiner.PollingManager
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Enums;
	using Skyline.Protocol.PollingManager.GenericAPI.Exceptions;

	/// <summary>
	/// Base class that implements <see cref="IPollable"/>.
	/// </summary>
	public abstract class PollableBase : IPollable
	{
		private readonly Dictionary<int, object> _singleParameterIds;
		private List<int> _tableParameterIds;

		/// <summary>
		/// Initializes a new instance of the <see cref="PollableBase"/> class.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <param name="description">Description of the PollingManager table row.</param>
		public PollableBase(SLProtocol protocol, string description)
		{
			Protocol = protocol;
			Description = description;
			Interval = double.NaN;
			SuggestedInterval = 10;
			LastPoll = default;
			PollStatus = PollStatus.NotPolled;
			PollInfo = string.Empty;
			AdminStatus = AdminState.Enabled;
			ActionId = 0;
			Mandatory = false;
			_singleParameterIds = new Dictionary<int, object>();
			_tableParameterIds = new List<int>();
		}

		public int ActionId { get; set; }

		public AdminState AdminStatus { get; set; }

		public List<IPollable> Children { get; set; } = new List<IPollable>();

		public Dictionary<int, Dependency> Dependencies { get; set; } = new Dictionary<int, Dependency>();

		public string Description { get; set; }

		public int ID { get; set; }

		public double Interval { get; set; }

		public DateTime LastPoll { get; set; }

		public bool Mandatory { get; set; }

		public string Name { get; set; }

		public List<IPollable> Parents { get; set; } = new List<IPollable>();

		public string PollInfo { get; set; }

		public PollStatus PollStatus { get; set; }

		public SLProtocol Protocol { get; set; }

		public double SuggestedInterval { get; set; }

		/// <summary>
		/// Adds child without creating two way relation between elements. This shouldn't be used directly. Use <see cref="AddChildren"/> instead.
		/// </summary>
		/// <param name="child">Child element.</param>
		/// <exception cref="InvalidOperationException">Throws if <paramref name="child"/> is already this elements parent.</exception>
		void IPollable.AddChild(IPollable child)
		{
			if (Parents.Contains(child))
			{
				throw new InvalidOperationException($"Circular dependency, '{child.Name}' is already a parent of '{Name}'.");
			}

			if (Children.Contains(child))
			{
				return;
			}

			Children.Add(child);
		}

		/// <summary>
		/// Adds children to this element, and adds this element as a parent of each child passed as parameter.
		/// </summary>
		/// <param name="children">Child elements.</param>
		/// <exception cref="InvalidOperationException">Throws if any <paramref name="children"/> element is already this elements parent.</exception>
		public void AddChildren(params IPollable[] children)
		{
			foreach (IPollable child in children)
			{
				if (Parents.Contains(child))
				{
					throw new InvalidOperationException($"Circular dependency, '{child.Name}' is already a parent of '{Name}'.");
				}

				if (Children.Contains(child))
				{
					return;
				}

				child.AddParent(this);
				Children.Add(child);
			}
		}

		/// <summary>
		/// Adds a dependency.
		/// </summary>
		/// <param name="paramId">The parameter ID of the dependency.</param>
		/// <param name="dependency">The dependency details.</param>
		public void AddDependency(int paramId, Dependency dependency)
		{
			Dependencies.Add(paramId, dependency);
		}

		/// <summary>
		/// Link parameter Id's to poll entry, this is then used to clear the parameters when polling is disabled.
		/// </summary>
		/// <param name="singleParameters">A dictionary of single parameters with their IDs and values.</param>
		/// <param name="tableParameters">A list of table parameter IDs.</param>
		public void AddParameters(Dictionary<int, object> singleParameters, List<int> tableParameters)
		{
			foreach (var pair in singleParameters)
			{
				_singleParameterIds[pair.Key] = pair.Value; // Overwrites if the key exists
			}

			_tableParameterIds.AddRange(tableParameters);
			_tableParameterIds = _tableParameterIds.Distinct().ToList();
		}

		/// <summary>
		/// Adds parent without creating two way relation between elements. This shouldn't be used directly. Use <see cref="AddParents"/> instead.
		/// </summary>
		/// <param name="parent">Parent element.</param>
		/// <exception cref="InvalidOperationException">Throws if <paramref name="parent"/> is already this elements child.</exception>
		void IPollable.AddParent(IPollable parent)
		{
			if (Children.Contains(parent))
			{
				throw new InvalidOperationException($"Circular dependency, '{parent.Name}' is already a child of '{Name}'.");
			}

			if (Parents.Contains(parent))
			{
				return;
			}

			Parents.Add(parent);
		}

		/// <summary>
		/// Adds parents to this element, and adds this element as a child of each parent passed as parameter.
		/// </summary>
		/// <param name="parents">Parent elements.</param>
		/// <exception cref="InvalidOperationException">Throws if any <paramref name="parents"/> element is already this elements child.</exception>
		public void AddParents(params IPollable[] parents)
		{
			foreach (IPollable parent in parents)
			{
				if (Children.Contains(parent))
				{
					throw new InvalidOperationException($"Circular dependency, '{parent.Name}' is already a child of '{Name}'.");
				}

				if (Parents.Contains(parent))
				{
					return;
				}

				parent.AddChild(this);
				Parents.Add(parent);
			}
		}

		/// <summary>
		/// Gets dependent parameters and compares their values with dependencies. Sets <see cref="PollInfo"/> to first condition not satisfied.
		/// </summary>
		/// <returns>False if any condition is not satisfied, otherwise true.</returns>
		public bool CheckDependencies()
		{
			try
			{
				foreach (KeyValuePair<int, Dependency> dependency in Dependencies)
				{
					object parameter = Protocol.GetParameter(dependency.Key)
						?? throw new Exception($"Parameter with ID '{dependency.Key}' doesn't exist.");

					if (dependency.Value.Value is double)
					{
						if (dependency.Value.ShouldEqual)
						{
							if (!CheckDoubleParameter(parameter, dependency.Value.Value))
							{
								PollInfo = dependency.Value.Message;
								PollStatus = PollStatus.Failed;
								LastPoll = DateTime.Now;
								return false;
							}
						}
						else
						{
							if (CheckDoubleParameter(parameter, dependency.Value.Value))
							{
								PollInfo = dependency.Value.Message;
								PollStatus = PollStatus.Failed;
								LastPoll = DateTime.Now;
								return false;
							}
						}
					}
					else if (dependency.Value.Value is string)
					{
						if (dependency.Value.ShouldEqual)
						{
							if (!CheckStringParameter(parameter, dependency.Value.Value))
							{
								PollInfo = dependency.Value.Message;
								PollStatus = PollStatus.Failed;
								LastPoll = DateTime.Now;
								return false;
							}
						}
						else
						{
							if (CheckStringParameter(parameter, dependency.Value.Value))
							{
								PollInfo = dependency.Value.Message;
								PollStatus = PollStatus.Failed;
								LastPoll = DateTime.Now;
								return false;
							}
						}
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				Protocol.Log($"QA{Protocol.QActionID}|{Protocol.GetTriggerParameter()}|PollableBase.CheckDependencies|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);

				PollInfo = "Something went wrong. Please check logs.";
				return false;
			}
		}

		/// <summary>
		/// Clears all linked parameters when the polling entry <see cref="AdminStatus"/> is set to Disabled.
		/// </summary>
		public void ClearParameters()
		{
			if (_singleParameterIds.Any())
			{
				Protocol.SetParameters(_singleParameterIds.Keys.ToArray(), _singleParameterIds.Values.ToArray());
			}

			foreach (var tablePid in _tableParameterIds)
			{
				string[] keys = Protocol.GetKeys(tablePid);
				Protocol.DeleteRow(tablePid, keys);
			}
		}

		/// <summary>
		/// This method gets called by <see cref="PollingManager"/>.
		/// </summary>
		/// <returns cref="PollableType">Will return <see cref="PollableType.TriggerAction"/> when a actionId is specified. Otherwise will return <see cref="PollableType.ProcessInCode"/>.</returns>
		/// <exception cref="PollingException">Throws if the initiate of the poll fails.</exception>
		public PollableType InitiatePoll()
		{
			try
			{
				PollConfiguration();
				if (ActionId != 0)
				{
					Protocol.NotifyProtocol(221/*NT_RUN_ACTION*/, ActionId, null);
					return PollableType.TriggerAction;
				}
				else
				{
					return PollableType.ProcessInCode;
				}
			}
			catch (Exception ex)
			{
				throw ex is PollingException ? ex : new PollingException("Failed to initiate the poll.", ex);
			}
		}

		/// <summary>
		/// Updates current state of <see cref="PollableBase"/>.
		/// </summary>
		/// <param name="row">Row on which to base the update.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="row"/> has length less then 9.</exception>
		public void Update(object[] row)
		{
			if (row.Length < 10)
			{
				throw new ArgumentException($"Parameter '{nameof(row)}' must have at least 9 elements, but has '{row.Length}'.");
			}

			Interval = Convert.ToDouble(row[(int)Column.Interval]);
			PollInfo = Convert.ToString(row[(int)Column.PollInfo]) ?? string.Empty;
			AdminStatus = (AdminState)Convert.ToDouble(row[(int)Column.AdminStatus]);
		}

		/// <summary>
		/// Method to be implemented by extending class. This method gets called by <see cref="InitiatePoll"/>.
		/// </summary>
		protected abstract void PollConfiguration();

		/// <summary>
		/// Compares values of boxed double types.
		/// </summary>
		/// <param name="parameter">Parameter object.</param>
		/// <param name="value">Object to compare value against.</param>
		/// <returns>True if the boxed values are the same, otherwise false.</returns>
		/// <exception cref="ArgumentException">Throws if boxed <paramref name="parameter"/> type is not double.</exception>
		private bool CheckDoubleParameter(object parameter, object value)
		{
			return parameter is double d
				? d == (double)value
				: throw new ArgumentException($"{nameof(parameter)} is not of type double.");
		}

		/// <summary>
		/// Compares values of boxed string types.
		/// </summary>
		/// <param name="parameter">Parameter object.</param>
		/// <param name="value">Object to compare value against.</param>
		/// <returns>True if the boxed values are the same, otherwise false.</returns>
		/// <exception cref="ArgumentException">Throws if boxed <paramref name="parameter"/> type is not string.</exception>
		private bool CheckStringParameter(object parameter, object value)
		{
			return parameter is string s
				? string.Compare(s, (string)value, false) == 0
				: throw new ArgumentException($"{nameof(parameter)} is not of type string.");
		}
	}
}
