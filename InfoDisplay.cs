using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;


namespace _2023S2_SProj1_ThousandMissile
{
    internal class InfoDisplay:FlowLayoutPanel
    {
        Form1 form;
        FlowLayoutPanel sideBar;
        TextBox NameLabel;
        internal TextBox Name;
        TextBox PriceLabel;
        internal TextBox Price;
        internal Label Test;
        internal Chart priceHistory;
        TextBox sixMonthHighLabel;
        internal TextBox sixMonthHigh;
        TextBox sixMonthLowLabel;
        internal TextBox sixMonthLow;
        TextBox priceChangeLabel;
        internal TextBox priceChange;
        TextBox yestChangeLabel;
        internal TextBox yestChange;
        ChartArea chartArea;
        internal Button favourites;
        internal Series priceData;
        
        
        
        public InfoDisplay(Form1 form)
        {


            Test = new Label();

            NameLabel = new TextBox();
            NameLabel.Text = "Product Name:";
            Name = new TextBox();
            Name.Text = "Apple";

            PriceLabel = new TextBox();
            PriceLabel.Text = "Price:";
            Price  = new TextBox();
            Price.Text = "$-1";

            sixMonthHighLabel = new TextBox();
            sixMonthHighLabel.Text = "Highest price in the last 6 Months:";
            sixMonthHigh = new TextBox();
            sixMonthHigh.Text = "999";

            sixMonthLowLabel = new TextBox();
            sixMonthLowLabel.Text = "Lowest price in the last 6 Months:";
            sixMonthLow = new TextBox();
            sixMonthLow.Text = "0";

            priceChangeLabel = new TextBox();
            priceChangeLabel.Text = "price change from average";
            priceChange = new TextBox();
            priceChange.Text = "0";

            yestChangeLabel = new TextBox();
            yestChangeLabel.Text = "% price change from average";
            yestChange = new TextBox();
            yestChange.Text = "0%";

            favourites = new Button();
            favourites.Text = "Add/Remove from favourites";
            favourites.Click += Favourites_Click;
            favourites.Font = new Font("Tahoma", 8);
            favourites.AutoSize = true;




            priceHistory = new Chart();
            priceHistory.Size = new Size(form.ClientSize.Width- form.ClientSize.Width/2, form.ClientSize.Height);
            chartArea = new ChartArea();
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisY.MajorGrid.Enabled = false;
            priceData = new Series();
            priceData.BorderWidth = 5;
            priceData.MarkerStyle = MarkerStyle.Triangle;
            priceData.MarkerColor = Color.Red;
            priceData.MarkerSize= 10;
            priceData.XValueType = ChartValueType.Date;
            priceData.ChartType = SeriesChartType.Line;
            priceData.BackHatchStyle = ChartHatchStyle.Cross;
            priceData.IsValueShownAsLabel = true;
            priceHistory.ChartAreas.Add(chartArea);
            priceHistory.Series.Add(priceData);

            sideBar = new FlowLayoutPanel();
            sideBar.Width = form.ClientSize.Width/5;
            sideBar.Height = form.ClientSize.Height;
            sideBar.AutoScroll = false;
            sideBar.WrapContents = false;
            sideBar.Padding = new Padding(0, 30, 0, 0);
            sideBar.FlowDirection = FlowDirection.TopDown;
            sideBar.Controls.Add(NameLabel);
            sideBar.Controls.Add(Name);
            sideBar.Controls.Add(PriceLabel);
            sideBar.Controls.Add(Price);
            sideBar.Controls.Add(sixMonthHighLabel);
            sideBar.Controls.Add(sixMonthHigh);
            sideBar.Controls.Add(sixMonthLowLabel);
            sideBar.Controls.Add(sixMonthLow);
            sideBar.Controls.Add(priceChangeLabel);
            sideBar.Controls.Add(priceChange);
            sideBar.Controls.Add(yestChangeLabel);
            sideBar.Controls.Add(yestChange);
            sideBar.Controls.Add(favourites);

            this.form = form;
            this.Width = form.ClientSize.Width - form.ClientSize.Width / 5-50;
            this.Height = form.ClientSize.Height;
            this.Padding = new Padding(40, 0, 10, 0);
            this.FlowDirection = FlowDirection.LeftToRight;
            this.AutoScroll = false;
            this.WrapContents = false;

            this.Controls.Add(sideBar);
            this.Controls.Add(priceHistory);



            int i = 0;
            //High level optimisations
            foreach (Control ctr in this.Controls[0].Controls)
            {

                if (ctr.GetType() == typeof(TextBox))
                {
                    TextBox txt = ((TextBox)ctr);
                    txt.AutoSize = true;
                    txt.WordWrap = true;
                    txt.Multiline = true;
                    txt.BorderStyle = BorderStyle.None;
                    txt.BackColor = BackColor;
                    txt.ReadOnly = true;
                    txt.Font = new Font("Tahoma", 10+5*(i%2));
                    txt.Padding = new Padding(0, 0, 0, 10 + 20 * (i % 2));
                    txt.BackColor = Color.FromArgb(247, 246, 242);
                    txt.ForeColor = Color.FromArgb(93, 90, 84);
                    TextLength(txt);
                    i++;
                }
            }

        }

