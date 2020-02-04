namespace AnubisWorks.GanacheSimulator.Domain.Interfaces
{
    using GanacheSimulator.Domain.Models.Exchanges;
    using Nethereum.Util;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public interface IEtherSystem
    {
        Nethereum.RPC.Accounts.IAccount LoadFromPrivateKey(string privateKey);

        Nethereum.RPC.Accounts.IAccount LoadFromKeyStore(string keyStoreEncryptedJson, string password);

        Nethereum.RPC.Accounts.IAccount LoadFromPassword(string address, string password);

        BigDecimal FromWeiToEther(BigInteger totalWeis);

        BigDecimal FromUsdToEther(double totalInDollars);

        BigInteger FromUsdToWei(double totalInDollars);

        Task<BigInteger> GetBalanceInWei(Nethereum.RPC.Accounts.IAccount account);

        Task<BigDecimal> GetBalanceInEther(Nethereum.RPC.Accounts.IAccount account);

        Task<double> GetBalanceInUsd(Nethereum.RPC.Accounts.IAccount account);

        double GetEtherUnitPriceInUsd();

        Task<BigInteger> EstimateOperatingCostInWei(Nethereum.RPC.Accounts.IAccount account, string toAddress, BigInteger value);

        Task<BigInteger> EstimateGas(Nethereum.RPC.Accounts.IAccount account, string toAddress, BigInteger value);

        double EstimateOperatingCostInUsd(BigInteger value);

        Task<Models.Ethereum.TransactionResult> TransferTo(Nethereum.RPC.Accounts.IAccount accountFrom, string accountTo, double totalInDollars);
    }
}