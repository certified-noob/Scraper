using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace Scraper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();




        }

        private void button1_Click(object sender, EventArgs e)
        {

            var html = @"" + textBox1.Text;

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
       
        private void NewMethod(HtmlNode node)
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
                            richTextBox1.Text += a.Value;
                            IsLink = true;
                            break;
                        }
                    }
                }
                if (!IsLink)
                    continue;

                var value = item.InnerText.Replace("\n","");
                if (!string.IsNullOrEmpty(value) && value != "\n")
                    richTextBox1.Text += value;
            }
        }
    }
}
