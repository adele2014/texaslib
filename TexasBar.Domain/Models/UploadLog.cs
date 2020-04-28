using System;
using System.Collections.Generic;

namespace TexasBar.Domain.Models
{
    public partial class UploadLog
    {
        public int Id { get; set; }
        public string LogId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UploadedFileName { get; set; }
        public int? Size { get; set; }
        public string Status { get; set; }
        public string BookValue { get; set; }
        public int? Version { get; set; }
        public bool? IsCurrent { get; set; }
        public string Path { get; set; }
    }
}
