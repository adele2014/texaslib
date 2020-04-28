using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TexasBar.Domain.Models;
using TexasBar.Persistence.Repo;
using TexasBar.Services;

namespace TexasBar
{
    [RequestSizeLimit(400000000)]
    public class UploadModel : PageModel
    {
        private IHostingEnvironment _environment;
        private readonly IUnitOfWork uow = null;
        private readonly IRepository<UploadLog> repoUploadLog = null;


        public UploadModel(IHostingEnvironment environment)
        {
            _environment = environment;
            uow = new UnitOfWork();
            repoUploadLog = new Repository<UploadLog>(uow);
        }
        public void OnGet()
        {

            var numbers = Enumerable.Range(1, 10); //Get numbers from 1 - 10
            Options = numbers.Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.ToString(),
                                      Text = "Version " + a.ToString()
                                  }).ToList();

        }

        [BindProperty]
        public IFormFile Upload { get; set; }
        [BindProperty]
        public List<SelectListItem> Options { get; set; }
        [BindProperty]
        public string BookValue { get; set; }
        [BindProperty]
        public string Version  { get; set; }

        
        public async Task OnPostAsync()
        {
            var file = Path.Combine(_environment.ContentRootPath, "UploadedFiles", Upload.FileName);
            using (var fileStream = new FileStream(file, FileMode.Create))
            {
               await Upload.CopyToAsync(fileStream);
                
            }
            BackgroundTask backT = new BackgroundTask();
            //   backT.CreateBucketTask("online-manuals");

            var folderPath = BookValue + "/" + Version + "/" + Upload.FileName;
            var res = backT.WriteToBucketTask(file, "online-manuals", folderPath);
        }

        public JsonResult OnGetBooks()
        {
            var upl = repoUploadLog.AllEager(x => x.Id > 0);
            // return new JsonResult(new List<string> { "as", "df" });
            return new JsonResult(new { data = upl, RespCode = 0, RespMessage = "Success" });
        }
    }
}