namespace tX.Jobs
{
    public interface IMyRecurringJob
    {
        void DoSomethingReentrant();
    }
    public class MyRecurringJob : IMyRecurringJob
    {
        public void DoSomethingReentrant()
        {
            Console.WriteLine("IMyRecurringJob doing something");
        }
    }
}
