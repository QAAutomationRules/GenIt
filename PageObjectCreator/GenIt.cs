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
using System.Security.Cryptography.X509Certificates;
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

            dynamic blah = crawler.CrawlBag;

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
                    WriteGeneralPageObject(GetPageElements(crawledPage.Uri.AbsoluteUri),
                        crawledPage.Uri.AbsoluteUri.Remove(crawledPage.Uri.AbsoluteUri.Length -
                                                           crawledPage.Uri.Segments.Last().Length));
                }
                Console.WriteLine(crawledPage.Uri.AbsoluteUri);
            }
            if (string.IsNullOrEmpty(crawledPage.Content.Text))
            {
                Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
            }
            
            
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

            HtmlWeb Webget = new HtmlWeb();

            HtmlAgilityPack.HtmlDocument doc = Webget.Load(url);

            try
            {
                GetXpaths(SetTags(), doc, elementKeyValuePair);             
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
                                   "namespace " + pageName + ".Pages"
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

        public void GetXpaths(Dictionary<string, string> tags,
            HtmlAgilityPack.HtmlDocument doc, Dictionary<string, string> elementKeyValuePair)
        {
            foreach(var tag in tags)
            { 
            if (doc.DocumentNode.SelectNodes(tag.Key).IsNullOrEmpty() == false)
            {
                if (tag.Key.Contains("//h"))
                {
                        if (doc.DocumentNode.SelectNodes(tag.Key).IsNullOrEmpty() == false)
                        {
                            foreach (HtmlNode node in doc.DocumentNode.SelectNodes(tag.Key))
                            {
                                elementKeyValuePair.Add(tag.Value + Guid.NewGuid().ToString("N"),
                                    tag.Key + "[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim().RegexReplace(@"[^0-9a-zA-Z]+", "") + "')]");
                            }
                        }
                    }
                else if (tag.Key.Contains("//div"))
                {
                        if (doc.DocumentNode.SelectNodes(tag.Key).IsNullOrEmpty() == false)
                        {
                            foreach (HtmlNode node in doc.DocumentNode.SelectNodes(tag.Key))
                            {
                                foreach (var attribute in node.Attributes)
                                {
                                    if (attribute.Name.Contains("id") == false && attribute.Name == "class")
                                    {
                                        elementKeyValuePair.Add(tag.Value + Guid.NewGuid().ToString("N"),
                                            tag.Key + "'" + attribute.Value + "']");
                                    }
                                }
                            }
                        }
                    }
                else
                    {
                        foreach (HtmlNode node in doc.DocumentNode.SelectNodes(tag.Key))
                        {
                            if (node.HasAttributes == true)
                            {
                                foreach (var attribute in node.Attributes)
                                {
                                    if (attribute.Name.Contains("id"))
                                    {
                                        elementKeyValuePair.Add(tag.Value + Guid.NewGuid().ToString("N"),
                                            tag.Key + "[@" + attribute.Name + "=" + "'" +
                                            attribute.Value + "']");
                                    }
                                    else if (attribute.Name.Contains("name"))
                                    {
                                        elementKeyValuePair.Add(tag.Value + Guid.NewGuid().ToString("N"),
                                            tag.Key + "[@" + attribute.Name + "=" + "'" +
                                            attribute.Value + "']");
                                    }
                                    else if (attribute.Name.Contains("class"))
                                    {
                                        elementKeyValuePair.Add(tag.Value + Guid.NewGuid().ToString("N"),
                                            tag.Key + "[@" + attribute.Name + "=" + "'" +
                                            attribute.Value + "']");
                                    }
                                    else if (attribute.Name.Contains("href"))
                                    {
                                        elementKeyValuePair.Add(tag.Value + Guid.NewGuid().ToString("N"),
                                            tag.Key + "[@" + attribute.Name + "=" + "'" +
                                            attribute.Value + "']");
                                    }
                                }
                            }
                            else if (node.HasAttributes == false)
                            {
                                elementKeyValuePair.Add(tag.Value + Guid.NewGuid().ToString("N"),
                                    tag.Key + "[contains(text(),'" + node.ChildNodes[0].InnerHtml.Trim() + "')]");
                            }
                        }
                    }
            }
            }
        }

        public Dictionary<string, string> SetTags()
        {
            Dictionary<string, string> tags = new Dictionary<string, string>();

            //add tags and tag Names
            tags.Add("//h1", "H1Text");
            tags.Add("//h2", "H2Text");
            tags.Add("//h3", "H3Text");
            tags.Add("//h4", "H4Text");
            tags.Add("//h5", "H5Text");
            tags.Add("//h6", "H6Text");
            tags.Add("//p", "Text");
            tags.Add("//a", "Link");
            tags.Add("//img", "Image");
            tags.Add("//select", "DropDown");
            tags.Add("//input", "TextBox");
            tags.Add("//Button", "Button");
            tags.Add("//label", "Text");
            tags.Add("//td", "Table");
            tags.Add("//div[@id]", "DivSection");
            tags.Add("//div[@class]", "DivClass");

            return tags;

        }
    }
}


    



