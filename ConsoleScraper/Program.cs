using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HtmlAgilityPack;

namespace ConsoleScraper
{
    class Program
    {

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "Auction Properties";
        static readonly string SpreadsheetId = "1TFJevoB7yLawRHsZxe4zEcSzlrC64vfdAwNcw89_CD8";
        static readonly string sheet = "Sheet2";
        static SheetsService service;
        static void Main(string[] args)
        {
            GetDataOnline();
            Auth();
            CreateEntry();
            //ReadEntries();

        }

        private static void Auth()
        {
            GoogleCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                .CreateScoped(Scopes);
            }

            // Create Google Sheets API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        private static void GetDataOnline()
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

        static void ReadEntries()
        {
            var range = $"{sheet}!A:H";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(SpreadsheetId, range);

            var response = request.Execute();
            IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    // Print columns A to F, which correspond to indices 0 and 4.
                    Console.WriteLine("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7}", row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7]);
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }
        static void CreateEntry()
        {

            var range = $"{sheet}!A:H";
            var valueRange = new ValueRange();
            foreach (var property in Properties)
            {
                var oblist = new List<object>() { property.Address, property.City, property.State, property.ZipCode, property.Beds,property.Baths,property.SquareFeet, property.URL};
                valueRange.Values = new List<IList<object>> { oblist };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendReponse = appendRequest.Execute();
                Console.WriteLine(appendReponse.ToString());
            }
        }

        class Property
        {
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string Beds { get; set; }
            public string Baths { get; set; }
            public string SquareFeet { get; set; }
            public string URL { get; set; }
        }        

        static List<Property> Properties = new List<Property>();
        private static void NewMethod(HtmlNode node)
        {
            bool IsLink = false;
            foreach (var item in node.ChildNodes)
            {
                var property = new Property();
                if (item.Name == "a")
                {
                    var attri = item.Attributes;
                    foreach (var a in attri)
                    {
                        if (a.Name == "href")
                        {
                            Console.Write(a.Value);
                            property.URL = a.Value;
                            IsLink = true;
                            break;
                        }
                    }
                }
                if (!IsLink)
                    continue;


                var value = item.InnerText.Replace(System.Environment.NewLine, "");
                
                if(string.IsNullOrWhiteSpace(value))
                    continue;

                Regex trimmer = new Regex(@"\s\s+");

                value = trimmer.Replace(value, " ");
                var values = value.Split("Foreclosure Sale,");
                var addCityStateZip = values[0].Split(',');

                property.Address=addCityStateZip[0];
                var index = 1;
                var stateZip = addCityStateZip[index].Split(" ");
                
                if(stateZip[1].Length!=2){
                    index++;
                    stateZip = addCityStateZip[index].Split(" ");
                    property.State=stateZip[1];
                    property.ZipCode =stateZip[2];
                }else{
                    property.State=stateZip[1];
                    property.ZipCode =stateZip[2];
                }
                
                property.City=addCityStateZip[index+1].ToString().Substring(0,(addCityStateZip[2].Length - 7));

                var remaining = values[1].Split(" Est. Resale");
                var bedBathArea = remaining[0].Replace(" Online ","").Replace(" In Person ","").Trim().Split(" ");

                property.Beds = bedBathArea[0];
                property.Baths = bedBathArea[1];
                property.SquareFeet = bedBathArea[2];

                if (!string.IsNullOrEmpty(value) && value != "\n")
                    Console.Write(value);

                Properties.Add(property);  
            }
        }
    }
}
