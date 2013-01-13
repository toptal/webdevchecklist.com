using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Xml;

/// <summary>
/// Summary description for DataProvider
/// </summary>
public static class Checklist
{
    public static Dictionary<string, XmlDocument> Docs = new Dictionary<string, XmlDocument>();
    public const string Title = "Web Developer";
    public static string Folder = HostingEnvironment.MapPath("~/sections/");    

    static Checklist()
    {
        BuildCache(Folder);
    }
    
    public static XmlDocument GetXmlDocument(HttpRequestBase request)
    {
        string name = GetFileName(request.RawUrl);

        if (HttpRuntime.Cache["data"] == null)
        {
            BuildCache(Folder);

            HttpRuntime.Cache.Insert("data", "test", new CacheDependency(Folder));
        }

        var result = Docs.FirstOrDefault(d => d.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (result.Value != null)
        {
            return result.Value;
        }

        return Docs["index"]; ;
    }

    private static void BuildCache(string folder)
    {
        Docs.Clear();
        foreach (string file in Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            string key = file
                .Replace(folder, string.Empty)
                .Replace("\\", "/")
                .Replace(".xml", string.Empty);

            if (key.Contains("/") && key.EndsWith("index"))
            {
                key = key.Replace("/index", string.Empty);
            }

            Docs.Add(key, doc);
        }
    }

    public static string PageName(HttpRequestBase request)
    {
        string name = GetFileName(request.RawUrl);
        var result = Docs.FirstOrDefault(d => d.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (result.Value != null)
        {
            return result.Key;
        }
                
        return Title;
    }

    public static string Subtitle(HttpRequestBase request)
    {        
        if (PageName(request) != Title)
        {
            string clean = request.RawUrl.Trim('/');

            if (Docs.Any(d => d.Key.Equals(clean, StringComparison.OrdinalIgnoreCase)))
                return string.Empty;

            int index = clean.IndexOf('/', 1);
         
            if (index > -1)
            {
                return clean.Substring(index);
            }

            return string.Empty;
        }

        return request.RawUrl;
    }

    public static string BaseName(HttpRequestBase request)
    {
        string fullName = GetFileName(request.RawUrl);
        int index = fullName.IndexOf('/');

        if (index > -1)
        {
            return fullName.Substring(0, index);
        }

        return fullName;
    }

    static private string GetFileName(string path)
    {
        string clean = path.Trim('/');

        if (Docs.Any(d => d.Key.Equals(clean, StringComparison.OrdinalIgnoreCase)))
            return clean;

        int index = clean.IndexOf('/');

        if (index > -1)
        {
            clean = clean.Substring(0, index);
        }

        return clean;
    }
}