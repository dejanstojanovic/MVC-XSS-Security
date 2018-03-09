using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Yahoo.Yui.Compressor;

namespace Mvc.Xss.HtmlHelpers
{
    public static partial class Extensions
    {
        public static MvcHtmlString InlineScriptBlock(this HtmlHelper htmlHelper, String[] paths, bool minify = true)
        {
            var context = htmlHelper.ViewContext.RequestContext.HttpContext;
            var builder = new TagBuilder("script");
            String nonceId = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            builder.Attributes.Add("type", "text/javascript");
            builder.Attributes.Add("nonce", nonceId);
            var nonceCollection = context.Items[Constants.XSS_INLINE_KEY] as IList<String>;
            if (nonceCollection == null)
            {
                nonceCollection = new List<String>();
                context.Items[Constants.XSS_INLINE_KEY] = nonceCollection;
            }
            nonceCollection.Add(nonceId);
            return GetHtmlTag(htmlHelper, builder, paths, minify);
        }

        public static MvcHtmlString InlineScriptBlock(this HtmlHelper htmlHelper, String path, bool minify = true)
        {
            return InlineScriptBlock(htmlHelper, new String[] { path }, minify);
        }

        private static MvcHtmlString GetHtmlTag(HtmlHelper htmlHelper, TagBuilder builder, String[] paths, bool minify)
        {
            var context = htmlHelper.ViewContext.RequestContext.HttpContext;
            List<String> physicalPaths = new List<string>();
            String cacheKey = string.Join("-", paths.Select(p => String.Concat(p.ToLower(), "_", minify.ToString())));
            String cachedContent = context.Cache.Get(cacheKey) as String;
            if (cachedContent == null)
            {
                foreach (String path in paths)
                {
                    var physicalPath = context.Server.MapPath(path);
                    physicalPaths.Add(physicalPath);

                    if (File.Exists(physicalPath))
                    {
                        using (var fileStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var streamReader = new StreamReader(fileStream))
                            {
                                cachedContent = String.Concat(cachedContent, streamReader.ReadToEnd(), System.Environment.NewLine);
                            }
                        }
                    }
                }

                if (minify)
                {
                    switch (builder.TagName.ToLower().Trim())
                    {
                        case "style":
                            cachedContent = new CssCompressor().Compress(cachedContent);
                            break;
                        case "script":
                            cachedContent = new JavaScriptCompressor().Compress(cachedContent);
                            break;
                        default:
                            throw new ArgumentException(String.Format("Unknown resource type {0}", builder.TagName));
                    }
                }
                context.Cache.Insert(cacheKey, cachedContent, new System.Web.Caching.CacheDependency(physicalPaths.ToArray()), DateTime.MaxValue, TimeSpan.FromHours(1));
            }
            builder.InnerHtml = cachedContent;
            return MvcHtmlString.Create(builder.ToString());
        }

    }
}