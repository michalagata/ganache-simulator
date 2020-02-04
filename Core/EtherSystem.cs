namespace AnubisWorks.GanacheSimulator.Application.Ethereum
{
    using System;
    using GanacheSimulator.Domain.Interfaces;
    using Nethereum.Util;
    using Nethereum.Web3;
    using Nethereum.Web3.Accounts;
    using System.Globalization;
    using System.Net;
    using System.Numerics;
    using System.Threading.Tasks;
    using static Nethereum.Util.UnitConversion;
    using GanacheSimulator.Domain.Constants;
    using Newtonsoft.Json;
    using Nethereum.Hex.HexTypes;
    using Nethereum.Hex.HexConvertors.Extensions;
    using Nethereum.Web3.Accounts.Managed;
    using GanacheSimulator.Domain.Models.Ethereum;
    using GanacheSimulator.Domain.Models.Exchanges;
    using Nethereum.RPC.Accounts;
    using AnubisWorks.GanacheSimulator.Domain.Enums;
    using AnubisWorks.GanacheSimulator.Functions;
    using Nethereum.Signer;

    public class EtherSystem : IEtherSystem
    {

        private readonly double UsdExchangeRate;
        private readonly string RpcServerUrl;

        public EtherSystem(double usdExchangeRate, string rpcServerUrl)
        {
            RpcServerUrl = rpcServerUrl;
            UsdExchangeRate = usdExchangeRate;
        }

        public static IEtherSystem Initialize(double usdExchangeRate, string rpcServerUrl)
        {
            var myWallet = new EtherSystem(usdExchangeRate, rpcServerUrl);
            return myWallet;
        }

        public Account GenerateNewAccount()
        {
            EthECKey ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            string privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            Account account = new Nethereum.Web3.Accounts.Account(privateKey);

            return account;
        }

        public async Task<string> NewAccount(string identifier, string rpcServerUrl)
        {
            return await new Web3(url: rpcServerUrl).Personal.NewAccount.SendRequestAsync(identifier);
        }

        public string SetTransactionForSmartContract(string to, BigInteger initialWalletValue)
        {
            try
            {
                Web3 wInst = new Web3(LoadFromPrivateKey(to), RpcServerUrl);
                var transferHandler = wInst.Eth.GetContractTransactionHandler<TransferFunction>();
                var transfer = new TransferFunction()
                {
                    To = to,
                    TokenAmount = initialWalletValue
                };
                var transactionReceipt = transferHandler.SendRequestAndWaitForReceiptAsync(to, transfer).Result;
                
                return transactionReceipt.TransactionHash;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return string.Empty;
            }
        }

        public IAccount LoadFromPassword(string address, string password)
        {
            var managedAccount = new ManagedAccount(address, password);
            return managedAccount.TransactionManager.Account;
        }

        public IAccount LoadFromPrivateKey(string privateKey)
        {
            return new Account(privateKey);
        }

        public IAccount LoadFromKeyStore(string keyStoreEncryptedJson, string password)
        {
            return Account.LoadFromKeyStore(keyStoreEncryptedJson, password);
        }

        public BigDecimal FromUsdToEther(double totalInDollars)
        {
            var weis = FromUsdToWei(totalInDollars);
            return UnitConversion.Convert.FromWeiToBigDecimal(weis, EthUnit.Ether);
        }

        public BigInteger FromUsdToWei(double totalInDollars)
        {
            BigInteger _totalInDollars = new BigInteger(System.Math.Ceiling(totalInDollars));
            var getEtherPriceInUsd = GetEtherUnitPriceInUsd();
            var convertToCents = new BigInteger(getEtherPriceInUsd * 100);
            var onePenny = BigInteger.Divide(new BigInteger(GanacheSimulator.Domain.Constants.Constants.ETHER), convertToCents);
            var oneDollar = BigInteger.Multiply(onePenny, new BigInteger(100));
            return BigInteger.Multiply(oneDollar, _totalInDollars);
        }

        public BigDecimal FromEtherToUsd(BigDecimal totalInEther)
        {
            var getEtherPriceInUsd = GetEtherUnitPriceInUsd();
            return totalInEther * getEtherPriceInUsd;
        }

        public BigDecimal FromWeiToEther(BigInteger totalWeis)
        {
            return Web3.Convert.FromWei(totalWeis, EthUnit.Ether);
        }

        public double GetEtherUnitPriceInUsd()
        {
            return UsdExchangeRate;
        }

        public static async Task<Exchange> GetEtherExchangeRateInUsd()
        {
            WebClient webClient = new WebClient();
            string jsonResponse = await webClient.DownloadStringTaskAsync(Constants.EXCHANGE_API_URL);
            return JsonConvert.DeserializeObject<Exchange>(jsonResponse);
        }

        public async Task<BigInteger> GetRapidBalance(IAccount acc)
        {
            Web3 web3 = new Web3(acc, this.RpcServerUrl);

            HexBigInteger ret = await web3.Eth.GetBalance.SendRequestAsync(acc.Address);

            return ret.Value;
        }

        public async Task<BigInteger> GetBalanceInWei(IAccount account)
        {
            HexBigInteger balance = await Web3Instance(account).Eth.GetBalance.SendRequestAsync(account.Address);
            return balance.Value;
        }

        public async Task<BigDecimal> GetBalanceInEther(IAccount account)
        {
            return FromWeiToEther(await GetBalanceInWei(account));
        }

        public async Task<double> GetBalanceInUsd(IAccount account)
        {
            var getBalance = await GetBalanceInEther(account);
            var getEtherPriceInUsd = GetEtherUnitPriceInUsd();
            var balance = getBalance * getEtherPriceInUsd;
            return double.Parse(balance.ToString(), GetCultureInfo());
        }

        public async Task<BigInteger> EstimateOperatingCostInWei(IAccount account, string toAddress, BigInteger value)
        {
            return BigInteger.Add(value, await EstimateGas(account, toAddress, value));
        }

        public double EstimateOperatingCostInUsd(BigInteger value)
        {
            return double.Parse(FromEtherToUsd(FromWeiToEther(value)).ToString(), GetCultureInfo());
        }

        public async Task<BigInteger> EstimateGas(IAccount account, string toAddress, BigInteger value)
        {
            var callInput = new Nethereum.RPC.Eth.DTOs.CallInput(null, toAddress, new HexBigInteger(value));
            var gasPrice = await Web3Instance(account)
                                    .Eth
                                    .GasPrice
                                    .SendRequestAsync();
            var estimatedGas = (await Web3Instance(account)
                                         .Eth
                                         .Transactions
                                         .EstimateGas
                                         .SendRequestAsync(callInput)).Value;
            return BigInteger.Multiply(gasPrice, estimatedGas);
        }

        public async Task<TransactionResult> TransferTo(IAccount accountFrom, string accountTo, double totalInDollars)
        {
            var totalWei = FromUsdToWei(totalInDollars);
            var estimateGas = await EstimateGas(accountFrom, accountTo, totalWei);
            var balanceFrom = await GetBalanceInWei(accountFrom);
            var operatingCostWei = await EstimateOperatingCostInWei(accountFrom, accountTo, new HexBigInteger(totalWei));
            var operatingCostUsd = EstimateOperatingCostInUsd(operatingCostWei);
            var getEtherPriceInUsd = GetEtherUnitPriceInUsd();

            if (balanceFrom >= operatingCostWei)
            {
                var token = await Web3Instance(accountFrom)
                    .TransactionManager
                    .SendTransactionAsync(accountFrom.Address, accountTo, new HexBigInteger(totalWei));

                return new TransactionResult
                {
                    From = accountFrom.Address,
                    To = accountTo,
                    EstimatedGas = estimateGas,
                    OperatingCostWei = operatingCostWei,
                    OperatingCostUsd = operatingCostUsd,
                    TxHash = token,
                    TransactionStatus = TransactionStatus.Done,
                    TransactionDetails = TransactionDetails.Successfully,
                    Message = "rock in",
                    ValueWei = totalWei,
                    ValueUsd = totalInDollars,
                    EtherPriceInUsd = getEtherPriceInUsd
                };

            }
            else
            {
                return new TransactionResult
                {
                    From = accountFrom.Address,
                    To = accountTo,
                    EstimatedGas = estimateGas,
                    OperatingCostWei = operatingCostWei,
                    OperatingCostUsd = operatingCostUsd,
                    TxHash = null,
                    TransactionStatus = TransactionStatus.Fail,
                    TransactionDetails = TransactionDetails.InsufficientFunds,
                    Message = "too expensive for ya",
                    ValueWei = totalWei,
                    ValueUsd = totalInDollars,
                    EtherPriceInUsd = getEtherPriceInUsd
                };
            }
        }

        private Web3 Web3Instance(IAccount account)
        {
            return new Web3(account, url: RpcServerUrl);
        }

        private CultureInfo GetCultureInfo()
        {
            return new CultureInfo("en-US");
        }
    }

}