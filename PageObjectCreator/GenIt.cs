using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Abot.Crawler;
using Abot.Poco;
using CsQuery.ExtensionMethods;
using CsQuery.ExtensionMethods.Internal;
using HtmlAgilityPack;

namespace PageObjectCreator
{
    [TestClass]
    public class GenIt
    {
        [TestMethod]
        public void GenStuff()
        {
            PoliteWebCrawler crawler = new PoliteWebCrawler();
            
            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            CrawlResult result = crawler.Crawl(new Uri("https://www.mysmartmove.com/"));

            int count = result.CrawlContext.CrawledCount;

            Console.WriteLine(result.CrawlContext.ToJSON());

            Console.WriteLine(result.ToJSON());

            foreach (var tag in GetPageElements())
            {
                Console.WriteLine(tag);
            }
        }

        void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri, pageToCrawl.ParentUri.AbsoluteUri);
        }

        void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            List<string> webPages = new List<string>();

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
            else
                Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);
                webPages.Add(crawledPage.Uri.AbsoluteUri);
                Console.WriteLine(crawledPage.Uri.AbsoluteUri);
            if (string.IsNullOrEmpty(crawledPage.Content.Text))
                Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);

            var htmlAgilityPackDocument = crawledPage.HtmlDocument; //Html Agility Pack parser
            var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; //AngleSharp parser

            //return webPages;
        }

        void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }


        public List<string> GetPageElements()
        {
            List<string> tagsList = new List<string>();

            string url = "https://www.mysmartmove.com/";

            var Webget = new HtmlWeb();

            var doc = Webget.Load(url);

            try
            {
                if (doc.DocumentNode.SelectNodes("//h1").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h1"))
                    {
                        tagsList.Add("//h1[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h2").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h2"))
                    {
                        tagsList.Add("//h2[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h3").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h3"))
                    {
                        tagsList.Add("//h3[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h4").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h4"))
                    {
                        tagsList.Add("//h4[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h5").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h5"))
                    {
                        tagsList.Add("//h5[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h6").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h6"))
                    {
                        tagsList.Add("//h6[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//li").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li"))
                    {
                        tagsList.Add("//li[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//a").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
                    {
                        tagsList.Add("//a[contains(text(),'" + node.ChildNodes[0].InnerHtml + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//img").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//img"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }

                if (doc.DocumentNode.SelectNodes("//p").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//p"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }

                if (doc.DocumentNode.SelectNodes("//div[@id]").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@id]"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }


                if (doc.DocumentNode.SelectNodes("//select").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//select"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }

                if (doc.DocumentNode.SelectNodes("//button").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//button"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }

                if (doc.DocumentNode.SelectNodes("////div[@class]").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class]"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }

                if (doc.DocumentNode.SelectNodes("//th").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//th"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }

                if (doc.DocumentNode.SelectNodes("//td").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//td"))
                    {
                        tagsList.Add(node.ChildNodes[0].InnerHtml);
                    }
                }

            }
            catch (Exception ex)
            {            
                Console.WriteLine(ex.Message);
            }
            

            return tagsList;
        }
    }
}
