﻿namespace Skyline.DataMiner.Protocol.PollingManager.Enums
{
    using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Represents columns of the <see cref="PollingmanagerQActionTable"/>.
	/// </summary>
    public enum Column
    {
        Id = 0,
        Name = 1,
        Period = 2,
        DefaultPeriod = 3,
        PeriodType = 4,
        LastPoll = 5,
        Status = 6,
        Reason = 7,
        Poll = 8,
        State = 9,
    }
}
