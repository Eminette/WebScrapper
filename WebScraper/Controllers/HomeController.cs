using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebScraper.Models;
using WebScraper.Services;

namespace WebScraper.Controllers
{
    public class HomeController : Controller
    {
        IUrlInvoker _invoker;
        public HomeController(IUrlInvoker invoker)
        {
            _invoker = invoker;
        }

        [HttpGet]
        public ViewResult Index()
        {
            return View(new SearchViewModel());
        }

        [HttpPost]
        public ViewResult Index(SearchViewModel model)
        {
            try
            {
                ValidateModel(model);
                string results = SearchUrlReturnsRanking(model.Url.Trim(), model.Keywords);
                ViewBag.Results = string.Format("This Url appears in rank {0} on google search", results);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message;
            }

            return View(model);
        }

        private void ValidateModel(SearchViewModel model)
        {
            if (string.IsNullOrEmpty(model.Keywords) || string.IsNullOrEmpty(model.Url))
                throw new Exception("Please enter an Url and at least one keyword.");
        }

        private const string SEARCHURL = "https://www.google.com.au/search?num=100&q=";
        private const string GOOGLESEARCHSEPARATOR = "class=\"r\"";

        private string SearchUrlReturnsRanking(string url, string keywords)
        {
            string searchUrl = SEARCHURL + keywords.Replace(" ", "+");
            string searchResultHtml = _invoker.GetPage(searchUrl);
            searchResultHtml = searchResultHtml.Substring(searchResultHtml.IndexOf("<h3") + 1);
            searchResultHtml = searchResultHtml.Substring(0, searchResultHtml.LastIndexOf("</body"));

            if (string.IsNullOrEmpty(searchResultHtml) || !searchResultHtml.Contains(GOOGLESEARCHSEPARATOR))
            {
                throw new Exception("The search returned no results.");
            }

            return GetRanking(url, searchResultHtml);
        }

        private string GetRanking(string url, string searchResultHtml)
        {
            string ranking = string.Empty;
            IList<string> searchResults = searchResultHtml.Split(new string[] { GOOGLESEARCHSEPARATOR }, StringSplitOptions.None);

            for (int i = 1; i < searchResults.Count; i++)
            {
                if (searchResults[i].Contains(url))
                {
                    if (!string.IsNullOrEmpty(ranking))
                        ranking += ", ";
                    ranking += i;
                }
            }

            if (string.IsNullOrEmpty(ranking))
                throw new Exception("The Url provided has not been found on the 100 first search results.");

            return ranking;
        }
    }
}

