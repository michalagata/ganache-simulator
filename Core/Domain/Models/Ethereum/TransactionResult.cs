namespace AnubisWorks.GanacheSimulator.Domain.Models.Ethereum
{
    using GanacheSimulator.Domain.Enums;
    using Nethereum.Util;
    using System;
    using System.Numerics;

    public class TransactionResult
    {
        public TransactionResult()
        {
            DateTransaction = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public string To { get; set; }
        public string From { get; set; }
        public string TxHash { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public TransactionDetails TransactionDetails { get; set; }
        public string Message { get; set; }
        public long DateTransaction { get; set; }
        public BigInteger EstimatedGas { get; set; }
        public BigInteger OperatingCostWei { get; set; }
        public double OperatingCostUsd { get; set; }
        public BigInteger ValueWei { get; set; }
        public double ValueUsd { get; set; }
        public double EtherPriceInUsd { get; set; }
    }
}