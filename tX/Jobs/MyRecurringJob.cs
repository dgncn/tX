using System.Security.Policy;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Nethereum.Contracts;
using Nethereum.Contracts.Services;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;
using Refit;
using tX.Data;
using tX.Data.Entities;
using tX.ServiceApis;

namespace tX.Jobs
{
    public interface IMyRecurringJob
    {
        Task UpdateTransactionInfos();

        /// <summary>
        /// dbdeki adresler arasında gezer. her adres için transactionları kaydeder.
        /// </summary>
        /// <returns></returns>
        Task CreateTransactions();
        Task GetTxHashInfos();
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
            var c = new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer()
            };
            var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io", c);

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
                        Timestamp = _tx.Timestamp,
                        To = _tx.To,
                        CreatedDate = _tx.CreatedDate,
                        Hash = _tx.Hash,
                        FunctionSignature = _tx.FunctionSignature
                    };

                    newTxList.Add(newTransaction);
                }
                if (newTxList.Count > 0) Thread.Sleep(300);
            }

            await _context.Transactions.AddRangeAsync(newTxList);
            await _context.SaveChangesAsync();
        }

        public async Task CreateTransaction(string txHash)
        {
            var c = new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer()
            };
            var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io", c);
            string address = await GetFromAddressOfTransaction(txHash);
            var txs = await _context.Transactions.FirstOrDefaultAsync(x => x.Hash == txHash);
            if (txs != null) return;

            var model = await etherScanApi.GetNormalTransactionsByAddress(address, "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");

            var _tx = model.Result.FirstOrDefault(x => x.Hash == txHash);
            var newTransaction = new TxEntity()
            {
                EthValue = _tx._ethVal,
                From = _tx.From,
                Timestamp = _tx.Timestamp,
                To = _tx.To,
                CreatedDate = _tx.CreatedDate,
                Hash = _tx.Hash,
                FunctionSignature = _tx.FunctionSignature
            };

            await _context.Transactions.AddAsync(newTransaction);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// transactioni tetikleyen kisi/gonderen
        /// </summary>
        /// <param name="txHash"></param>
        /// <returns></returns>
        private async Task<string> GetFromAddressOfTransaction(string txHash)
        {
            var tx = await GetTransactionOfTxHash(txHash);
            return tx.From;
        }

        private async Task<Transaction> GetTransactionOfTxHash(string txHash)
        {
            var web3 = new Web3("https://mainnet.infura.io/v3/f3be6af8c5d7458d8a5d582b43cc4d54");
            var transaction =
                await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(
                    txHash);

            return transaction;
        }

        private async Task<TxEntity> GetTransactionFromDbByTxHash(string txHash)
        {
            var item = await _context.Transactions.FirstOrDefaultAsync(x => x.Hash == txHash);
            return item;
        }

        public async Task UpdateTransactionInfos()
        {
            await UpdateTransactions();
        }

        public async Task UpdateTransactions()
        {
            var web3 = new Web3("https://mainnet.infura.io/v3/f3be6af8c5d7458d8a5d582b43cc4d54");

            #region commented examples
            //Getting current block number  
            //var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            //Console.WriteLine("Current BlockNumber is: " + blockNumber.Value);

            ////Getting current block with transactions 
            //var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(8257129));
            //Console.WriteLine("Block number: " + block.Number.Value);
            //Console.WriteLine("Block hash: " + block.BlockHash);
            //Console.WriteLine("Block no of transactions:" + block.Transactions.Length);
            //Console.WriteLine("Block transaction 0 From:" + block.Transactions[0].From);
            //Console.WriteLine("Block transaction 0 To:" + block.Transactions[0].To);
            //Console.WriteLine("Block transaction 0 Amount:" + block.Transactions[0].Value);
            //Console.WriteLine("Block transaction 0 Hash:" + block.Transactions[0].TransactionHash);

            #endregion

            var unProcessedlist = await _context.Transactions.Where(x => x.TxStatus == TxStatusEnum.UnProcessed).ToListAsync();

            var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io");

            foreach (var unprocessedItem in unProcessedlist)
            {
                unprocessedItem.TxStatus = TxStatusEnum.Processing;
                _context.Transactions.Update(unprocessedItem);
                await _context.SaveChangesAsync();

                await ProcessItem(unprocessedItem, web3, etherScanApi);

                unprocessedItem.TxStatus = TxStatusEnum.Processed;
                _context.Transactions.Update(unprocessedItem);
                await _context.SaveChangesAsync();

                Thread.Sleep(300);
            }

            #region commentedCodes

            //Console.WriteLine("Transaction From:" + transaction.From);
            //Console.WriteLine("Transaction To:" + transaction.To);
            //Console.WriteLine("Transaction Amount:" + transaction.Value);
            //Console.WriteLine("Transaction Hash1:" + transaction.TransactionHash);

            //var transactionReceipt =
            //    await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(
            //        "0x08153c0546e4f73b178edae90d1e30fb519c7c49ff0dcd00e4b0bcd74dab468c");
            //Console.WriteLine("Transaction Hash:" + transactionReceipt.TransactionHash);
            //Console.WriteLine("Transaction ContractAddress:" + transactionReceipt.Root);
            //Console.WriteLine("TransactionReceipt Logs:" + transactionReceipt.Logs);

            //var balance = await web3.Eth.GetBalance.SendRequestAsync("0xAf2358e98683265cBd3a48509123d390dDf54534");
            //Console.WriteLine($"Balance in Wei: {balance.Value}");


            #endregion
        }

        private async Task ProcessItem(TxEntity unprocessedItem, Web3 web3, IEtherScanApi etherScanApi)
        {
            var transaction =
                await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(
                    unprocessedItem.Hash);


            var abiModel = await etherScanApi.GetAbiByAddress(transaction.To, "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");

            var contract = web3.Eth.GetContract(abiModel.Result, transaction.To);
            var testFunction = contract.GetFunction(unprocessedItem.FunctionName);

            var decode = testFunction.DecodeInput(transaction.Input);
            int x = 4;
            #region commented codes
            //"0x08153c0546e4f73b178edae90d1e30fb519c7c49ff0dcd00e4b0bcd74dab468c");

            //var model = await etherScanApi.GetNormalTransactionsByAddress("0xf4f45c30065f15305b4707b70a2da055ce58b7c1", "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");

            //$"https://api.etherscan.io/api?module=contract&action=getabi&address={}&apikey=VY28RYSRRCW617QIM92BJTQ3WES87PGJSP";

            //var abi =
            //   @"[{""constant"":false,""inputs"":[{""name"":""a"",""type"":""uint256""},{""name"":""b"",""type"":""string""},{""name"":""c"",""type"":""uint[3]""} ],""name"":""test"",""outputs"":[{""name"":""d"",""type"":""uint256""}],""type"":""function""}]";

            //var ethApi = new EthApiContractService(null, null);

            //x.GetFunction()
            //var contract = x.GetContract(abiModel.Result, "ContractAddress");

            //get the function by name
            //var testFunction = contract.GetFunction("swapExactETHForTokens");

            //var data = testFunction.GetData(69, str, array);
            //FunctionMessageExtensions.DecodeInput()

            //var etherAmount = Web3.Convert.FromWei(balance.Value);
            //Console.WriteLine($"Balance in Ether: {etherAmount}");
            #endregion
        }

        public async Task GetTxHashInfos()
        {
            string txHash = "0xac9d15f1642946eae26aae251289c97df88731ee1782e321b73c21c670ba49ee";
            await CreateTransaction(txHash);
            var model = await GetTransactionFromDbByTxHash(txHash);
            var web3 = new Web3("https://mainnet.infura.io/v3/f3be6af8c5d7458d8a5d582b43cc4d54");
            var transaction =
                await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(
                    txHash);

            var c = new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer()
            };
            var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io", c);
            var abiModel = await etherScanApi.GetAbiByAddress(transaction.To, "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");
            var contract = web3.Eth.GetContract(abiModel.Result, transaction.To);
            var testFunction = contract.GetFunction(model.FunctionName);
            var decode = testFunction.DecodeInput(transaction.Input);
            int x = 3;
        }
    }
}