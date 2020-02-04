namespace AnubisWorks.GanacheSimulator.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;
    using AnubisWorks.GanacheSimulator.Application.Ethereum;
    using AnubisWorks.GanacheSimulator.Domain.Enums;
    using AnubisWorks.GanacheSimulator.Domain.Interfaces;
    using AnubisWorks.GanacheSimulator.Domain.Models.Ethereum;
    using Nethereum.Hex.HexTypes;
    using Nethereum.RPC.Accounts;
    using Nethereum.RPC.Eth.DTOs;
    using Nethereum.RPC.TransactionReceipts;
    using Nethereum.Util;
    using Nethereum.Web3;
    using Nethereum.Web3.Accounts;
    using Xunit;
    using Xunit.Abstractions;

    public class SimpleTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private EtherSystem ethSystem;
        private string serverUrl = "http://localhost:8545"; //Ganache Address
        //enter private very very rich owner
        private const string startPrivate = "0xc13b7680aff826143a233356e0e811df6b9435673975e523b24d5013ac29f0dd";
        private BigInteger howManyTransfer;

        private Account fromAccount;
        private Account toAccount;
        private Account startAccount;

        public SimpleTests(ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Setting things up!
        /// </summary>
        private void Setup()
        {
            ethSystem = new EtherSystem(12.25, this.serverUrl);

            this.fromAccount = ethSystem.GenerateNewAccount();

            this.toAccount = ethSystem.GenerateNewAccount();

            startAccount = (Account) this.ethSystem.LoadFromPrivateKey(startPrivate);
        }

        private Task<BigDecimal> FetchBallanceInEth(IAccount acc)
        {
            return ethSystem.GetBalanceInEther(acc);
        }

        /// <summary>
        /// Transferring funds to a newly created account
        /// </summary>
        /// <param name="from">Source Account</param>
        /// <param name="to">Destination Account</param>
        /// <returns></returns>
        private async Task<string> PerformInitials(Account from, Account to)
        {
            Web3 web3 = new Web3(from, this.serverUrl);

            ITransactionReceiptService transactionPolling = web3.TransactionManager.TransactionReceiptService;

            BigInteger currentBalance = await web3.Eth.GetBalance.SendRequestAsync(to.Address);

            TransactionReceipt hash = await transactionPolling.SendRequestAndWaitForReceiptAsync(() =>
                web3.TransactionManager.SendTransactionAsync(from.Address, to.Address, new HexBigInteger(howManyTransfer))
            );

            BigInteger status = BigInteger.Parse(hash.Status.ToString(), NumberStyles.AllowHexSpecifier);

            if (status != 1) throw new Exception("error concerning transfer status");

            BigInteger afterBalance = await web3.Eth.GetBalance.SendRequestAsync(to.Address);

            if(currentBalance != afterBalance) return hash.TransactionHash;

            return string.Empty;
        }

        /// <summary>
        /// Perform funds transfer in real money
        /// </summary>
        /// <param name="from">Source Account</param>
        /// <param name="to">Destination Account</param>
        /// <param name="currencyValue">Amount of Money</param>
        /// <returns></returns>
        private async Task<bool> PerformTransfer(IAccount from, IAccount to, double currencyValue)
        {
            TransactionResult txHash = await ethSystem.TransferTo(accountFrom: from, accountTo: to.Address, totalInDollars: currencyValue);

            if(txHash.TransactionStatus == TransactionStatus.Fail)
            {
                Console.WriteLine($"Transaction failed because of: {txHash.TransactionDetails}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if initial transfers succeeded
        /// </summary>
        [Fact]
        public void PerformInitialTransferChecking()
        {
            if(this.ethSystem == null) Setup();

            BigInteger bint = ethSystem.GetRapidBalance(this.startAccount).Result;

            this.howManyTransfer = bint / 2;

            if(bint == 0) throw new Exception("something wrong");

            BigInteger beforeInitials = ethSystem.GetRapidBalance(this.fromAccount).Result;

            if(!string.IsNullOrEmpty(startPrivate))
            {
                string tStatusZeroHash = PerformInitials(this.startAccount, this.fromAccount).Result;
            }

            BigInteger afterInitials = ethSystem.GetRapidBalance(this.fromAccount).Result;

            Assert.True(beforeInitials + howManyTransfer == afterInitials);
        }

        /// <summary>
        /// Check whether after transfer it has been validated and stored
        /// </summary>
        [Fact]
        public void PerformAfterTransferChecking()
        {
            if(this.ethSystem == null) Setup();

            if(ethSystem.GetRapidBalance(this.fromAccount).Result<=100) PerformInitialTransferChecking();

            BigInteger nfa = ethSystem.GetRapidBalance(this.fromAccount).Result;

            Assert.True(nfa == howManyTransfer);
        }

        /// <summary>
        /// Check whether after transfer it has been validated and stored. Ether based
        /// </summary>
        [Fact]
        public void PerformAfterTransferEthChecking()
        {
            if (this.ethSystem == null) Setup();

            if (ethSystem.GetRapidBalance(this.fromAccount).Result <= 100) PerformInitialTransferChecking();

            BigDecimal ether = ethSystem.GetBalanceInEther(this.fromAccount).Result;
            
            Assert.True(ether == Web3.Convert.FromWei(howManyTransfer, UnitConversion.EthUnit.Ether));
        }

        /// <summary>
        /// Check whether after transfer, propper values are set
        /// </summary>
        [Fact]
        public void PerformTransferChecking()
        {
            if(this.ethSystem == null) Setup();

            _testOutputHelper.WriteLine($"Source Account has got ETH = {ethSystem.GetRapidBalance(this.startAccount).Result} available.");

            BigDecimal beforeOps = FetchBallanceInEth(this.toAccount).Result;

            bool tStatus = PerformTransfer(this.startAccount, this.toAccount, 1).Result;

            if(!tStatus) throw new Exception("error occured during transfer");

            BigDecimal afterOps = FetchBallanceInEth(this.toAccount).Result;
            
            Assert.True(beforeOps<afterOps);
        }

        /// <summary>
        /// Account2Account Transfer checks
        /// </summary>
        [Fact]
        public void PerformAccount2AccountTransferChecking()
        {
            if (this.ethSystem == null) Setup();

            if(FetchBallanceInEth(this.fromAccount).Result == 0) PerformInitialTransferChecking();

            BigInteger checkB = ethSystem.GetRapidBalance(this.fromAccount).Result;

            _testOutputHelper.WriteLine($"Source Account has got ETH = {checkB} available.");

            BigDecimal beforeOps = FetchBallanceInEth(this.toAccount).Result;

            BigDecimal amnt = this.ethSystem.FromEtherToUsd(this.ethSystem.FromWeiToEther(checkB));

            double amount = double.Parse(amnt.Floor().ToString());

            bool tStatus = PerformTransfer(this.fromAccount, this.toAccount, amount).Result;

            if (!tStatus) throw new Exception("error occured during transfer");

            BigDecimal afterOps = FetchBallanceInEth(this.toAccount).Result;

            Assert.True(beforeOps < afterOps);
        }
    }
}