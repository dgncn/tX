using Refit;
using tX.Models;

namespace tX.ServiceApis
{
    public interface IEtherScanApi
    {
        // //
        [Get("/api?module=account&action=txlist&address={address}&startblock=0&endblock=99999999&page=1&offset=10&sort=desc&apikey={_apiKey}")]
        Task<TxNormalTransactionModel> GetNormalTransactionsByAddress(string address,string _apiKey); //
    }
}
