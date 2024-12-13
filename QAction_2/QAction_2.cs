using System;

using Skyline.DataMiner.PollingManager;
using Skyline.DataMiner.Scripting;
using Skyline.Protocol.PollingManager;

/// <summary>
/// DataMiner QAction Class: After Startup.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
			// Polling Manager Initialization
			var configuration = new PollingManagerConfiguration(protocol);
			PollingManagerContainer.RemoveInstance(protocol);
			PollingManagerContainer.AddManager(protocol, configuration);
		}
		catch (Exception ex)
		{
			protocol.Log(
				$"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|After Startup|Exception thrown:{Environment.NewLine}{ex}",
				LogType.Error,
				LogLevel.NoLogging);
		}
	}
}
