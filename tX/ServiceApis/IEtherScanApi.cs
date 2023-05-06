using Refit;
using tX.Models;

namespace tX.ServiceApis
{
    public interface IEtherScanApi
    {
        [Get("/api?module=account&action=txlist&address={address}&startblock=0&endblock=99999999&page=1&offset=10&sort=desc&apikey={_apiKey}")]
        Task<TxNormalTransactionModel> GetNormalTransactionsByAddress(string address, string _apiKey);

        [Get("/api?module=account&action=txlist&address={address}&startblock=0&endblock=99999999&page=1&offset=10&sort=desc&apikey={_apiKey}")]
        Task<TxNormalTransactionModel> GetTransactionByHash(string address, string _apiKey);

        [Get("/api?module=contract&action=getabi&address={transactionToAddress}&apikey={_apiKey}")]
        Task<TxAbiModel> GetAbiByAddress(string transactionToAddress, string _apiKey);
    }
}
