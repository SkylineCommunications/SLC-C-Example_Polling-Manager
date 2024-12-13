namespace Skyline.Protocol.PollingManager.GenericAPI.Exceptions
{
	using System;

	/// <summary>
	///  Represents errors that occur during polling execution.
	/// </summary>
	public class PollingException : Exception
	{
		/// <summary>Initializes a new instance of the <see cref="PollingException" /> class with a specified error message.</summary>
		/// <param name="message">The message that describes the error.</param>
		public PollingException(string message) : base(message)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="PollingException" /> class.</summary>
		public PollingException()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="PollingException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
		public PollingException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
