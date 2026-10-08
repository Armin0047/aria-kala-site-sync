using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti.classes
{
    public class requestKala
    {
        public int code { get; set; }
        public int? price { get; set; }
        public int avacheck { get; set; }
        public int ava { get; set; }
        public string expire { get; set; }
    }

    public class requestoken
    {
        public string mobile { get; set; }
        public string api_key { get; set; }
    }
}
