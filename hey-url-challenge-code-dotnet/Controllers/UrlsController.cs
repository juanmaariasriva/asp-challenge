using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using hey_url_challenge_code_dotnet.Models;
using hey_url_challenge_code_dotnet.Utils;
using hey_url_challenge_code_dotnet.ViewModels;
using HeyUrlChallengeCodeDotnet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shyjus.BrowserDetection;

namespace HeyUrlChallengeCodeDotnet.Controllers
{
    [Route("/")]
    public class UrlsController : Controller
    {
        private readonly ILogger<UrlsController> _logger;
        private static readonly Random getrandom = new Random();
        private readonly IBrowserDetector browserDetector;
        private readonly ApplicationContext _db;

        public UrlsController(ILogger<UrlsController> logger, IBrowserDetector browserDetector, ApplicationContext db)
        {
            this.browserDetector = browserDetector;
            this._db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel();
            model.Urls = _db.Urls;
            model.NewUrl = new();
            return View(model);
        }
        [HttpPost]
        public IActionResult Create(String url)
        {
            url = url.Trim();
            Uri uriResult;
            Boolean isValid = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
            if (isValid)
            {
                var aux = _db.Urls.Where(x=> x.OriginalUrl == url).FirstOrDefault();
                if (aux is null)
                {
                    var obj = new Url();
                    obj.ShortUrl = Util.getShortUrl(5);
                    obj.OriginalUrl = url;
                    obj.Count = 0;
                    obj.Created = DateTime.Now;
                    _db.Urls.Add(obj);
                    _db.SaveChanges();
                    TempData["Notice"] = "Was successfully created!!!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Notice"] = "The URL already have a short url : "+aux.ShortUrl;
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Notice"] = "The URL is invalid!!!";
                return RedirectToAction("Index");
            }
        }

        [Route("/{url}")]
        public IActionResult Visit(string url)
        {
            var obj = _db.Urls.Where(x => x.ShortUrl == url).FirstOrDefault();
            if(obj is null)
            {
                return new NotFoundResult();
            }
            else
            {
                obj.Count++; 
                _db.Urls.Update(obj);
                var record = new UrlRecord();
                record.url = obj.ShortUrl;
                record.browser = this.browserDetector.Browser.Name;
                record.platform = this.browserDetector.Browser.OS;
                record.Created = DateTime.Now;
                _db.UrlRecords.Add(record);
                _db.SaveChanges();
                return new RedirectResult(obj.OriginalUrl);
            }
        }
        //public IActionResult Visit(string url) => new OkObjectResult($"{url}, {this.browserDetector.Browser.OS}, {this.browserDetector.Browser.Name}");

        [Route("urls/{url}")]
        public IActionResult Show(string url) => View(new ShowViewModel
        {
            Url = _db.Urls.Where(x => x.ShortUrl == url).FirstOrDefault(),
            DailyClicks = _db.UrlRecords.Where(x => x.url == url && x.Created.Year == DateTime.Today.Year && x.Created.Month == DateTime.Today.Month).Select(x => x)
            .GroupBy(x => x.Created.Date).Select(a => new { Key = a.Key.ToString(), Count = a.Count()}).ToDictionary(k=> k.Key, v=> v.Count),
            BrowseClicks = _db.UrlRecords.Where(x => x.url == url).GroupBy(x => x.browser).Select( x => new {Key = x.Key,Value = x.Count()}).ToDictionary(k=> k.Key, v=>v.Value),
            PlatformClicks = _db.UrlRecords.Where(x => x.url == url).GroupBy(x => x.platform).Select(x => new { Key = x.Key, Value = x.Count() }).ToDictionary(k => k.Key, v => v.Value)
        });

        [Route("lastCreated")]
        [Produces("application/json")]
        public List<Url> GetLastUrls()
        {
            return _db.Urls.OrderByDescending(x => x.Created).Take(10).ToList();
        }
    }
}