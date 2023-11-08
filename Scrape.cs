using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System.Net.Sockets;
using Excel = ClosedXML.Excel;

namespace _2023S2_SProj1_ThousandMissile

{
    internal class Scrape
    {
        IPage page;
        public List<string> URLS = new List<string> { @"https://www.woolworths.com.au/shop/browse/fruit-veg?pageNumber=1&sortBy=Name", @"https://www.woolworths.com.au/shop/browse/lunch-box?pageNumber=1&sortBy=Name",
            @"https://www.woolworths.com.au/shop/browse/poultry-meat-seafood?pageNumber=1&sortBy=Name",@"https://www.woolworths.com.au/shop/browse/bakery?pageNumber=1&sortBy=Name",@"https://www.woolworths.com.au/shop/browse/deli-chilled-meals?pageNumber=1&sortBy=Name",
        @"https://www.woolworths.com.au/shop/browse/dairy-eggs-fridge?pageNumber=1&sortBy=Name"};
        string ProductTileClass = ".product-tile-v2";
        string ProductTileName = ".product-title-link";
        string ProductTilePrice = ".product-tile-price";
        string ProductTilePricePrimary = ".primary";
        string NextButton = ".paging-next";
        int pageLoadTime = 2500;
        int PageCount = 1000;
        SortedDictionary<string,string> products = new SortedDictionary<string,string>();
        public bool debug = false;
        public bool enabled = true;
        Form1 form;
        int i;
        int lastStop = 0;
        public Scrape(Form1 form)
        {
            this.form = form;
        }

        public void RunNotAsync()
        {
            if (enabled != true) return;
            CreateLog("------------------NEW EXECUTION------------------");
            if (IsScrapeNeeded())
            {
                Run();
            }
        }

       private bool IsScrapeNeeded()
        {
            CreateLog("Checking if scrape is needed...");
            if (File.Exists("../../../LastRun.txt"))
            {
                string lastRun = File.ReadAllText("../../../LastRun.txt");
                if (lastRun != DateTime.Now.ToString().Split()[0])
                {
                    CreateLog("Scrape is needed");
                    return true;
                }
                CreateLog("Scrape is not needed");
                return false;
            }
            File.WriteAllText("../../../LastRun.txt", DateTime.Now.ToString().Split()[0]);
            return true;


        }
        private async void Run()
        {
            
            var playwright = await Playwright.CreateAsync();
            Microsoft.Playwright.IBrowser browser;
            try { browser = await playwright.Firefox.LaunchAsync(new() { Headless = true }); }
            catch
            {
                Microsoft.Playwright.Program.Main(new[] { "install" });
                browser = await playwright.Firefox.LaunchAsync(new() { Headless = true });
            }
            lastStop = int.Parse(File.ReadAllText("../../../LastStop.txt"));
            CreateLog(String.Format("LastRun {0}", lastStop));
            for (i = lastStop; i < URLS.Count; i++)
            {
                products.Clear();
                page = await browser.NewPageAsync();
                await page.GotoAsync(URLS[i]);
                await page.WaitForLoadStateAsync();
                CreateLog("Begin scrape");
                await ScrapeAllPages();
            }
           
            //Update last run txt
            File.WriteAllText("../../../LastRun.txt", DateTime.Now.ToString().Split()[0]);
            File.WriteAllText("../../../LastStop.txt", "0");

        }



        private async Task<int> ScrapeAllPages()
        {
            var workbook = new Excel.XLWorkbook("../../../data.xlsx");
            try {
                var sheet = workbook.Worksheets.Worksheet(String.Format("Sheet{0}", i + 1));
                CreateLog("Start compiling items");
                List<string> headers = sheet.Row(1).CellsUsed().Select(c => c.Value.ToString()).ToList();
                foreach (string name in headers)
                {
                    try { products.Add(name, "-1"); }
                    catch { }

                }
                CreateLog("Finished dictionary population");
            }
            catch
            {
                CreateLog("New Entry, no dictionary population");
            }
            CreateLog(String.Format("Section: {0}", URLS[i].Split("/").Last().Split("?")[0]));


            int pagecount = await GetPageNumber();
            PageCount = pagecount;
            pagecount -= 1;
            CreateLog(String.Format("PageCount Selected: {0}", PageCount));
            for (int i = 0; i < pagecount; i++)
            {
                await ScrapeOnePage();
                await NextPage();
                CreateLog(String.Format("Scraped page {0}", i + 1));
            }
            //After the last nexpage click, need to scrape the last page.
            await ScrapeOnePage();
            CreateLog(String.Format("Scraped page {0}", PageCount));
            CreateLog("Saving data...");
            Save(products);
            CreateLog("Done Scrape");
            return 0;
        }

        private async Task<int> ScrapeOnePage()
        {
            await page.WaitForTimeoutAsync(pageLoadTime);
            await DeleteBottomAdd();
            await GetAllProducts();
            //await page.WaitForTimeoutAsync(pageScrapeTime);
            return 0;
        }

