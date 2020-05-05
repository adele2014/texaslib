using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TexasBar.Domain.Models;
using TexasBar.Persistence.Repo;
using TexasBar.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using SharedUtility;

namespace TexasBar.Razor
{
    public class indexModel : PageModel
    {
        private readonly IUnitOfWork uow = null;
        private readonly IRepository<Chapters> repoChapters = null;
        private readonly IRepository<Bookmarks> repoBookmarks = null;
        private BackgroundTask backT = null;

        public indexModel()
        {
            uow = new UnitOfWork();
            repoChapters = new Repository<Chapters>(uow);
            repoBookmarks = new Repository<Bookmarks>(uow);
            backT = new BackgroundTask();
        }

        [BindProperty]
        public List<Chapters> chapters { get; set; }

        [BindProperty]
        public string Version { get; set; }

        public string html { get; set; }
        public string subMenu { get; set; }
        public string anchorMenu { get; set; }

        [BindProperty]
        public HtmlString htmlString { get; set; }

        [BindProperty]
        public HtmlString subMenuHtmlString { get; set; }

        [BindProperty]
        public HtmlString anchorMenuHtmlString { get; set; }

        [BindProperty]
        public string chapterNo { get; set; }

        [BindProperty]
        public HtmlString chaptersTempString { get; set; }


        public void OnGet(string bookValue, string version, string chapterFolder, string firstChapterNo)
        {
            //var bookValue = RouteData.Values["bookValue"].ToString();
            //var version = RouteData.Values["version"].ToString();
            //var chapterFolder = RouteData.Values["bookValue"].ToString();
            //var firstChapterNo= RouteData.Values["bookValue"].ToString();

            if (bookValue == "tps_web.zip")
            {


            }
            //call sqs 

            // backT.ReceiveSQSMesaageTask("ChaptersInQueue", bookValue);

            chapters = repoChapters.WhereOne(x => x.BookCode == bookValue);

            // var endpoint = "http://online-manuals.s3-website.us-east-2.amazonaws.com/";
            // string url = endpoint + bookValue + "/" + version + "/public_html/" + firstChapter + "_" + firstChapterName + "/Chapter_" + firstChapter + ".htm";
            //  string url = endpoint + bookValue + "/" + version + "/public_html/index.html";
            var keyName = bookValue + "/" + version + "/public_html/" + chapterFolder + "/" + firstChapterNo + ".htm";

            var answer = backT.ReadObjectDataAsyncTask("online-manuals", keyName).GetAwaiter().GetResult();
            if (answer.Text == "SUCCESS")
            {
                html = answer.Value;
                if (firstChapterNo.StartsWith("Form"))
                {
                    //parse for download link
                    html = DownloadString(html, bookValue, version, chapterFolder);

                }
                var parser = new HtmlAgilityPack.HtmlDocument();
                parser.LoadHtml(html);

                //var dbody = parser.GetElementbyId("body").InnerHtml;
                var containerClass = "chapter_content";
                var subMenuClass = "form_nav";
                var anchorMenuClass = "section_nav";
                var chapterNoClass = "chapter_number";



                foreach (HtmlNode node in parser.DocumentNode.SelectNodes("//div[@class='" + containerClass + "']"))
                {
                    html = node.InnerHtml;
                    break;
                }

                //get sub menus
                foreach (HtmlNode node in parser.DocumentNode.SelectNodes("//ul[@class='" + subMenuClass + "']"))
                {
                    subMenu = node.InnerHtml;
                    break;
                }

                //get anchor menus
                foreach (HtmlNode node in parser.DocumentNode.SelectNodes("//ul[@class='" + anchorMenuClass + "']"))
                {
                    anchorMenu = ListOfAnchorTags(node.InnerHtml);
                    break;
                }

                //get chapters ul
                foreach (HtmlNode node in parser.DocumentNode.SelectNodes("//p[@class='" + chapterNoClass + "']"))
                {
                    chapterNo = node.InnerText;
                    break;
                }

                // get chapters
                var chaptersTemp = ListOfChapters(answer.Value, bookValue, version);

                var formsTemp = ListOfForms(answer.Value, bookValue, version);



                htmlString = new HtmlString(html);
                subMenuHtmlString = new HtmlString(formsTemp);
                anchorMenuHtmlString = new HtmlString(anchorMenu);
                chaptersTempString = new HtmlString(chaptersTemp);
            }



        }

