using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Mvc.Xss.HttpModules
{
    public class XssHeadersModule : IHttpModule
    {
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.PostRequestHandlerExecute += context_PostRequestHandlerExecute;
        }

        private void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            HttpContext context = app.Context;
            string contentType = app.Context.Response.ContentType;

            if (context.Request.HttpMethod == "GET" &&
                contentType.Equals("text/html") &&
                context.Response.StatusCode == 200 &&
                context.CurrentHandler != null)
            {
                var request = context.Request;
                String xssHeadersKey = "Content-Security-Policy";
                String xssHeadersValue = String.Empty;
                request.Headers.Remove(xssHeadersKey);
                var nonceCollection = context.Items[Constants.XSS_INLINE_KEY] as IList<String>;
                if (nonceCollection != null && nonceCollection.Any())
                {
                    xssHeadersValue = $"script-src {String.Join(" ", nonceCollection.Select(n=> $"'nonce-{n}'"))}";
                }

                var scriptWebCollection = context.Items[Constants.XSS_BLOCK_KEY] as IList<String>;
                if (scriptWebCollection != null && scriptWebCollection.Any())
                {
                    if (String.IsNullOrWhiteSpace(xssHeadersValue))
                    {
                        xssHeadersValue = $"script-src";
                    }
                    xssHeadersValue = $"{xssHeadersValue} {String.Join(" ", scriptWebCollection)}";
                }
                else{
                    xssHeadersValue = $"{xssHeadersValue} 'self'";
                }

                context.Response.Headers.Add(xssHeadersKey, xssHeadersValue);
            }

        }
    }
}
