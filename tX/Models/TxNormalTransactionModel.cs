namespace tX.Models
{
    public class TxNormalTransactionModel
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<TxModel> Result { get; set; }
    }
}
