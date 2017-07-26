using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Abot.Crawler;
using Abot.Poco;
using CsQuery.ExtensionMethods;
using CsQuery.ExtensionMethods.Internal;
using HtmlAgilityPack;
using System.IO;
using System.Text;

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

            //PageObjects are being created as they are asynchronously found during the crawl
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            CrawlResult result = crawler.Crawl(new Uri(ConfigurationManager.AppSettings["HomePageURL"]));

            int count = result.CrawlContext.CrawledCount;

            Console.WriteLine(result.CrawlContext.ToJSON());

            Console.WriteLine(result.ToJSON());
            Console.WriteLine("Total Crawled Page Count = " + count);

        }

        void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("About to crawl link {0} which was found on page {1}", pageToCrawl.Uri.AbsoluteUri,
                pageToCrawl.ParentUri.AbsoluteUri);
        }

        void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
            }
            else
            {
                
                Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);

                if (ConfigurationManager.AppSettings["PageObjectType"] == "PageFactory")
                {

                    string pageName = crawledPage.Uri.Segments.LastOrDefault();
                                                                         
                    pageName = pageName.RegexReplace("[^a-zA-Z0-9]", string.Empty);
                    
                    //need to create Page Objects by the pages that were successfully crawled
                    WritePageFactoryPageObject(GetPageElements(crawledPage.Uri.AbsoluteUri),
                        pageName);

                    Console.WriteLine("PageFactory PageObject Created");
                }
                else if (ConfigurationManager.AppSettings["PageObjectType"] == "PageObjectGeneral")
                {
                    // need to create Page Objects by the pages that were successfully crawled
                    WriteGeneralPageObject(GetPageElements(crawledPage.Uri.AbsoluteUri), crawledPage.Uri.AbsoluteUri.Remove(crawledPage.Uri.AbsoluteUri.Length -
                                                           crawledPage.Uri.Segments.Last().Length));
                }
                Console.WriteLine(crawledPage.Uri.AbsoluteUri);
            }
            if (string.IsNullOrEmpty(crawledPage.Content.Text))
            {
                Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
            }

            var htmlAgilityPackDocument = crawledPage.HtmlDocument; //Html Agility Pack parser
            var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; //AngleSharp parser

        }

        void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri,
                e.DisallowedReason);
        }

        void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }


        public Dictionary<string, string> GetPageElements(string crawledPageUrl)
        {
            Dictionary<string, string> elementKeyValuePair = new Dictionary<string, string>();

            string url = crawledPageUrl;

            var Webget = new HtmlWeb();

            var doc = Webget.Load(url);

            try
            {
                if (doc.DocumentNode.SelectNodes("//h1").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h1"))
                    {
                        elementKeyValuePair.Add("H1Text" + Guid.NewGuid().ToString("N"),
                            "//h1[contains(text(),'" +
                            node.ChildNodes[0].InnerHtml.Trim().RegexReplace(@"[^0-9a-zA-Z]+", "") + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h2").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h2"))
                    {
                        elementKeyValuePair.Add("H2Text" + Guid.NewGuid().ToString("N"),
                            "//h2[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h3").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h3"))
                    {
                        elementKeyValuePair.Add("H3Text" + Guid.NewGuid().ToString("N"),
                            "//h3[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h4").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h4"))
                    {
                        elementKeyValuePair.Add("H4Text" + Guid.NewGuid().ToString("N"),
                            "//h4[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h5").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h5"))
                    {
                        elementKeyValuePair.Add("H5Text" + Guid.NewGuid().ToString("N"),
                            "//h5[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//h6").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//h6"))
                    {
                        elementKeyValuePair.Add("H6Text" + Guid.NewGuid().ToString("N"),
                            "//h6[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                    }
                }

                if (doc.DocumentNode.SelectNodes("//small").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//small"))
                    {
                        if (node.HasAttributes == true)
                        {
                            foreach (var attribute in node.Attributes)
                            {
                                if (attribute.Name.Contains("id"))
                                {
                                    elementKeyValuePair.Add("Small" + Guid.NewGuid().ToString("N"),
                                        "//small[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("class"))
                                {
                                    elementKeyValuePair.Add("Small" + Guid.NewGuid().ToString("N"),
                                        "//small[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("href"))
                                {
                                    elementKeyValuePair.Add("Small" + Guid.NewGuid().ToString("N"),
                                        "//small[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                            }
                        }
                        else if (node.HasAttributes == false)
                        {
                            elementKeyValuePair.Add("Small" + Guid.NewGuid().ToString("N"),
                                "//small[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                        }

                    }
                }

                if (doc.DocumentNode.SelectNodes("//a").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
                    {
                        if (node.HasAttributes == true)
                        {
                            foreach (var attribute in node.Attributes)
                            {
                                if (attribute.Name.Contains("id"))
                                {
                                    elementKeyValuePair.Add("ALink" + Guid.NewGuid().ToString("N"),
                                        "//a[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("class"))
                                {
                                    elementKeyValuePair.Add("ALink" + Guid.NewGuid().ToString("N"),
                                        "//a[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("alt"))
                                {
                                    elementKeyValuePair.Add("ALink" + Guid.NewGuid().ToString("N"),
                                        "//a[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("href"))
                                {
                                    elementKeyValuePair.Add("ALink" + Guid.NewGuid().ToString("N"),
                                        "//a[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                            }
                        }
                        else if (node.HasAttributes == false)
                        {
                            elementKeyValuePair.Add("ALink" + Guid.NewGuid().ToString("N"),
                                "//a[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                        }
                    }
                }

                if (doc.DocumentNode.SelectNodes("//img").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//img"))
                    {
                        if (node.HasAttributes == true)
                        {
                            foreach (var attribute in node.Attributes)
                            {
                                if (attribute.Name.Contains("id"))
                                {
                                    elementKeyValuePair.Add("Image" + Guid.NewGuid().ToString("N"),
                                        "//img[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("class"))
                                {
                                    elementKeyValuePair.Add("Image" + Guid.NewGuid().ToString("N"),
                                        "//img[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("href"))
                                {
                                    elementKeyValuePair.Add("Image" + Guid.NewGuid().ToString("N"),
                                        "//img[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                            }
                        }
                        else if (node.HasAttributes == false)
                        {
                            elementKeyValuePair.Add("Image" + Guid.NewGuid().ToString("N"),
                                "//img[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                        }
                    }

                }


                if (doc.DocumentNode.SelectNodes("//p").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//p"))
                    {
                        if (node.HasChildNodes == true)
                        {
                            if (node.HasAttributes == true)
                            {
                                foreach (var attribute in node.Attributes)
                                {
                                    if (attribute.Name.Contains("id"))
                                    {
                                        elementKeyValuePair.Add("PText" + Guid.NewGuid().ToString("N"),
                                            "//p[@" + attribute.Name + "=" + "'" +
                                            attribute.Value + "']");
                                    }
                                    else if (attribute.Name.Contains("class"))
                                    {
                                        elementKeyValuePair.Add("PText" + Guid.NewGuid().ToString("N"),
                                            "//p[@" + attribute.Name + "=" + "'" +
                                            attribute.Value + "']");
                                    }
                                    else if (attribute.Name.Contains("href"))
                                    {
                                        elementKeyValuePair.Add("PText" + Guid.NewGuid().ToString("N"),
                                            "//p[@" + attribute.Name + "=" + "'" +
                                            attribute.Value + "']");
                                    }
                                }
                            }
                            else if (node.HasAttributes == false)
                            {
                                elementKeyValuePair.Add("PText" + Guid.NewGuid().ToString("N"),
                                    "//p[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                            }
                        }
                    }
                }

                if (doc.DocumentNode.SelectNodes("//div[@id]").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@id]"))
                    {
                        foreach (var attribute in node.Attributes)
                        {
                            if (attribute.Name == "id")
                            {
                                elementKeyValuePair.Add("DivSection" + Guid.NewGuid().ToString("N"),
                                    "//div[@id=" + "'" + attribute.Value + "']");
                            }
                        }
                    }
                }

                if (doc.DocumentNode.SelectNodes("//div[@class]").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class]"))
                    {
                        foreach (var attribute in node.Attributes)
                        {
                            if (attribute.Name.Contains("id") == false && attribute.Name == "class")
                            {
                                elementKeyValuePair.Add("DivClass" + Guid.NewGuid().ToString("N"),
                                    "//div[@class=" + "'" + attribute.Value + "']");
                            }
                        }
                    }
                }


                if (doc.DocumentNode.SelectNodes("//select").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//select"))
                    {
                        if (node.HasAttributes == true)
                        {
                            foreach (var attribute in node.Attributes)
                            {
                                if (attribute.Name.Contains("id"))
                                {
                                    elementKeyValuePair.Add("DropDown" + Guid.NewGuid().ToString("N"),
                                        "//select[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("class"))
                                {
                                    elementKeyValuePair.Add("DropDown" + Guid.NewGuid().ToString("N"),
                                        "//select[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("href"))
                                {
                                    elementKeyValuePair.Add("DropDown" + Guid.NewGuid().ToString("N"),
                                        "//select[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                            }
                        }
                        else if (node.HasAttributes == false)
                        {
                            elementKeyValuePair.Add("DropDown" + Guid.NewGuid().ToString("N"),
                                "//select[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                        }

                    }
                }

                if (doc.DocumentNode.SelectNodes("//input").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//input"))
                    {
                        if (node.HasAttributes == true)
                        {
                            foreach (var attribute in node.Attributes)
                            {
                                if (attribute.Name.Contains("id"))
                                {
                                    elementKeyValuePair.Add("TextBox" + Guid.NewGuid().ToString("N"),
                                        "//input[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("class"))
                                {
                                    elementKeyValuePair.Add("TextBox" + Guid.NewGuid().ToString("N"),
                                        "//input[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("href"))
                                {
                                    elementKeyValuePair.Add("TextBox" + Guid.NewGuid().ToString("N"),
                                        "//input[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                            }
                        }
                        else if (node.HasAttributes == false)
                        {
                            elementKeyValuePair.Add("TextBox" + Guid.NewGuid().ToString("N"),
                                "//input[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                        }

                    }
                }

                if (doc.DocumentNode.SelectNodes("//button").IsNullOrEmpty() == false)
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//button"))
                    {
                        if (node.HasAttributes == true)
                        {
                            foreach (var attribute in node.Attributes)
                            {
                                if (attribute.Name.Contains("id"))
                                {
                                    elementKeyValuePair.Add("Button" + Guid.NewGuid().ToString("N"),
                                        "//button[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("class"))
                                {
                                    elementKeyValuePair.Add("Button" + Guid.NewGuid().ToString("N"),
                                        "//button[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                                else if (attribute.Name.Contains("href"))
                                {
                                    elementKeyValuePair.Add("Button" + Guid.NewGuid().ToString("N"),
                                        "//button[@" + attribute.Name + "=" + "'" +
                                        attribute.Value + "']");
                                }
                            }
                        }
                        else if (node.HasAttributes == false)
                        {
                            elementKeyValuePair.Add("Button" + Guid.NewGuid().ToString("N"),
                                "//button[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                        }

                    }
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return elementKeyValuePair;
        }


        public void WritePageFactoryPageObject(Dictionary<string, string> pageElements, string pageName)
        {

            // Specify the directory you want to manipulate.
            string path = @"C:\Generator Files\";

            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    Console.WriteLine("That path exists already.");

                }
                else
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }


            using (StreamWriter file = new StreamWriter(@"C:\Generator Files\" + pageName + ".txt"))
            {
                {
                    file.WriteLine("using System;" + Environment.NewLine +
                                   "using System.Collections.Generic;" + Environment.NewLine +
                                   "using System.Configuration;" + Environment.NewLine +
                                   "using System.Linq;" + Environment.NewLine +
                                   "using System.Text;" + Environment.NewLine +
                                   "using System.Threading.Tasks;" + Environment.NewLine +
                                   "using OpenQA.Selenium;" + Environment.NewLine +
                                   "using OpenQA.Selenium.Support.PageObjects;"
                                   + Environment.NewLine);
                }

                {
                    file.WriteLine(Environment.NewLine +
                                    "namespace " + pageName +".Pages" 
                                   + Environment.NewLine 
                                   + "{"
                                   + Environment.NewLine
                                   + Environment.NewLine
                                   + "public class " + pageName 
                                   + Environment.NewLine 
                                   + "{"
                                   + Environment.NewLine
                                   + Environment.NewLine
                                   + "#region Locators"
                                   + Environment.NewLine);
                }

                //remove duplicate values from the Dictionary
                var uniqueValues = pageElements.GroupBy(pair => pair.Value)
                         .Select(group => group.First())
                         .ToDictionary(pair => pair.Key, pair => pair.Value);

                foreach (var element in uniqueValues)
                {
                    if (element.Key.Contains("Logo") || (element.Key.Contains("Image")))
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + "Image " + " { get; set; }"
                                       + Environment.NewLine
                            );

                    }

                    else if (element.Key.Contains("TextBox"))
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + "TextBox " + " { get; set; }"
                                       + Environment.NewLine
                            );
                    }

                    else if (element.Key.Contains("Text"))
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + "Text " + " { get; set; }"
                                       + Environment.NewLine
                            );
                    }

                    else if (element.Key.Contains("Link") || (element.Key.Contains("Tab")))
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + "Link " + " { get; set; }"
                                       + Environment.NewLine
                            );
                    }

                    else if (element.Key.Contains("DropDown"))
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + "DropDown " + " { get; set; }"
                                       + Environment.NewLine
                            );
                    }

                    else if (element.Key.Contains("CheckBox"))
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + "CheckBox " + " { get; set; }"
                                       + Environment.NewLine
                            );
                    }

                    else if (element.Key.Contains("Button"))
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + "Button " + " { get; set; }"
                                       + Environment.NewLine
                            );
                    }

                    else
                    {
                        file.WriteLine("[FindsBy(How = How.XPath, Using = \"" + element.Value + "\")]"
                                       + Environment.NewLine
                                       + "[CacheLookup]"
                                       + Environment.NewLine
                                       + "public IWebElement " + element.Key + " { get; set; }"
                                       + Environment.NewLine
                            );
                    }
                }

                {
                    file.WriteLine(Environment.NewLine + "#endregion");
                }

                {
                    file.WriteLine(Environment.NewLine + "#region constructor"
                                   + Environment.NewLine
                                   + Environment.NewLine);
                }

                {
                    file.Write("public " + ConfigurationManager.AppSettings["PageName"] + "(IWebDriver driver)"
                               + Environment.NewLine + "{"
                               + Environment.NewLine
                               + Environment.NewLine + "this.Driver = driver;"
                               + Environment.NewLine + "PageFactory.InitElements(driver, this);"
                               + Environment.NewLine
                               + Environment.NewLine
                               + Environment.NewLine + "}"
                               + Environment.NewLine
                               + Environment.NewLine);
                }

                {
                    file.WriteLine("protected void WaitForPageToLoad()"
                                   + Environment.NewLine + "{"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine
                                   + Environment.NewLine);
                }

                {
                    file.WriteLine("protected void ValidatePageLoaded()"
                                   + Environment.NewLine + "{"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine
                                   + Environment.NewLine + "#endregion"
                                   + Environment.NewLine);
                }


                {
                    file.WriteLine(Environment.NewLine
                                   + Environment.NewLine
                                   + Environment.NewLine
                                   + Environment.NewLine + "#region Actions"
                                   + Environment.NewLine
                                   + Environment.NewLine
                                   + Environment.NewLine + "#endregion"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}");
                }
            }

        }

        public void WriteGeneralPageObject(Dictionary<string, string> pageElements, string pageName)
        {
            using (StreamWriter file = new StreamWriter(@"C:\Generator Files\" + pageName + "PageObject.txt")
                )
            {
                {
                    file.WriteLine("using System;" + Environment.NewLine +
                                   "using System.Collections.Generic;" + Environment.NewLine +
                                   "using System.Configuration;" + Environment.NewLine +
                                   "using System.Linq;" + Environment.NewLine +
                                   "using System.Text;" + Environment.NewLine +
                                   "using System.Threading.Tasks;" + Environment.NewLine +
                                   "using OpenQA.Selenium;" + Environment.NewLine +
                                   "using OpenQA.Selenium.Support.PageObjects;");
                }

                {
                    file.WriteLine("namespace " + pageName +
                                   ".Pages" + Environment.NewLine + "{"
                                   + Environment.NewLine +
                                   "public class " + pageName +
                                   Environment.NewLine + "{"
                                   + Environment.NewLine + "#region Locators"
                                   + Environment.NewLine);
                }

                foreach (var element in pageElements.Distinct())
                {
                    if (element.Key.Contains("Logo") || (element.Key.Contains("Image")))
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");

                    }

                    else if (element.Key.Contains("TextBox"))
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");
                    }

                    else if (element.Key.Contains("Text"))
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");
                    }

                    else if (element.Key.Contains("Link") || (element.Key.Contains("Tab")))
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");
                    }

                    else if (element.Key.Contains("DropDown"))
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");
                    }

                    else if (element.Key.Contains("CheckBox"))
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");
                    }

                    else if (element.Key.Contains("Button"))
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");
                    }

                    else
                    {
                        file.WriteLine("protected By " + element.Key + "Locator" + " = By.XPath(" + element.Value + ");");
                    }
                }

                {
                    file.WriteLine(Environment.NewLine + "#endregion");
                }

                {
                    file.WriteLine(Environment.NewLine + "#region constructor"
                                   + Environment.NewLine
                                   + Environment.NewLine);
                }

                {
                    file.Write("public " + ConfigurationManager.AppSettings["PageName"] + "(IWebDriver driver)"
                               + Environment.NewLine + "{"
                               + Environment.NewLine
                               + Environment.NewLine + "this.Driver = driver;"
                               + Environment.NewLine
                               + Environment.NewLine
                               + Environment.NewLine + "}"
                               + Environment.NewLine);
                }

                {
                    file.WriteLine("protected void WaitForPageToLoad()"
                                   + Environment.NewLine + "{"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine
                                   + Environment.NewLine);
                }

                {
                    file.WriteLine("protected void ValidatePageLoaded()"
                                   + Environment.NewLine + "{"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine
                                   + Environment.NewLine + "#endregion"
                                   + Environment.NewLine);
                }


                {
                    file.WriteLine(Environment.NewLine
                                   + Environment.NewLine + "};"
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine);
                }

                {
                    file.WriteLine(Environment.NewLine
                                   + Environment.NewLine + "#endregion"
                                   + Environment.NewLine
                                   + Environment.NewLine
                                   + Environment.NewLine + "#region Actions"
                                   + Environment.NewLine
                                   + Environment.NewLine
                                   + Environment.NewLine + "#endregion"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}"
                                   + Environment.NewLine
                                   + Environment.NewLine + "}");
                }
            }
        }
    }

}
    



