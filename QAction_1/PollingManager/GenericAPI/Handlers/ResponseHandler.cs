namespace Skyline.Protocol.PollingManager.GenericAPI.Handlers
{
	using System;
	using Skyline.DataMiner.Scripting;
	using Skyline.Protocol.PollingManager.GenericAPI.Exceptions;

	/// <summary>
	/// Base class that implements <see cref="IPollingManagerResponseHandler"/>.
	/// </summary>
	public abstract class ResponseHandler : IPollingManagerResponseHandler
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseHandler"/> class.
		/// </summary>
		/// <param name="rowName">Link to PK of the <see cref="PollingmanagerQActionTable"/>.</param>
		public ResponseHandler(string rowName)
		{
			RowName = rowName;
		}

		public string RowName { get; }

		/// <summary>
		/// Process the response for a row in the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		/// <exception cref="PollingException">
		/// Throws if processing of the response has failed./>.
		/// </exception>
		public void Process(SLProtocol protocol)
		{
			try
			{
				ProcessResponse(protocol);
			}
			catch (Exception ex)
			{
				throw ex is PollingException ? ex : new PollingException("Failed to process the response.", ex);
			}
		}

		/// <summary>
		/// Process the response for a row in the <see cref="PollingmanagerQActionTable"/>.
		/// </summary>
		/// <param name="protocol">Link with SLProtocol process.</param>
		protected abstract void ProcessResponse(SLProtocol protocol);
	}
}
