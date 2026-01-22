namespace Skyline.Protocol.PollingManager.CustomCode.ClearParameters
{
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.ClearParameters;

	internal static class ClearParametersConfiguration
	{
		public static readonly ClearParameters SystemInfo = new ClearParametersBuilder()
		   .AddSingle(Parameter.systemname_20, string.Empty)
		   .AddSingle(Parameter.serialnumber_21, NotAvailable)
		   .Build();

		public static readonly ClearParameters CpuInfo = new ClearParametersBuilder()
		   .AddTable(Parameter.Cpus.tablePid)
		   .Build();

		public static readonly ClearParameters TemperatureInfo = new ClearParametersBuilder()
		   .AddSingle(Parameter.temp1_30, null)
		   .AddSingle(Parameter.temp2_31, null)
		   .Build();

		public static readonly ClearParameters InterfacesInfo = new ClearParametersBuilder()
		   .AddTable(Parameter.Interfaces.tablePid)
		   .Build();

		public static readonly ClearParameters VlanInfo = new ClearParametersBuilder()
		   .AddTable(Parameter.Vlans.tablePid)
		   .Build();

		public static readonly ClearParameters PvstInfo = new ClearParametersBuilder()
			   .AddTable(Parameter.Vlanpvst.tablePid)
			   .Build();

		private const string NotAvailable = "-1";
	}
}