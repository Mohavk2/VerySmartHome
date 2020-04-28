using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SmartBulbColor.RemoteBulbAPI
{
    public class BulbCommand
    {
        public ResponseMode Mode { get; private set; }

        [JsonPropertyName(name: "id")]
        public int? Id { get; set; }
        [JsonPropertyName(name: "method")]
        public string Method { get; set; }
        [JsonPropertyName(name: "params")]
        public ArrayList Parameters { get; set; }

        public BulbCommand(string method, ArrayList parameters, ResponseMode mode)
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
