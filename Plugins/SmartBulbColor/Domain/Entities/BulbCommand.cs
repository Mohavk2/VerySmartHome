using System.Collections;
using System.Text.Json.Serialization;

namespace SmartBulbColor.Models
{
    public class BulbCommand
    {
        public CommandType Mode { get; private set; }

        [JsonPropertyName(name: "id")]
        public int? Id { get; set; }
        [JsonPropertyName(name: "method")]
        public string Method { get; set; }
        [JsonPropertyName(name: "params")]
        public ArrayList Parameters { get; set; }

        public BulbCommand(string method, ArrayList parameters, CommandType mode)
        {
            Mode = mode;
            Method = method;
            Parameters = parameters;
        }
        public void SetDeviceId(int id)
        {
            Id = id;
        }
    }
}
