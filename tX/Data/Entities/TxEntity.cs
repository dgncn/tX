using System.ComponentModel.DataAnnotations;

namespace tX.Data.Entities
{
    public class TxEntity
    {
        [Key]
        public Guid Id { get;set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Timestamp { get; set; }
        public decimal EthValue { get; set; }
        public DateTime CreatedDate
        {
            get; set;
        }
    }
}
