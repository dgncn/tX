using Newtonsoft.Json;

namespace tX.Models
{
    public class TxModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Hash { get; set; }

        private string _timeStamp;
        public string Timestamp
        {
            get { return _timeStamp; }
            set
            {
                _timeStamp = value;
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
        private string _val;
        [JsonProperty("value")]
        public string Value
        {
            get
            {
                return _val;
            }

            set
            {
                var v = Convert.ToDecimal(value);
                _val = value;
                this._ethVal = (v / (decimal)Math.Pow(10, 18));
            }
        }
        //public string FunctionName { get; set; }

        [JsonProperty("functionName")]
        public string FunctionSignature { get; set; }
        public string FunctionName { get; set; }
    }
}
