﻿using System;
using JetBrains.Annotations;

namespace Alpaca.Markets
{
    /// <summary>
    /// Encapsulates single trading day information from Alpaca REST API.
    /// </summary>
    public interface ICalendar
    {
        /// <summary>
        /// Gets trading date in EST time zone.
        /// </summary>
        [UsedImplicitly]
        DateTime TradingDateEst { get; }

        /// <summary>
        /// Gets trading date in UTC time zone.
        /// </summary>
        DateTime TradingDateUtc { get; }

        /// <summary>
        /// Gets trading date open time in EST time zone.
        /// </summary>
        [UsedImplicitly]
        DateTime TradingOpenTimeEst { get; }

        /// <summary>
        /// Gets trading date open time in UTC time zone.
        /// </summary>
        [UsedImplicitly]
        DateTime TradingOpenTimeUtc { get; }

        /// <summary>
        /// Gets trading date close time in EST time zone.
        /// </summary>
        [UsedImplicitly]
        DateTime TradingCloseTimeEst { get; }

        /// <summary>
        /// Gets trading date close time in UTC time zone.
        /// </summary>
        DateTime TradingCloseTimeUtc { get; }
    }
}
