﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alpaca.Markets
{
    /// <summary>
    /// Supported asset types in Polygon REST API.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [Obsolete("This enum will be removed in the next major SDK release.", true)]
    public enum MarketDataType
    {
        /// <summary>
        /// Equities.
        /// </summary>
        [EnumMember(Value = "equities")]
        Equities,

        /// <summary>
        /// Indexes.
        /// </summary>
        [EnumMember(Value = "index")]
        Indexes,

        /// <summary>
        /// Currencies.
        /// </summary>
        [EnumMember(Value = "currencies")]
        Currencies
    }
}
