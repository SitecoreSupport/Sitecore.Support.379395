using System;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.XA.Foundation.Multisite.Extensions;
using Sitecore.XA.Foundation.TokenResolution.Extensions;

namespace Sitecore.XA.Foundation.Multisite.Pipelines.HttpRequest
{
    public class LocalizableUrlItemResolver : HttpRequestProcessor
    {
        private string CACHE_KEY = "LUIR_{0}_{1}";
        public int CacheExpiration { get; set; }
        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item != null || Context.Site == null || !Context.Site.IsSxaSite() || Context.Database == null || !string.IsNullOrEmpty(Context.Page.FilePath))
            {
                return;
            }
            var urlPart = Context.RawUrl.ToLower();
            var questionMarkIndex = urlPart.IndexOf('?');
            if (questionMarkIndex > 0)
            {
                urlPart = urlPart.Substring(0, questionMarkIndex);
            }
            var cacheKey = string.Format(CACHE_KEY, urlPart, Context.Database.Name);
            var cachedItemId = HttpRuntime.Cache.Get(cacheKey);
            if (cachedItemId != null)
            {
                Context.Item = Context.Database.GetItem((ID)cachedItemId, Context.Language);
            }
            else
            {
                if (Context.Site.VirtualFolder != "/")
                {
                    urlPart = urlPart.Replace(Context.Site.VirtualFolder.ToLower(), string.Empty);
                }
                var pathElements = urlPart.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var startPath = Sitecore.DependencyInjection.ServiceLocator.ServiceProvider.GetService<ISiteInfoResolver>().GetStartPath(Context.Site.SiteInfo);
                startPath = startPath.EscapePath(true);
                var query = new StringBuilder();
                query.Append(startPath);
                foreach (var pathElement in pathElements)
                {
                    var pathElem = pathElement;
                    query.Append(string.Format("/*[@{0}=\"{1}\" or @@Name=\"{1}\"]", Constants.LocalizedUrlPart, pathElem));
                }
                var items = Context.Database.SelectItems(query.ToString());
                if (items.Count() == 1)
                {
                    HttpRuntime.Cache.Insert(cacheKey, items[0].ID, null, DateTime.UtcNow.AddMinutes(CacheExpiration), System.Web.Caching.Cache.NoSlidingExpiration);
                    Context.Item = items[0];
                }
            }
        }
    }
}