using Refit;
using tX.ServiceApis;

namespace tX.Jobs
{
    public interface IMyRecurringJob
    {
        Task DoSomethingReentrant();
    }
    public class MyRecurringJob : IMyRecurringJob
    {
        public async Task DoSomethingReentrant()
        {
            Console.WriteLine("IMyRecurringJob doing something");
            var etherScanApi = RestService.For<IEtherScanApi>("https://api.etherscan.io");
            var model = await etherScanApi.GetNormalTransactionsByAddress("0xf4f45c30065f15305b4707b70a2da055ce58b7c1", "VY28RYSRRCW617QIM92BJTQ3WES87PGJSP");

            foreach (var item in model.Result)
            {
                Console.WriteLine(item._ethVal);
            }
        }
    }
}
