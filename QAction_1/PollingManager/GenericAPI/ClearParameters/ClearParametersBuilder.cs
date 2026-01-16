namespace Skyline.Protocol.PollingManager.GenericAPI.ClearParameters
{
	using System.Collections.Generic;

	public class ClearParametersBuilder
	{
		private readonly Dictionary<int, object> _single = new Dictionary<int, object>();

		private readonly List<int> _table = new List<int>();

		public ClearParametersBuilder AddSingle(int key, object value)
		{
			_single[key] = value;
			return this;
		}

		public ClearParametersBuilder AddTable(int key)
		{
			_table.Add(key);
			return this;
		}

		public ClearParameters Build()
		{
			return new ClearParameters(_single, _table);
		}
	}
}