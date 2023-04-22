using Newtonsoft.Json;

namespace tX.Models
{
    public class TxModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Timestamp
        {
            get { return Timestamp; }
            set
            {
                var longTime = Convert.ToInt64(value);
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(longTime);
                this.CreatedDate = dateTimeOffset.DateTime;
            }
        }
        public DateTime CreatedDate
        {
            get; set;
        }
        public decimal _ethVal { get; set; }
        [JsonProperty("value")]
        public string Value
        {
            get
            {
                return Value;
            }

            set
            {
                var v = Convert.ToDecimal(value);
                this._ethVal = (v / (decimal)Math.Pow(10, 18));
            }
        }
    }
}
