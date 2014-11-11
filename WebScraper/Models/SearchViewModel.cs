using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebScraper.Models
{
    public class SearchViewModel
    {
        [Required]
        public string Url { get; set; }

        [Required]
        public string Keywords { get; set; }
    }
}