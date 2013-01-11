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

    private static string folder = HostingEnvironment.MapPath("~/app_data/");    

    static Checklist()
    {
        BuildCache(folder);
    }
    
    public static XmlDocument GetXmlDocument(HttpRequestBase request)
    {
        string name = GetFileName(request.RawUrl);

        if (HttpRuntime.Cache["data"] == null)
        {
            BuildCache(folder);

            HttpRuntime.Cache.Insert("data", "test", new CacheDependency(folder));
        }

        var result = Docs.FirstOrDefault(d => d.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (result.Value != null)
        {
            return result.Value;
        }

        return Docs["items"]; ;
    }

    private static void BuildCache(string folder)
    {
        Docs.Clear();
        foreach (string file in Directory.GetFiles(folder, "*.xml", SearchOption.TopDirectoryOnly))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            string key = Path.GetFileName(file).Replace(Path.GetExtension(file), string.Empty);
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
            int index = request.RawUrl.IndexOf('/', 1);
         
            if (index > -1)
            {
                return request.RawUrl.Substring(index);
            }

            return string.Empty;
        }

        return request.RawUrl;
    }

    static private string GetFileName(string path)
    {
        string clean = path.Trim('/');

        int index = clean.IndexOf('/');

        if (index > -1)
        {
            clean = clean.Substring(0, index);
        }

        return clean;
    }
}