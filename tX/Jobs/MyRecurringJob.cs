using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Nethereum.Contracts;
using Nethereum.Contracts.Services;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Refit;
using tX.Data;
using tX.Data.Entities;
using tX.ServiceApis;

namespace tX.Jobs
{
    public interface IMyRecurringJob
    {
        Task DoSomethingReentrant();

        /// <summary>
        /// dbdeki adresler arasında gezer. her adres için transactionları kaydeder.
        /// </summary>
        /// <returns></returns>
        Task CreateTransactions();
    }
    public class MyRecurringJob : IMyRecurringJob
    {
        private readonly TxContext _context;

        public MyRecurringJob(TxContext context)
        {
            _context = context;
        }
        public async Task CreateTransactions()
        {
            var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io");

            var addresses = await _context.Addresses.ToListAsync();
            var txs = await _context.Transactions.OrderByDescending(x => x.CreatedDate).ToListAsync();

            var newTxList = new List<TxEntity>();
            foreach (var address in addresses)
            {
                var ts = txs?.FirstOrDefault(x => x.From == address.Hash)?.CreatedDate;
                var model = await etherScanApi.GetNormalTransactionsByAddress(address.Hash, "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");

                foreach (var _tx in model.Result)
                {
                    if (ts.HasValue && !(_tx.CreatedDate > ts)) continue;

                    var newTransaction = new TxEntity()
                    {
                        EthValue = _tx._ethVal,
                        From = _tx.From,
                        Timestamp = _tx._timeStamp,
                        To = _tx.To,
                        CreatedDate = _tx.CreatedDate
                    };

                    newTxList.Add(newTransaction);
                }
                if (newTxList.Count > 0) Thread.Sleep(300);
            }

            await _context.Transactions.AddRangeAsync(newTxList);
            await _context.SaveChangesAsync();


            //GetAccountBalance().Wait();

        }

        public async Task DoSomethingReentrant()
        {
            //Console.WriteLine("IMyRecurringJob doing something");
            //var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io");
            //var model = await etherScanApi.GetNormalTransactionsByAddress("0xf4f45c30065f15305b4707b70a2da055ce58b7c1", "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");

            //foreach (var item in model.Result)
            //{
            //    Console.WriteLine($"{item._ethVal} - {item.CreatedDate}");
            //}

            GetAccountBalance().Wait();

        }

        static async Task GetAccountBalance()
        {



            var web3 = new Web3("https://mainnet.infura.io/v3/f3be6af8c5d7458d8a5d582b43cc4d54");



            //Getting current block number  
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            Console.WriteLine("Current BlockNumber is: " + blockNumber.Value);

            //Getting current block with transactions 
            var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(8257129));
            Console.WriteLine("Block number: " + block.Number.Value);
            Console.WriteLine("Block hash: " + block.BlockHash);
            Console.WriteLine("Block no of transactions:" + block.Transactions.Length);
            Console.WriteLine("Block transaction 0 From:" + block.Transactions[0].From);
            Console.WriteLine("Block transaction 0 To:" + block.Transactions[0].To);
            Console.WriteLine("Block transaction 0 Amount:" + block.Transactions[0].Value);
            Console.WriteLine("Block transaction 0 Hash:" + block.Transactions[0].TransactionHash);

            var transaction =
                await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(
                    "0x08153c0546e4f73b178edae90d1e30fb519c7c49ff0dcd00e4b0bcd74dab468c");
            Console.WriteLine("Transaction From:" + transaction.From);
            Console.WriteLine("Transaction To:" + transaction.To);
            Console.WriteLine("Transaction Amount:" + transaction.Value);
            Console.WriteLine("Transaction Hash1:" + transaction.TransactionHash);

            var transactionReceipt =
                await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(
                    "0x08153c0546e4f73b178edae90d1e30fb519c7c49ff0dcd00e4b0bcd74dab468c");
            Console.WriteLine("Transaction Hash:" + transactionReceipt.TransactionHash);
            Console.WriteLine("Transaction ContractAddress:" + transactionReceipt.Root);
            Console.WriteLine("TransactionReceipt Logs:" + transactionReceipt.Logs);











            var balance = await web3.Eth.GetBalance.SendRequestAsync("0xAf2358e98683265cBd3a48509123d390dDf54534");
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io");
            var abiModel = await etherScanApi.GetAbiByAddress(transaction.To, "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");


            var model = await etherScanApi.GetNormalTransactionsByAddress("0xf4f45c30065f15305b4707b70a2da055ce58b7c1", "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");

            //$"https://api.etherscan.io/api?module=contract&action=getabi&address={}&apikey=VY28RYSRRCW617QIM92BJTQ3WES87PGJSP";

            //var abi =
            //   @"[{""constant"":false,""inputs"":[{""name"":""a"",""type"":""uint256""},{""name"":""b"",""type"":""string""},{""name"":""c"",""type"":""uint[3]""} ],""name"":""test"",""outputs"":[{""name"":""d"",""type"":""uint256""}],""type"":""function""}]";

            //var ethApi = new EthApiContractService(null, null);

            var x = web3.Eth.GetContract(abiModel.Result, transaction.To);
            //x.GetFunction()
            //var contract = x.GetContract(abiModel.Result, "ContractAddress");

            //get the function by name
            var testFunction = x.GetFunction("swapExactETHForTokens");
            var array = new[] { 1, 2, 3 };

            var str = "hello";
            var data = testFunction.GetData(69, str, array);
            var decode = testFunction.DecodeInput(transaction.Input);

            //FunctionMessageExtensions.DecodeInput()

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");
        }
    }
}
