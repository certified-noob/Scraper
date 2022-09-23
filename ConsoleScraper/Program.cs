using System;
using HtmlAgilityPack;

namespace ConsoleScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter URL:");
            var html = @"" + Console.ReadLine();
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(html);

            //var path = @"C:\Users\muham\Downloads\auction.html";
            //var doc = new HtmlAgilityPack.HtmlDocument();
            //doc.Load(path);

            var raw = doc.Text;
            raw = raw.Substring(raw.IndexOf("asset_list_content"));
            raw = "<div data-elm-id=\"" + raw;

            doc.LoadHtml(raw);

            var node = doc.DocumentNode.SelectSingleNode("//div//div");
            NewMethod(node);

            node = doc.DocumentNode.SelectSingleNode("//div");
            NewMethod(node);

        }
        private static  void NewMethod(HtmlNode node)
        {
            bool IsLink = false;
            foreach (var item in node.ChildNodes)
            {
                if (item.Name == "a")
                {
                    var attri = item.Attributes;
                    foreach (var a in attri)
                    {
                        if (a.Name == "href")
                        {
                            Console.WriteLine(a.Value);
                            IsLink = true;
                            break;
                        }
                    }
                }
                if (!IsLink)
                    continue;

                var value = item.InnerText.Replace("\n", "");
                if (!string.IsNullOrEmpty(value) && value != "\n")
                    Console.WriteLine(value);
            }
        }
    }
}
