namespace AnubisWorks.GanacheSimulator.Domain.Models.Ethereum
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Linq;
    using System.Net;
    using System.Numerics;
    using System.Threading.Tasks;
    using Nethereum.ABI.FunctionEncoding.Attributes;
    using Nethereum.Contracts;

    [Function("balanceOf", "uint256")]
    public class BalanceOfFunction : FunctionMessage
    {
        [Parameter("address", "_owner", 1)]
        public string Owner { get; set; }
    }

    [FunctionOutput]
    public class BalanceOfOutputDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "balance", 1)]
        public BigInteger Balance { get; set; }
    }

    [FunctionOutput]
    public class BalanceOfOutputMultipleDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "balance1", 1)]
        public BigInteger Balance1 { get; set; }

        [Parameter("uint256", "balance2", 2)]
        public BigInteger Balance2 { get; set; }

        [Parameter("uint256", "balance1", 3)]
        public BigInteger Balance3 { get; set; }
    }
}