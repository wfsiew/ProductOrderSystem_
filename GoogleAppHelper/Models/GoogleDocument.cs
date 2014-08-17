using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Google.GData.Documents;

namespace GoogleAppHelper.Models
{
    public class GoogleDocument
    {
        public string Title { get; set; }
        public string ResourceID { get; set; }
        public string AlternateUri { get; set; }
        public string DownloadUrl { get; set; }
        public DateTime Updated { get; set; }
        public DocumentEntry OriginalEntry { get; set; }
    }

    public enum DocumentShareRole
    {
        [Description("reader")]
        reader = 1,
        [Description("writer")]
        writer = 2
    }
}
