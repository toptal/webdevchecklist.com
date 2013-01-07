<%@ WebHandler Language="C#" Class="GoogleAnalytics" %>

using System;
using System.Web;
using System.Net;

public class GoogleAnalytics : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/javascript";
        DateTime lastModified = DateTime.Now;

        using (WebClient client = new WebClient())
        {
            byte[] buffer = client.DownloadData("http://www.google-analytics.com/ga.js");
            context.Response.BinaryWrite(buffer);
            
            if (!string.IsNullOrEmpty(client.ResponseHeaders[HttpResponseHeader.LastModified]))
                DateTime.TryParse(client.ResponseHeaders[HttpResponseHeader.LastModified], out lastModified);
        }

        context.Response.Cache.SetValidUntilExpires(true);
        context.Response.Cache.SetExpires(DateTime.Now.AddDays(30));
        context.Response.Cache.SetCacheability(HttpCacheability.Public);
        context.Response.Cache.SetLastModified(lastModified);
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}