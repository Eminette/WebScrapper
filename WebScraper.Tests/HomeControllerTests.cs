using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NBehave.Spec.MSTest;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using WebScraper.Controllers;
using WebScraper.Models;
using WebScraper.Services;

namespace WebScraper.Tests
{
    public class when_working_with_the_home_controller
    {
        protected HomeController _controller;
        protected Mock<IUrlInvoker> _invoker;
        public when_working_with_the_home_controller()
        {
            _invoker = new Mock<IUrlInvoker>();
            _invoker.Setup(i => i.GetPage(It.IsAny<string>())).Returns("");

            _controller = new HomeController(_invoker.Object);
        }
    }

    [TestClass]
    public class and_reaching_the_index : when_working_with_the_home_controller
    {
        [TestMethod]
        public void then_a_view_is_returned()
        {
            _controller.Index().ShouldBeInstanceOfType(typeof(ViewResult));
        }

        [TestMethod]
        public void then_the_model_is_a_search_view_model()
        {
            ((ViewResult)_controller.Index()).Model.ShouldBeInstanceOfType(typeof(SearchViewModel));

        }
    }

    [TestClass]
    public class and_making_a_search : when_working_with_the_home_controller
    {
        protected ActionResult Search(SearchViewModel model)
        {
            return _controller.Index(model);
        }

        [TestMethod]
        public void then_if_the_url_is_null_or_empty_an_error_is_returned()
        {
            string expected = "Please enter an Url and at least one keyword.";
            Search(new SearchViewModel { Keywords = "test" });
            _controller.TempData["ErrorMessage"].ShouldEqual(expected);
        }

        [TestMethod]
        public void then_if_the_keyword_is_null_or_empty_an_error_is_returned()
        {
            string expected = "Please enter an Url and at least one keyword.";
            Search(new SearchViewModel { Url = "test" });
            _controller.TempData["ErrorMessage"].ShouldEqual(expected);
        }

        [TestMethod]
        public void then_a_view_is_returned()
        {
            Search(new SearchViewModel { Keywords = "test", Url="test"}).ShouldBeInstanceOfType(typeof(ViewResult));
        }

        [TestMethod]
        public void then_the_model_is_a_search_view_model()
        {
            ((ViewResult)Search(new SearchViewModel { Keywords = "test", Url="test"}))
                .Model.ShouldBeInstanceOfType(typeof(SearchViewModel));
        }

        [TestMethod]
        public void then_if_a_result_is_found_the_viewbag_is_populated()
        {
            _invoker.Setup(i => i.GetPage(It.IsAny<string>())).Returns("<body><h3 class=\"r\"></h3>test</body>");
            Search(new SearchViewModel { Keywords = "test", Url="test"});
            var dynamic = _controller.ViewBag as DynamicObject;
            dynamic.GetDynamicMemberNames().Contains("Results").ShouldBeTrue();
        }

        [TestMethod]
        public void then_if_a_result_is_found_the_viewbag_contains_the_ranking()
        {
            _invoker.Setup(i => i.GetPage(It.IsAny<string>())).Returns("<body><h3 class=\"r\"></h3>test</body>");
            Search(new SearchViewModel { Keywords = "test", Url = "test" });
            string test = _controller.ViewBag.Results;
            test.ShouldEqual("This Url appears in rank 1 on google search");
        }

        [TestMethod]
        public void then_if_the_search_displays_no_results_an_error_message_is_returned()
        {
            string expected = "The search returned no results.";
            _invoker.Setup(i => i.GetPage(It.IsAny<string>())).Returns("<body></body>");
            Search(new SearchViewModel { Keywords = "test", Url = "test" });
            _controller.TempData["ErrorMessage"].ShouldEqual(expected);
        }

        [TestMethod]
        public void then_if_the_url_is_not_found_an_error_message_is_returned()
        {
            string expected = "The Url provided has not been found on the 100 first search results.";
            _invoker.Setup(i => i.GetPage(It.IsAny<string>())).Returns("<body><h3 class=\"r\"></h3></body>");
            Search(new SearchViewModel { Keywords = "test", Url = "test" });
            _controller.TempData["ErrorMessage"].ShouldEqual(expected);
        }
    }
}
