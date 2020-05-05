using System;
using System.Collections.Generic;

namespace TexasBar.Domain.Models
{
    public partial class Bookmarks
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Route { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string BookCode { get; set; }
        public string Version { get; set; }
        public string ChapterFolder { get; set; }
        public string Chapter { get; set; }
    }
}
