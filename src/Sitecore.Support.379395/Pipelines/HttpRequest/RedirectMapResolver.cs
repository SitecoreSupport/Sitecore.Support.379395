using System;
using System.IO;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Web;

namespace Sitecore.Support.XA.Feature.Redirects.Pipelines.HttpRequest
{
    public class RedirectMapResolver : Sitecore.XA.Feature.Redirects.Pipelines.HttpRequest.RedirectMapResolver
    {
        protected override bool IsFile(string filePath)
        {
            try
            {
                return string.IsNullOrEmpty(filePath) || WebUtil.IsExternalUrl(filePath) || File.Exists(HttpContext.Current.Server.MapPath(filePath));
            }
            catch (Exception error)
            {
                Log.Warn("Sitecore.Support.379395: " + error.Message, error, this);
                return false;
            }
        }
    }
}
