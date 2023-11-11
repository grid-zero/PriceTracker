using Excel = ClosedXML.Excel;
namespace _2023S2_SProj1_ThousandMissile
{
    public struct productRefrence
    {
        public float priceChange;
        public string productName;
        public int index;
        public productRefrence(int index, string name, float priceChange)
        {
            this.index = index;
            this.priceChange = priceChange;
            this.productName = name;
        }
    }

    public partial class Form1 : Form  
    {
       
        internal FlowLayoutPanel MainDiv;
        internal InfoDisplay infoDisplay;
        internal FlowLayoutPanel SideBarDiv;
        internal FlowLayoutPanel ProductSideBar;
        internal TextBox SearchBar;
        internal PageSelector pageSelector;
        string Data = "data.xlsx";
        internal string[] fav;
        internal List<ProductSource> Products = new List<ProductSource>();
        internal List<productRefrence> SearchedItems = new List<productRefrence>();
        internal List<productRefrence> Favourites = new List<productRefrence>();
        internal List<productRefrence> byPriceChange = new List<productRefrence>();
        internal int page = 0;
        internal int ListUsed = 1;
        internal Product Selected;
        Scrape scraper;
        int maxlen = 0;

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            scraper  = new Scrape(this);
            scraper.debug = true;
            scraper.enabled = true;
            scraper.RunNotAsync();

            this.Resize += FormResize;
            LoadFavouritesFromFile();
            LoadProductsFromFile();

            InitMainDiv();
            InitSideBarDiv();

            infoDisplay = new InfoDisplay(this);
            MainDiv.Controls.Add(SideBarDiv);
            MainDiv.Controls.Add(infoDisplay);

            this.Controls.Add(MainDiv);
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(247, 246, 242);
            this.FormClosed += Form1_FormClosing;
            
        }

        private void FormResize(object? sender, EventArgs e)
        {
            //MainDiv
            MainDiv.Width = this.ClientSize.Width;
            MainDiv.Height = this.ClientSize.Height;
            int percent20 = MainDiv.Width / 5;

            //SideBarDiv
            SideBarDiv.Width = percent20;
            SideBarDiv.Height = MainDiv.Height;
            //SearchBar
            SearchBar.Width = percent20 - SearchBar.Margin.Right - SearchBar.Margin.Left;
            //ProductSideBar
            ProductSideBar.Width = percent20;
            ProductSideBar.Height = MainDiv.Height - SideBarDiv.Controls[0].Height - SideBarDiv.Controls[1].Height - 50;



            //To much lag
           /* for(int i = 0; i < ProductSideBar.Controls.Count; i += 2)
            {
                ((Product)ProductSideBar.Controls[i]).Name.AutoSize = false;
                ((Product)ProductSideBar.Controls[i]).Name.MaximumSize = new Size(this.ClientSize.Width / 5 - 15, 10000000);
                ((Product)ProductSideBar.Controls[i]).Name.AutoSize = true;

            }*/

        }

        //------------Inits-----------------
        private void InitMainDiv()
        {
            MainDiv = new FlowLayoutPanel();
            MainDiv.FlowDirection = FlowDirection.LeftToRight;
            MainDiv.Width = this.ClientSize.Width;
            MainDiv.Height = this.ClientSize.Height;
        }

        private void InitSideBarDiv()
        {

            SideBarDiv = new FlowLayoutPanel();
            SideBarDiv.FlowDirection = FlowDirection.TopDown;
            SideBarDiv.Height = MainDiv.Height;
            SideBarDiv.Width = MainDiv.Width/5;
            SideBarDiv.AutoScroll = false;
            SideBarDiv.WrapContents = false;
            SideBarDiv.Margin = new Padding(0);

            InitSearchBar();
            SideBarDiv.Controls.Add(SearchBar);
            pageSelector = new PageSelector(this);
            SideBarDiv.Controls.Add(pageSelector);
            SideBarDiv.Controls.Add(AddSidePanelDivider());
            InitProductSideBar();
            SideBarDiv.Controls.Add(ProductSideBar);


            ProductSideBar.VerticalScroll.Maximum = 10;
            ProductSideBar.VerticalScroll.LargeChange = 10;
        }

