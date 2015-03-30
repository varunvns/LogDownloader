using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchPredicateSample.sitecore.admin.LogDownloader.Resources.Models
{
    public class LogFileModel
    {
        public string FileName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        //public DateTime ModifiedDateTime { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
    }
}