using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TexasBar.Domain.Models;
using TexasBar.Persistence.Repo;
using TexasBar.Services;

namespace TexasBar
{
    public class ViewerModel : PageModel
    {
        private readonly IUnitOfWork uow = null;
        private readonly IRepository<UploadLog> repoUploadLog = null;

        public ViewerModel()
        {
            uow = new UnitOfWork();
            repoUploadLog = new Repository<UploadLog>(uow);
        }


        public void OnGet()
        {
            //BackgroundTask backT = new BackgroundTask();
            //backT.CreateBucketTask("online-manuals");
        }

        public JsonResult OnGetBooks()
        {
            var upl = repoUploadLog.AllEager(x=>x.Id>0);
           // return new JsonResult(new List<string> { "as", "df" });
            return new JsonResult(new { data = upl, RespCode = 0, RespMessage = "Success" });
        }

    }
}