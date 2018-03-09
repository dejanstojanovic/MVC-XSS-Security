using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mvc.Xss.HtmlHelpers
{
    public static partial class Extensions
    {
        public static MvcHtmlString ScriptBlock(this HtmlHelper htmlHelper, Uri url)
        {
            var context = htmlHelper.ViewContext.RequestContext.HttpContext;
            String urlToAdd = "'self'";
            var builder = new TagBuilder("script");
            builder.Attributes.Add("type", "text/javascript");
            builder.Attributes.Add("src", url.AbsoluteUri);
            var scriptWebCollection = context.Items[Constants.XSS_BLOCK_KEY] as IList<String>;
            if (scriptWebCollection == null)
            {
                scriptWebCollection = new List<String>();
                context.Items[Constants.XSS_BLOCK_KEY] = scriptWebCollection;
            }

            if (!url.Host.Equals(context.Request.Url.Host, StringComparison.InvariantCultureIgnoreCase) || url.Port != context.Request.Url.Port)
            {
                urlToAdd = $"{url.Scheme}://{url.Host}{(url.Port == 80 ? String.Empty : $":{url.Port}")}".ToLower();
            }
            if (!scriptWebCollection.Where(v => v.Equals(urlToAdd, StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                scriptWebCollection.Add(urlToAdd);
            }
            return MvcHtmlString.Create(builder.ToString());
        }

        public static MvcHtmlString ScriptBlock(this HtmlHelper htmlHelper, String url)
        {
            var context = htmlHelper.ViewContext.RequestContext.HttpContext;
            if (url.StartsWith("~") || url.StartsWith("/"))
            {
                url = VirtualPathUtility.ToAbsolute(url);
                url = $"{context.Request.Url.Scheme}://{context.Request.Url.Host}{(context.Request.Url.Port == 80 ? String.Empty : $":{context.Request.Url.Port}")}{url}";
            }
            return ScriptBlock(htmlHelper, new Uri(url));
        }
    }
}
