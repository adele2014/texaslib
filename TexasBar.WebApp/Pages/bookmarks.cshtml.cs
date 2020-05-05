using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TexasBar.Domain.Models;
using TexasBar.Persistence.Repo;

namespace TexasBar.WebApp
{
    public class bookmarksModel : PageModel
    {

        private readonly IUnitOfWork uow = null;
        private readonly IRepository<Bookmarks> repoBookmarks = null;

        public bookmarksModel()
        {
            uow = new UnitOfWork();
            repoBookmarks = new Repository<Bookmarks>(uow);
        }

        public void OnGet()
        {

        }

        public JsonResult OnGetBookmarks()
        {
            var upl = repoBookmarks.AllEager(x => x.Id > 0);
            // return new JsonResult(new List<string> { "as", "df" });
            return new JsonResult(new { data = upl, RespCode = 0, RespMessage = "Success" });
        }
    }
}