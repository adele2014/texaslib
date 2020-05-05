using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedUtility;
using TexasBar.Services;

namespace TexasBar.WebApp
{
    public class HomeModel : PageModel
    {
        [BindProperty]
        public HtmlString bodyContent { get; set; }

        public string html { get; set; }

        public void OnGet(string bookValue, string version, string bookname)
        {
            string keyName = bookValue + "/" + version + "/public_html/index.html";
            BackgroundTask backT = new BackgroundTask();

            var answer = backT.ReadObjectDataAsyncTask("online-manuals", keyName).GetAwaiter().GetResult();
            if (answer.Text == "SUCCESS")
            {
                html = answer.Value;
                var parser = new HtmlAgilityPack.HtmlDocument();
                parser.LoadHtml(html);
                var contentClass = "content";
                foreach (HtmlNode node in parser.DocumentNode.SelectNodes("//div[@class='" + contentClass + "']"))
                {
                    html = node.InnerHtml;
                    break;
                }

                //modify links
                var chaptersHome = ModifyHomeChapter(answer.Value, bookValue, version);
                if (chaptersHome.Status == "SUCCESS")
                {
                  html=  html.Replace(chaptersHome.Value, chaptersHome.Text);
                }

                bodyContent = new HtmlString(html);
            }
        }
        public GenericResponse ModifyHomeChapter(string html, string bookValue, string version)
        {
            var res = new GenericResponse();
            try
            {

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var anchorHtml = htmlDoc.DocumentNode.SelectNodes(
                @"//div[@class='home_nav_links']/a").Where(x => x.Attributes["href"] != null)
                .FirstOrDefault();
            var aItemHtml = anchorHtml.OuterHtml;

            HtmlDocument newDoc = new HtmlDocument();
            newDoc.OptionOutputAsXml = true;
                newDoc.LoadHtml(aItemHtml);
                 var result = newDoc.DocumentNode.SelectSingleNode("//a");
                var href= result.GetAttributeValue("href", "#");

                var folderName = href.Split('/')[0];
                var chapterName = href.Split('/')[1].Replace(".htm", string.Empty);
                
                var newUrl = "../bookviewer?bookValue=" + bookValue + "&version=" + version + "&chapterFolder=" + folderName + "&firstChapterNo=" + chapterName;

                anchorHtml.SetAttributeValue("href", newUrl);

                //
                HtmlNode node = newDoc.DocumentNode;
                node.InnerHtml = anchorHtml.OuterHtml;
                res.Text = node.InnerHtml;
                res.Value = aItemHtml;
                res.Status = "SUCCESS";
            }
            catch (Exception ex)
            {
                res.Text = ex.Message;
                res.Status = "ERROR";
            }

            return res;
        }
    }
}