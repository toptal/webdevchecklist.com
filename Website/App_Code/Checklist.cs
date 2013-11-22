using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml;

public static class Checklist
{
    // Public
    public static Dictionary<string, Dictionary<string, XmlDocument>> Docs = new Dictionary<string, Dictionary<string, XmlDocument>>();
    public const string Title = "Web Developer Checklist";

    // Private
    private const string defaultSite = "webdevchecklist.com";
    private static string SitesFolder = HostingEnvironment.MapPath("~/sites/");

    static Checklist()
    {
        BuildCache();
    }

    private static void BuildCache()
    {
        Docs.Clear();

        foreach (string folder in Directory.GetDirectories(SitesFolder))
        {
            var dic = new Dictionary<string, XmlDocument>();

            foreach (string file in Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);

                string key = file
                    .Replace(folder, string.Empty)
                    .Replace("Sections", string.Empty)
                    .Replace("\\", "/")
                    .Replace(".xml", string.Empty)
                    .TrimStart('/');

                if (key.Contains("/") && key.EndsWith("index"))
                {
                    key = key.Replace("/index", string.Empty);
                }

                dic.Add(key, doc);
            }

            Docs.Add(Path.GetFileName(folder), dic);
        }
    }

    public static XmlDocument GetXmlDocument(HttpRequestBase request)
    {
        string site = GetSiteName(request);

        var section = Docs[site];
        var pageName = GetPageName(request);
        var result = section.Keys.SingleOrDefault(k => k.Equals(pageName, StringComparison.OrdinalIgnoreCase));

        if (result != null)
        {
            return section[result];
        }

        return Docs[site]["index"];
    }

    public static string GetSiteName(HttpRequestBase request)
    {
        return request.Url.Host.StartsWith("localhost") ? defaultSite : request.Url.Host;
    }

    public static string GetSiteSectionFolder(HttpRequestBase request)
    {
        string siteName = GetSiteName(request);

        return Path.Combine(SitesFolder, siteName, "Sections");
    }

    public static string GetPageTitle(HttpRequestBase request)
    {
        XmlDocument doc = GetXmlDocument(request);
        XmlAttribute attr = doc.SelectSingleNode("checklist").Attributes["name"];

        if (attr != null)
        {
            return attr.InnerText;
        }

        return Title;
    }

    public static string GetPageName(HttpRequestBase request)
    {
        string site = GetSiteName(request);

        if (Docs.ContainsKey(site))
        {
            var pair = Docs[site];
            var path = request.RawUrl.Trim('/');

            while (true)
            {
                string clean = string.IsNullOrEmpty(path) ? "index" : path;
                var result = pair.Keys.SingleOrDefault(k => k.Equals(clean, StringComparison.OrdinalIgnoreCase));

                if (result != null)
                    return result;

                int index = path.LastIndexOf('/');
                if (index == -1)
                    break;

                path = path.Substring(0, index);
            }
        }

        return Title;
    }

    public static string GetBaseName(HttpRequestBase request)
    {
        string clean = request.RawUrl.Trim('/');

        if (Docs.Any(d => d.Key.Equals(clean, StringComparison.OrdinalIgnoreCase)))
            return clean;

        int index = clean.IndexOf('/');

        if (index > -1)
            clean = clean.Substring(0, index);

        return clean;
    }
}