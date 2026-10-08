using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti.classes
{
    public class responseResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        
    }

    public class responseBody : responseResult
    {
        public responceKala dtos { get; set; }
    }

    public class responceKala
    {
        public int code { get; set; }
        public int price { get; set; }
        public int stock { get; set; }
        public string updated_at { get; set; }
    }

    public class responsetokem : responseResult
    {
        public Data data { get; set; }
    }

    public class Data 
    {
        public string token { get; set; }
        public Vendor vendor { get; set; }
        public string expires_at { get; set; }
    }


    public class Vendor 
    {
        public int id { get; set; }
        public string title { get; set; }
    }

}

