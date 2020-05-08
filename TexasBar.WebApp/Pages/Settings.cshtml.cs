using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TexasBar
{
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public string bucket { get; set; }

        private BackgroundTask backT = null;

        public SettingsModel()
        {
            backT = new BackgroundTask();
        }
        public void OnGet()
        {

        }

        public void OnPost()
        {

        }
    }
}