namespace AnubisWorks.GanacheSimulator.SmartContracts
{
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using System.Runtime.CompilerServices;
	using System.Text;
	using System.Threading.Tasks;
	using AnubisWorks.GanacheSimulator.Domain.Models.Ethereum;
	using AnubisWorks.GanacheSimulator.Functions;
	using Nethereum.Contracts.ContractHandlers;
	using Nethereum.Contracts.CQS;
	using Nethereum.RPC.Accounts;
	using Nethereum.RPC.Eth.DTOs;
	using Nethereum.Web3;

    public class SmartContractInteraction
    {
        private readonly Web3 bcNode;
        private string contractAddress;
        private TransactionReceipt transactionReceipt;

        public SmartContractInteraction(Web3 node)
        {
            this.bcNode = node;

            if(!DeployContract().Result) throw new Exception("Init failed");
        }


        private async Task<bool> DeployContract()
        {
            try
            {
                var deploymentMessage = new StandardTokenDeployment {TotalSupply = 100000};

                IContractDeploymentTransactionHandler<StandardTokenDeployment> deploymentHandler = bcNode.Eth.GetContractDeploymentHandler<StandardTokenDeployment>();
                transactionReceipt = await deploymentHandler.SendRequestAndWaitForReceiptAsync(deploymentMessage);
                contractAddress = transactionReceipt.ContractAddress;

            }
            catch(Exception e)
            {
                Console.WriteLine(e);

                return false;
            }

            return true;
        }

        public async Task<BigInteger> QuerySingleContract(IAccount account)
        {
            BalanceOfFunction balanceOfFunctionMessage = new BalanceOfFunction() {Owner = account.Address,};

            IContractQueryHandler<BalanceOfFunction> balanceHandler = bcNode.Eth.GetContractQueryHandler<BalanceOfFunction>();
            BigInteger balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOfFunctionMessage);

            return balance;
        }

        public async Task<BigInteger> QueryChain(IAccount account)
        {
            BalanceOfFunction balanceOfFunctionMessage = new BalanceOfFunction() {Owner = account.Address,};

            IContractQueryHandler<BalanceOfFunction> balanceHandler = bcNode.Eth.GetContractQueryHandler<BalanceOfFunction>();
            BalanceOfOutputDTO balanceOutput = await balanceHandler.QueryDeserializingToObjectAsync<BalanceOfOutputDTO>(balanceOfFunctionMessage, contractAddress);

            return balanceOutput.Balance;
        }

        public async Task<BigInteger> QueryPReviousContractState(IAccount account)
        {
            BalanceOfFunction balanceOfFunctionMessage = new BalanceOfFunction() { Owner = account.Address, };

            IContractQueryHandler<BalanceOfFunction> balanceHandler = bcNode.Eth.GetContractQueryHandler<BalanceOfFunction>();
            BalanceOfOutputDTO balanceOutput = await balanceHandler.QueryDeserializingToObjectAsync<BalanceOfOutputDTO>(balanceOfFunctionMessage, contractAddress, new Nethereum.RPC.Eth.DTOs.BlockParameter(transactionReceipt.BlockNumber));

            return balanceOutput.Balance;
        }
    }
}