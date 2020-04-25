using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SmartBulbColor.BulbCommands
{
    public class BulbCommand
    {
        [JsonPropertyName(name: "id")]
        public int? Id { get; set; }
        [JsonPropertyName(name: "method")]
        public string Method { get; set; }
        [JsonPropertyName(name: "params")]
        public ArrayList Parameters { get; set; }

        public BulbCommand(string method, ArrayList parameters)
        {
            Method = method;
            Parameters = parameters;
        }
        public void CompleteWithId(int id)
        {
            Id = id;
        }
    }
}
