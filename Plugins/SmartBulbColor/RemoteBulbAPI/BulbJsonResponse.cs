using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SmartBulbColor.RemoteBulbAPI
{
    public class BulbJsonResponse
    {
        public int id { get; set; }
        public string [] result { get; set; }
    }
}
