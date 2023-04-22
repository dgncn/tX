using Newtonsoft.Json;

namespace tX.Models
{
    public class TxModel
    {
        public string From { get; set; }
        public string To { get; set; }

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
