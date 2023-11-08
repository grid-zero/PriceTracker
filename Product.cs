using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = ClosedXML.Excel;

namespace _2023S2_SProj1_ThousandMissile
{

    public struct ProductSource
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public int Index { get; set; }
        public List<string> data = new List<string>();
        public List<string> dates = new List<string>();
        public bool favourite;
        public ProductSource(string name, string price, int index, List<string> data, List<string> dates,bool fav)
        {
            Name = name;
            Price = price;
            Index = index;
            this.data = data;
            this.dates = dates;
            favourite = fav;
        }
    }

    internal class Product:FlowLayoutPanel
    {
        public Label ProductName;
        public Label Price;
        public int sourceIndex;
        public Form1 form;
        public List<string> data = new List<string>();
        public List<string> dates = new List<string>();
        internal bool favourite = false;
        public Product(string name, string price, int index, Form1 form)
        {
            this.form = form;

            ProductName = new Label();
            ProductName.Text = name;
            ProductName.Padding = new Padding(12, 0, 18, 0);
            ProductName.MaximumSize = new Size(form.ClientSize.Width / 5 -15, 10000000);
            ProductName.AutoSize = true;
            ProductName.Font = new Font("Tahoma",10);
            ProductName.ForeColor = Color.FromArgb(93, 90, 84);
            ProductName.Click += Product_Click;

            Price = new Label();       
            Price.Text = price;
            Price.Padding = new Padding(12, 0, 18, 0);
            Price.MaximumSize = new Size(form.ClientSize.Width / 5 -15, 10000000);
            Price.AutoSize = true;
            Price.Font = new Font("Tahoma", 10);
            Price.ForeColor = Color.FromArgb(93, 90, 84);
            Price.Click += Product_Click;


            sourceIndex = index;
            this.Controls.Add(ProductName);
            this.Controls.Add(Price);
            this.AutoSize = true;
            this.FlowDirection = FlowDirection.TopDown;
            this.Padding = new Padding(0, 10, 0, 10);
            this.MinimumSize = new Size(form.ClientSize.Width / 5, 0);
            this.Click += Product_Click;

        }

        private void Product_Click(object? sender, EventArgs e)
        {

            if (ProductName.Text == "") return;
            if (form.Selected != null)
            {
                form.Selected.ProductName.Font = new Font(form.Selected.ProductName.Font.Name, 10);
                if (form.Products[form.Selected.sourceIndex].favourite == true)
                {

                }
                else
                {
                    form.Selected.BackColor = Color.FromArgb(247, 246, 242);
                }
            }
            this.ProductName.Font = new Font(ProductName.Font.Name, 9, FontStyle.Bold);
            form.Selected = this;
            if(form.Products[this.sourceIndex].favourite == true)
            {

            }
            else
            {
                this.BackColor = Color.FromArgb(239, 236, 229);
            }
           
            //Update lengths
            form.infoDisplay.UpdateInfo(this);

           
        }
        public void UpdateLabels()
        {
            ProductName.AutoSize = false;
            ProductName.MaximumSize = new Size(form.ClientSize.Width / 5 -15, 10000000);
            ProductName.AutoSize = true;

        }
    }
}
