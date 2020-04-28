using System;
using System.Collections.Generic;

namespace TexasBar.Domain.Models
{
    public partial class Chapters
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string ChapterNo { get; set; }
        public string BookCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public byte? Status { get; set; }
    }
}
