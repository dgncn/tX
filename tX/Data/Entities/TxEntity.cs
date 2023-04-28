using System.ComponentModel.DataAnnotations;

namespace tX.Data.Entities
{
    public class TxEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string Hash { get; set; }
        public string FunctionSignature
        {
            get
            {
                return this.FunctionSignature;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.FunctionName = value.Substring(0, value.IndexOf('('));
                }
            }
        }
        public string FunctionName { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Timestamp { get; set; }
        public decimal EthValue { get; set; }
        public DateTime CreatedDate
        {
            get; set;
        }
        public TxStatusEnum TxStatus { get; set; } = TxStatusEnum.UnProcessed;


    }

    public enum TxStatusEnum
    {
        UnProcessed = 1,
        Processing = 2,
        Processed = 3
    }
}