        public void UpdateInfo(Product caller)
        {
            ProductSource temp = form.Products[caller.sourceIndex];
            Name.Text =  caller.ProductName.Text;
            Price.Text = "$" +caller.Price.Text;
            if (Price.Text == "$-1") Price.Text = "OUT OF STOCK";

            priceHistory.Series[0].Points.Clear();
            for (int i = 0; i < temp.data.Count; i++)
            {
                string val = temp.data[i];
                if (val == "-1")
                {
                    temp.data[i] = null;
                    val = null;
                }
                priceHistory.Series[0].Points.AddXY(Convert.ToDateTime(temp.dates[i]).ToOADate(), val);
            }

            
            List<string> intdata = temp.data.Where(x => x != null).ToList();
            sixMonthHigh.Text = "$" + intdata.Max(x=>float.Parse(x));
            sixMonthLow.Text = "$" + intdata.Min(x => float.Parse(x));
            if (intdata.Count == 1 | intdata.Count == 0) yestChange.Text = "0%";
            else
            {
                List<float> newdata = intdata.Select(x => float.Parse(x)).ToList();
                newdata.RemoveAt(newdata.Count - 1);
                float average = newdata.Average();
                float change = float.Parse(intdata.Last()) / average;
                float reducedChange = change - 1;
                float percentChange = reducedChange * 100;
                float absoluteChange = float.Parse(intdata.Last()) - average;
                priceChange.Text = "$" + absoluteChange.ToString("F");
                yestChange.Text = percentChange.ToString("F") + "%";
            }
            for (int i = 1; i < Controls[0].Controls.Count; i += 2)
            {

                TextBox txt = (TextBox)Controls[0].Controls[i];
                TextLength(txt);
            }
        }


        private void Favourites_Click(object? sender, EventArgs e)
        {
            
            form.Selected.favourite = !form.Selected.favourite;
            productRefrence refrence = new productRefrence(form.Selected.sourceIndex,form.Selected.ProductName.Text,form.FindPriceChange(form.Selected.data));

            ProductSource updates = new ProductSource(form.Products[form.Selected.sourceIndex].Name, form.Products[form.Selected.sourceIndex].Price, form.Selected.sourceIndex,
                                                            form.Products[form.Selected.sourceIndex].data, form.Products[form.Selected.sourceIndex].dates, form.Selected.favourite, 
                                                            form.Products[form.Selected.sourceIndex].source);

            //Add or remove from favourites
            if (form.Selected.favourite == true) form.Favourites.Add(refrence);
            else
            {
                for(int i = 0; i< form.Favourites.Count; i++)
                {
                    if(form.Selected.ProductName.Text == form.Products[form.Favourites[i].index].Name)
                    {
                        form.Favourites.RemoveAt(i);
                        break;
                    }
                }
            }
            //Update real refrence
            form.Products[form.Selected.sourceIndex] = updates;
            form.Selected.BackColor = Color.FromArgb(183, 230, 86);


        }

        public void TextLength(TextBox WhatLengthAmI)
        {
            Test.Text = WhatLengthAmI.Text;
            Test.Font = WhatLengthAmI.Font;
            Test.AutoSize = true;
            Test.MaximumSize = new Size(form.ClientSize.Width / 5 - 50, 99999999);
            sideBar.Controls.Add(Test);
            WhatLengthAmI.Height = Test.Height;
            WhatLengthAmI.Width = Test.Width+50;
            sideBar.Controls.Remove(Test);
        }
    }

}
