namespace Skyline.Protocol.PollingManager.GenericAPI.Enums
{
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents states of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public enum AdminState
	{
		Disabled = 0,

		Enabled = 1,

		ForceDisabled = 3,

		ForceEnabled = 4,
	}
}