        private async Task<int> DeleteBottomAdd()// There is a bottom bar of promotional products that are not wanted
        {
            await page.EvaluateAsync(@"
    var element = document.querySelector('.container-carousel');
            if (element)
                element.parentNode.removeChild(element);
            ");
            return 0;
        }


        private async Task<int> GetAllProducts()
        {
            //Get all product tiles
            var ProductElements = await page.QuerySelectorAllAsync(ProductTileClass);
            foreach (var product in ProductElements)
            {
                //get the name
                var productNameElement = await product.QuerySelectorAsync(ProductTileName);
                string productName = await productNameElement.InnerTextAsync();
                string productPrice;

                //get the price, if its out of stock set price as -1
                var productPriceWrapper = await product.QuerySelectorAsync(ProductTilePrice);
                if (productPriceWrapper == null)
                {
                    products[productName] = "-1";
                    continue;
                }
                var productPriceElement = await productPriceWrapper.QuerySelectorAsync(ProductTilePricePrimary);
                if (productPriceElement != null)
                {
                    //somtimes the price element has more info like /kg, just take the first index for the actual price
                    productPrice = await productPriceElement.InnerTextAsync();
                    productPrice = productPrice.Replace("$", "");
                    products[productName] = productPrice.Split("\n")[0];
                }
                else
                {
                    products[productName]="-1";
                }
            }
            return 0;
        }

        private async Task<int> GetPageNumber()
        {
            await page.WaitForTimeoutAsync(pageLoadTime);
            var pageSelector = await page.QuerySelectorAsync(".paging-section");
            var pageNumber = await pageSelector.QuerySelectorAllAsync(".paging-pageNumber");
            var pageCountElement = pageNumber.Last();
            string pageCountString = await pageCountElement.InnerTextAsync();
            int pagecount = int.Parse(pageCountString.Split("\n").Last());
            return pagecount;
        }

        private async Task<int> NextPage()
        {
            var button = await page.QuerySelectorAsync(NextButton);
            button.ClickAsync();
            return 0;
        }

        private void Save(SortedDictionary<string, string> data)
        {
            CreateLog(String.Format("{0} many products", data.Count));
            var workbook = new Excel.XLWorkbook("../../../data.xlsx");
            Excel.IXLWorksheet sheet = workbook.Worksheets.Worksheet("fruit-veg");
            try { sheet = workbook.Worksheets.Worksheet(URLS[i].Split("/").Last().Split("?")[0]); }
            catch
            {
                sheet = workbook.Worksheets.Add(URLS[i].Split("/").Last().Split("?")[0]);
            }
            
            List<string> names = data.Keys.ToList();
            int workingRow;
            //Check if empty
            if (sheet.Cell(1, 2).Value.ToString() == "")
            {
                //First run, fill headers, and set working row to below that
                sheet.Cell(1, 2).InsertData(names, true);
                workingRow = 2;
            }
            else
            {
                workingRow = sheet.LastRowUsed().RowNumber() + 1;//Else use next empty row(99% of the time)
            }
            CreateLog("Passed first run check");
            sheet.Cell(workingRow, 1).SetValue(DateTime.Now.ToString().Split()[0]);
            List<string> headers = sheet.Row(1).CellsUsed().Select(c => c.Value.ToString()).ToList();
            CreateLog(headers.ToString());
            //Since data is a dicionary based of headers, then if the length is the same, then they are both exactly the same.
            if (names.Count == headers.Count)
            {
                CreateLog("Using quick insert");
                sheet.Cell(workingRow, 2).InsertData(data.Values.ToList(), true);
            }
            //only other possiblity is a new item meaning it is larger than headers, so data entry with insert clause is needed
            //cannot be less as if scraped data is less then headers, then headers will just have alot of -1 values
            else
            if (headers.Count < names.Count)
            {
                CreateLog("Using 2nd if to save");
                for (int i = 1; i < names.Count+1; i++)
                {
                    
                    //If sorted price name matches
                    if (names[i - 1] == sheet.Cell(1, i + 1).Value.ToString())
                    {
                        sheet.Cell(workingRow, i + 1).Value = data[names[i-1]];
                    }
                    else//Assume we have a new item, inbetween existing ones
                    {
                        string header = sheet.Cell(1, i + 1).Value.ToString();
                        string header2 = sheet.Cell(1, i).Value.ToString();
                        string name = names[i - 1];

                        sheet.Column(i).InsertColumnsAfter(1);
                        CreateLog(String.Format("Item Inserted, i = {0}",i));
                        sheet.Cell(1, i+1).Value = names[i - 1];
                        sheet.Cell(workingRow, i+1).Value = data[names[i - 1]];


                        string after = sheet.Cell(1, i).Value.ToString();
                        string after2 = sheet.Cell(1, i+1).Value.ToString();
                    }
                }
            }

            CreateLog("Saving Now");
            workbook.Save();
            File.WriteAllText("../../../LastStop.txt", (i+1).ToString());
            // System.IO.File.WriteAllLines(String.Format("SavedLists{0}.txt", runCount), );
        }
           
        public async void CreateLog(string text)
        {
            if (debug == false) return;
            UdpClient sender = new UdpClient();
            byte[] data = Encoding.ASCII.GetBytes(text);
            sender.Send(data,data.Length,"localhost",1444);
        }
    }
}