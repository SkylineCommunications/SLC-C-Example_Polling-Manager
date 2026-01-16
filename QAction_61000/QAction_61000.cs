using System;
using Skyline.DataMiner.PollingManager;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class.
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
			// Get trigger row key.
			int trigger = protocol.GetTriggerParameter();

			// Updates row with specific key that was triggered by specific column.
			PollingManagerContainer.GetManager(protocol).ProcessResponse(trigger);
		}
		catch (Exception ex)
		{
			protocol.Log(
				$"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Polling Manager - Responses|Exception thrown:{Environment.NewLine}{ex}",
				LogType.Error,
				LogLevel.NoLogging);
		}
	}
}