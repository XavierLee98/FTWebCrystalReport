using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FTWebCrystalReport.Models
{
    public class ReportRequest
    {
        public string DBName { get;set; }

        public ft_ORPT Report { get; set; }
    }
}