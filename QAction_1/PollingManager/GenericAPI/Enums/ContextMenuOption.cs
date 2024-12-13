namespace Skyline.Protocol.PollingManager.GenericAPI.Enums
{
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents context menu options of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public enum ContextMenuOption
	{
		Enable = 1,
		ForceEnable = 2,
		Disable = 3,
		ForceDisable = 4,
		Poll = 5,
		EnableAll = 11,
		DisableAll = 12,
		PollAll = 13,
		SuggestedInterval = 21,
	}
}