        private void InitSearchBar()
        {
            SearchBar = new TextBox();
            SearchBar.Margin = new Padding(12, 10, 18, 10);
            SearchBar.Width = MainDiv.Width/5 - SearchBar.Margin.Right - SearchBar.Margin.Left;
            SearchBar.KeyUp += SearchProducts;
            SearchBar.Height = 100;
            SearchBar.PlaceholderText = "Search For Items Here.";
        }

        private void InitProductSideBar()
        {

            //Side panel init
            ProductSideBar = new FlowLayoutPanel();
            ProductSideBar.Height = MainDiv.Height - SideBarDiv.Controls[0].Height - SideBarDiv.Controls[1].Height - 50;
            ProductSideBar.Width = MainDiv.Width / 5;

            //Disable horizontal scroll bars, setting autoscroll to false first is important
            ProductSideBar.AutoScroll = false;
            ProductSideBar.HorizontalScroll.Enabled = false;
            ProductSideBar.HorizontalScroll.Visible = false;
            ProductSideBar.HorizontalScroll.Maximum = 0;
            ProductSideBar.AutoScroll = true;
            ProductSideBar.FlowDirection = FlowDirection.TopDown;
            //Wrap is for scrolling to work
            ProductSideBar.WrapContents = false;
            ProductSideBar.Margin = new Padding(0);


            //Do this before Scroll bar to check to get right answer
            PopulateSideBar();
           
            //if (CalcHeight(SideBarDiv)< this.ClientSize.Height) ProductSideBar.AutoScroll = false;
        }


        private void Form1_FormClosing(object? sender, FormClosedEventArgs e)
        {
            fav = Favourites.Select(x => x.productName).ToArray();
            File.WriteAllLines("../../../fav.txt",fav);
        }


        //------------Actions--------------


        private void LoadFavouritesFromFile()
        {
            fav = File.ReadAllLines("../../../fav.txt");
        }
        private void LoadProductsFromFile()
        {
            int maxString = 0;
            var workbook = new Excel.XLWorkbook("../../../" + Data);
            int total = 0;
            for (int j = 0; j < scraper.URLS.Count(); j++)
            {
                var sheet = workbook.Worksheets.Worksheet(scraper.URLS[j].Split("/").Last().Split("?")[0]);
                List<string> headers = sheet.Row(1).CellsUsed().Select(c => c.Value.ToString()).ToList();
                List<string> prices = sheet.LastRowUsed().CellsUsed().Select(c => c.Value.ToString()).ToList();
                prices.RemoveAt(0);
                for (int i = 0; i < headers.Count; i++, total++)
                {
                    List<string> data = sheet.Column(i + 2).CellsUsed().Select(c => c.Value.ToString()).ToList();
                    data.RemoveAt(0);

                    List<string> dates = sheet.Column(1).CellsUsed().Select(c => c.Value.ToString()).ToList();
                    string name = headers[i];
                    if(name.Length>maxString) maxString = name.Length;
                    string price = prices[i];
                    ProductSource product = new ProductSource(name, price, total, data, dates, false, scraper.URLS[j].Split("/").Last().Split("?")[0]);
                    Products.Add(product);                    
                }
            }
            Products = Products.OrderBy(x => x.Name).ToList();
            for (int i = 0; i< total; i++)
            {

                bool favorite = false;
                string temp = Products[i].Name;
                if (Products[i].Name.Length< maxString)
                {
                    string space = new string(' ', maxString - Products[i].Name.Length);
                    temp = Products[i].Name + space;
                }
                float change = FindPriceChange(Products[i].data);
                if (fav.Any(x => x.Contains(temp.Trim())))
                {
                    Favourites.Add(new productRefrence(i, Products[i].Name, change));
                    favorite = true;
                }
                Products[i] = new ProductSource(temp, Products[i].Price, i, Products[i].data, Products[i].dates, favorite, Products[i].source);
               
                SearchedItems.Add(new productRefrence(i, Products[i].Name, change));
               
            }

            byPriceChange = SearchedItems.OrderBy(x => x.priceChange).ToList();
            maxlen = maxString;
        }

