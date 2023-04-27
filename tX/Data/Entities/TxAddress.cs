using System.ComponentModel.DataAnnotations;

namespace tX.Data.Entities
{
    public class TxAddress
    {
        [Key]
        public Guid Id { get;set; }
        public string Hash { get; set; }
        public string Name { get; set; }
    }
}
