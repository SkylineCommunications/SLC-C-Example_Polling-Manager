namespace Skyline.Protocol.PollingManager.GenericAPI.ClearParameters
{
	using System.Collections.Generic;
	using Skyline.DataMiner.PollingManager;

	public class ClearParameters
	{
		public ClearParameters(Dictionary<int, object> singleParameters, List<int> tableParameters)
		{
			SingleParameters = new Dictionary<int, object>(singleParameters);
			TableParameters = new List<int>(tableParameters);
		}

		public IReadOnlyDictionary<int, object> SingleParameters { get; }

		public IReadOnlyList<int> TableParameters { get; }

		public void ApplyToRow(IPollable row)
		{
			row.AddParameters(SingleParameters, TableParameters);
		}
	}
}