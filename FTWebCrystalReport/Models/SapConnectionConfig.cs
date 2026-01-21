using System;
using System.Collections.Generic;
using System.Linq;

namespace FTWebCrystalReport.Models
{
    public class SapConnectionConfig
    {
        public string SAPSERVER { get; set; }
        public string SAPDB { get; set; }
        public string SQLUSER { get; set; }
        public string SQLPASSWORD { get; set; }
    }
}