        string ListOfChapters(string html, string bookValue, string version)
        {

            string res = null;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var regs = htmlDoc.DocumentNode.SelectSingleNode(@"//ul[@class='chapter_group_nav']");

            var descendant = regs.Descendants()
                .Where(x => x.Name == "li" && x.Attributes["class"].Value == "current")
                .Select(x => x.OuterHtml);

            IEnumerable<string> listItemHtml = htmlDoc.DocumentNode.SelectNodes(
                @"//ul[@class='chapter_group_nav']/li[@class='current']/ul/li")
                .Select(li => li.OuterHtml);

            HtmlDocument newDoc = new HtmlDocument();
            foreach (string item in listItemHtml)
            {
                // res += item;
                newDoc.OptionOutputAsXml = true;
                newDoc.LoadHtml(item);
                var result = newDoc.DocumentNode
               .Descendants("a")
               .Where(x => x.Attributes["href"] != null).Select(x => x.Attributes["href"].Value).FirstOrDefault();

                var folderName = result.Split('/')[1];
                var chapterName = result.Split('/')[2].Replace(".htm", string.Empty);
                //  var newUrl = "../bookviewer/" + bookValue + "/" + version + "/public_html/" + folderName + "/" + chapterName;
                var newUrl = "../bookviewer?bookValue=" + bookValue + "&version=" + version + "&chapterFolder=" + folderName + "&firstChapterNo=" + chapterName;

                var result1 = newDoc.DocumentNode
              .Descendants("a")
              .Where(x => x.Attributes["href"] != null).FirstOrDefault();
                result1.SetAttributeValue("href", newUrl);

                //
                HtmlNode node = newDoc.DocumentNode.SelectSingleNode("//li");
                node.InnerHtml = result1.OuterHtml;
                res += node.OuterHtml;
            }

            return res;
        }

        string ListOfForms(string html, string bookValue, string version)
        {

            string res = null;
            HtmlDocument htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(html);

            var regs = htmlDoc.DocumentNode.SelectSingleNode(@"//ul[@class='form_nav']");

            var descendant = regs.Descendants()
                .Where(x => x.Name == "li")
                .Select(x => x.OuterHtml);

            IEnumerable<string> listItemHtml = htmlDoc.DocumentNode.SelectNodes(
                @"//ul[@class='form_nav']/li")
                .Select(li => li.OuterHtml);

            HtmlDocument newDoc = new HtmlDocument();
            foreach (string item in listItemHtml)
            {
                // res += item;
                newDoc.OptionOutputAsXml = true;
                newDoc.LoadHtml(item);
                var result = newDoc.DocumentNode
               .Descendants("a")
               .Where(x => x.Attributes["href"] != null).Select(x => x.Attributes["href"].Value).FirstOrDefault();

                var folderName = result.Split('/')[1];
                var chapterName = result.Split('/')[2].Replace(".htm", string.Empty);
                //  var newUrl = "../bookviewer/" + bookValue + "/" + version + "/public_html/" + folderName + "/" + chapterName;
                var newUrl = "../bookviewer?bookValue=" + bookValue + "&version=" + version + "&chapterFolder=" + folderName + "&firstChapterNo=" + chapterName;

                var result1 = newDoc.DocumentNode
              .Descendants("a")
              .Where(x => x.Attributes["href"] != null).FirstOrDefault();
                result1.SetAttributeValue("href", newUrl);

                //
                HtmlNode node = newDoc.DocumentNode.SelectSingleNode("//li");
                node.InnerHtml = result1.OuterHtml;
                res += node.OuterHtml;
            }

            return res;
        }

        string ListOfAnchorTags(string html)
        {

            string res = null;
            HtmlDocument htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(html);

            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
            {

                string hrefValue = link.GetAttributeValue("href", string.Empty);
                var newLink = hrefValue.Contains("#") ? hrefValue.Split('#')[1] : hrefValue;

                link.SetAttributeValue("href", "#" + newLink);


                res += "<li>" + link.OuterHtml + "</li>";
            }

            return res;
        }

        string DownloadString(string html, string bookValue, string version, string chapterFolder)
        {

            GenericResponse res = new GenericResponse();
            var result = "";
            try
            {
                HtmlDocument htmlDoc = new HtmlDocument();

                htmlDoc.LoadHtml(html);

                var regs = htmlDoc.DocumentNode.SelectSingleNode(@"//div[@class='srcDocs']");
                var href = regs.Descendants().Where(x => x.Name == "a");


                var downloadFileName = href.Select(x => x.Attributes["href"]).FirstOrDefault().Value;
                var keyName = bookValue + "/" + version + "/public_html/" + chapterFolder + "/" + downloadFileName;
                var newUrl = "../bookviewer?handler=PDF&keyName=" + keyName;
                //replace the url
                var dAnchor = href.FirstOrDefault();
                res.Value = dAnchor.OuterHtml;
                dAnchor.SetAttributeValue("href", newUrl);
                res.Text = dAnchor.OuterHtml;
                res.Status = "SUCCESS";
                return result = html.Replace(res.Value, res.Text);
            }
            catch (Exception ex)
            {
                res.Status = "ERROR";
                res.Value = ex.Message;

            }

            return result;
        }

        public RedirectResult OnGetPDF(string keyName)
        {
            string bucketName = "online-manuals";

            var sw = backT.GetPreSignUrlTask(bucketName, keyName);

            return Redirect(sw.Value);
        }


        public JsonResult OnPostBookmarks(string version, string bookValue, string chapterFolder,string firstChapterNo, string title)
        {
            
            try
            {
                var bookmark = new Bookmarks();
                bookmark.BookCode = bookValue;
                bookmark.ChapterFolder = chapterFolder;
                bookmark.Title = title;
                bookmark.Chapter = firstChapterNo;
                bookmark.Version = version;
                repoBookmarks.Insert(bookmark);

                uow.Save("david");
                return new JsonResult(new { data = "Page is bookmarked", RespCode = 0, RespMessage = "Success" });



            }
            catch (Exception ex)
            {

                return new JsonResult(new { data = ex.Message, RespCode = -1, RespMessage = "ERROR" });
            }

        }
    }

   
}