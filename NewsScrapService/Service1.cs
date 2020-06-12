using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using System.Windows.Forms;



namespace NewsScrapService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }

        public class News
        {
            public News() { }
            public News(string title, string url, string from=null, string time = null)
            {
                Title = title;
                Url = url;
                From = from;
                Time = time;
            }

            public string Title { get; set; }
            public string Url { get; set; }
            public string From { get; set; }
            public string Time { get; set; }

        }

        public void Process()
        {
            List<News> news = new List<News>();

            int page = 1;
            string url = string.Format("https://news.daum.net/breakingnews/economic?page={0}", page);
            string text =  Scraping.Scrap(url, Method.GET, null);
            
            string filter1 = "<strong class=\"tit_thumb\">";
            string filter2 = "<a href=\"";
            string filter3 = "class=\"link_txt\">";
            string filter4 = "</a>";

            while (text.IndexOf(filter1) > 0)
            {
                text = text.Substring(text.IndexOf(filter1) + filter1.Length);
                text = text.Substring(text.IndexOf(filter2) + filter2.Length);

                //
                string clickUrl = text.Substring(0, text.IndexOf("\""));

                text = text.Substring(text.IndexOf(filter3) + filter3.Length);

                string title = text.Substring(0, text.IndexOf(filter4));

                news.Add(new News(title, clickUrl));

            }

            



            //HTML

            //< strong class="tit_thumb">
            //        <a href = "https://v.daum.net/v/20200612174202121" class="link_txt">고용보험 확대·한국판 뉴딜..내년 예산 550조 넘을듯</a>
            //        <span class="info_news">매일경제<span class="txt_bar"> · </span><span class="info_time">17:42</span></span>
            //    </strong>

        }
        public void Process2()
        {
            int page = 1;
            string url = string.Format("https://news.daum.net/breakingnews/economic?page={0}", page);
            //string text =  Scraping.Scrap(url, Method.GET, null);
            WebBrowser browser = new WebBrowser();
            browser.Navigate(url);

            HtmlDocument doc = browser.Document;
            var titles = doc.GetElementsByTagName("string");

            foreach (HtmlElement data in titles)
            {
                Console.WriteLine(data.InnerText);
            }



            //HTML

            //< strong class="tit_thumb">
            //        <a href = "https://v.daum.net/v/20200612174202121" class="link_txt">고용보험 확대·한국판 뉴딜..내년 예산 550조 넘을듯</a>
            //        <span class="info_news">매일경제<span class="txt_bar"> · </span><span class="info_time">17:42</span></span>
            //    </strong>
        }
        
    }
}
