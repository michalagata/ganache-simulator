namespace AnubisWorks.GanacheSimulator.Domain.Models.Exchanges
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Data
    {
        [JsonProperty("base")]
        public string BaseCoin { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("amount")]
        public string Amount { get; set; }
    }

    public class Exchange
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}