        internal float FindPriceChange(List<string> data)
        {
            List<string> intdata = data.Where(x => x != null & x!="-1").ToList();
            if (intdata.Count == 1 | intdata.Count == 0) return 0;
            List<float> newdata = intdata.Select(x => float.Parse(x)).ToList();
            newdata.RemoveAt(newdata.Count - 1);
            float average = newdata.Average();
            float change = float.Parse(intdata.Last()) / average;
            float reducedChange = change - 1;
            float percentChange = reducedChange * 100;
            return percentChange;
        }
        internal void PopulateSideBar()
        {
            for (int i = 0 + page * 50; i < Products.Count(); i++)
            {
                Product product = new Product(Products[i].Name, Products[i].Price, Products[i].Index, this);
                product.data = Products[i].data;
                product.dates = Products[i].dates;
                if (i == (page + 1) * 50)
                {
                    this.Selected = (Product)ProductSideBar.Controls[0];
                    this.Selected.ProductName.Font = new Font(this.Selected.ProductName.Font.Name, this.Selected.ProductName.Font.Size, FontStyle.Bold);

                    return;
                }
                ProductSideBar.Controls.Add(product);
                ProductSideBar.Controls.Add(AddSidePanelDivider());
            }
            
        }

        internal void AddProductsToSideBar(List<productRefrence> products)
        {
            int i;
            for (i = 0 + page * 50; i < products.Count; i++)
            {
                int temp = 2 * (i - page * 50);
                if (i == (page + 1) * 50)
                {
                    return;
                }
                Product product = ((Product)ProductSideBar.Controls[temp]);
                product.ProductName.Text = Products[products[i].index].Name;
                product.Price.Text = Products[products[i].index].Price;
                product.sourceIndex = Products[products[i].index].Index;
                //((Product)ProductSideBar.Controls[temp]).data = Products[products[i].index].data;
                //((Product)ProductSideBar.Controls[temp]).dates = Products[products[i].index].dates;
                if (Products[products[i].index].favourite == true) ((Product)ProductSideBar.Controls[temp]).BackColor = Color.FromArgb(183, 230, 86);
                else ((Product)ProductSideBar.Controls[temp]).BackColor = default(Color);
            }
            List<string> cool = new List<string>();
            for (int j = 2 * (i - page * 50); j < ProductSideBar.Controls.Count; j += 2)
            {

                ((Product)ProductSideBar.Controls[j]).ProductName.Text = new string(' ', maxlen);
                ((Product)ProductSideBar.Controls[j]).Price.Text = new string(' ', maxlen);
                ((Product)ProductSideBar.Controls[j]).data = cool;
                ((Product)ProductSideBar.Controls[j]).dates = cool;
                ((Product)ProductSideBar.Controls[j]).BackColor = default(Color);
            }
            /*Panel padding = new Panel();
            padding.Size = new Size(MainDiv.Width / 5, 400);
            padding.AutoSize = false;
            ProductSideBar.Controls.Add(padding);*/
        }

        internal void SearchProducts(object sender, KeyEventArgs e)
        {
            ListUsed = 1;
            if (e.KeyCode != Keys.Enter) return;
            SideBarDiv.SuspendLayout();
            //perform search
            SearchedItems.Clear();
            string[] terms = SearchBar.Text.Split();
            foreach (ProductSource product in Products)
            {
            
                if (product.Name.ToLower().Contains(terms[0].ToLower()) || product.source.ToLower().Contains(terms[0].ToLower()))
                {
                    SearchedItems.Add(new productRefrence(product.Index, product.Name, FindPriceChange(product.data)));
                }

            }
            for(int i = 1; i < terms.Length; i++)
            {
                for(int j = 0; j < SearchedItems.Count(); j++)
                {
                    if(!(Products[SearchedItems[j].index].Name.ToLower().Contains(terms[i].ToLower())|| Products[SearchedItems[j].index].source.ToLower().Contains(terms[i].ToLower())))
                    {
                        SearchedItems.RemoveAt(j);
                        j--;
                    }
                }
            }
            page = 0;
            pageSelector.totalPages = (int)Math.Ceiling(SearchedItems.Count / 50f);
            pageSelector.UpdateSideBar();
            SideBarDiv.ResumeLayout(true);
            
        }


        //-----------------Helper Functions--------------
        
        internal Label AddSidePanelDivider()
        {
            Label divider = new Label();
            divider.Text = string.Empty;
            divider.BorderStyle = BorderStyle.Fixed3D;
            divider.AutoSize = false;
            divider.Height = 4;
            divider.Width = 2000;
            divider.BackColor = Color.FromArgb(218, 214, 203);
            divider.Margin = new Padding(0);
            return divider;
        }
    }
}