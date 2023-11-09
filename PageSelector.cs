using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2023S2_SProj1_ThousandMissile
{
    internal class PageSelector: FlowLayoutPanel 
    {


        public Form1 form;
        public FlowLayoutPanel ButtonDiv;
        public Button back;
        public Button forward;
        public Button favourites;
        public Button sortChange;
        public Button ReversePrice;
        Label PageCount;
        public int totalPages;
        public PageSelector(Form1 form)
        {
            this.form = form;
            this.Width = form.ClientSize.Width/5;
            this.FlowDirection = FlowDirection.TopDown;
            this.AutoSize = true;

         

            back = new Button();
            back.Text =  "←";
            back.Click += Back_Click;

            forward = new Button();
            forward.Text = "→";
            forward.Click += Forward_Click;

            favourites = new Button();
            favourites.Text = "favourites";
            favourites.Click += Favourites_Click;
            favourites.BackColor = Color.GreenYellow;
            favourites.FlatStyle = FlatStyle.Flat;
            favourites.FlatAppearance.BorderSize = 0;

            sortChange = new Button();
            sortChange.Text = "Sort by Price Change";
            sortChange.Click += SortChange_Click;
            sortChange.BackColor = Color.LightGreen;
            sortChange.FlatStyle = FlatStyle.Flat;
            sortChange.FlatAppearance.BorderSize = 0;

            ReversePrice = new Button();
            ReversePrice.Text = "Reverse Order";
            ReversePrice.Click += ReversePrice_Click;

            ButtonDiv = new FlowLayoutPanel();
            //ButtonDiv.AutoSize = true;
            ButtonDiv.Width = form.ClientSize.Width / 5;
            ButtonDiv.FlowDirection = FlowDirection.LeftToRight;
            ButtonDiv.WrapContents = true;
            ButtonDiv.Controls.Add(back);
            ButtonDiv.Controls.Add(forward);
            ButtonDiv.Controls.Add(favourites);
            ButtonDiv.Controls.Add(sortChange);
            ButtonDiv.Controls.Add(ReversePrice);

            foreach (Control ctr in ButtonDiv.Controls)
            {
                Button but = (Button)ctr;
                //but.FlatStyle = FlatStyle.System;
                but.AutoSize = true;
                but.Font = new Font("Tahoma", 10);
                but.ForeColor = Color.FromArgb(93, 90, 84);
            }
            sortChange = new Button();

            

            PageCount = new Label();
            PageCount.Text = "Page 1 of 10";
            PageCount.AutoSize = true;
            PageCount.Font = new Font("Tahoma", 10);
            PageCount.ForeColor = Color.FromArgb(93, 90, 84);

            totalPages = (int)Math.Ceiling(form.Products.Count/50f);
            this.Controls.Add(PageCount);
            this.Controls.Add(ButtonDiv);
        }

        private void ReversePrice_Click(object? sender, EventArgs e)
        {

            if (form.ListUsed == 1) form.SearchedItems.Reverse();
            if (form.ListUsed == 2) form.Favourites.Reverse();
            if(form.ListUsed == 3) form.SearchedItems.Reverse();
            UpdateSideBar();
        }

        private void SortChange_Click(object? sender, EventArgs e)
        {
            form.ListUsed = 3;
            form.page = 0;
            form.pageSelector.totalPages = (int)Math.Ceiling(form.SearchedItems.Count / 50f);
            form.SearchedItems = form.SearchedItems.OrderBy(x => x.priceChange).ToList();
            UpdateSideBar();
        }

        private void Favourites_Click(object? sender, EventArgs e)
        {
            form.ListUsed = 2;
            form.page = 0;
            form.pageSelector.totalPages = (int)Math.Ceiling(form.Favourites.Count / 50f);
            UpdateSideBar();
        }

        private void Back_Click(object? sender, EventArgs e)
        {
            if (form.page == 0) return;
            form.page--;
            UpdateSideBar();
        }

        private void Forward_Click(object? sender, EventArgs e)
        {
            if (form.page > totalPages-2) return;
            form.page++;
            UpdateSideBar();
        }

        public void UpdateSideBar()
        {
            PageCount.Text = String.Format("Page {0} of {1}", form.page + 1, totalPages);
            if (form.ListUsed == 1)
            {
                form.AddProductsToSideBar(form.SearchedItems);
            }
            if (form.ListUsed == 2)
            {
                form.AddProductsToSideBar(form.Favourites);
            }
            if (form.ListUsed == 3)
            {
                form.AddProductsToSideBar(form.SearchedItems);
            }
            if(form.ListUsed == 4)
            {

            }
         

            form.Selected.ProductName.Font = new Font(form.Selected.ProductName.Font.Name, form.Selected.ProductName.Font.Size);
            if (form.ProductSideBar.Controls[0].ProductName != "")
            {
                form.Selected = (Product)form.ProductSideBar.Controls[0];
                form.Selected.ProductName.Font = new Font(form.Selected.ProductName.Font.Name, form.Selected.ProductName.Font.Size, FontStyle.Bold);
            }
            else
            {
                form.Selected = null;
            }
            form.infoDisplay.UpdateInfo(form.Selected);
            form.ProductSideBar.VerticalScroll.Value = 0;
        }
    }
}
