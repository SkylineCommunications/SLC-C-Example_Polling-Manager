namespace Skyline.Protocol.PollingManager.GenericAPI.Handlers
{
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents base for handling a response to a row in the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
	public interface IPollingManagerResponseHandler
	{
		string EntryName { get; }

		/// <summary>
		/// Process the response for a row in the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		void Process(SLProtocol protocol);
	}
}