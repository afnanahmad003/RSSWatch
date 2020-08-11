using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ApplicationLogger;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Threading;
using HtmlAgilityPack;
using RESSWATCH.Shopify.Json;
using OpenQA.Selenium.Chrome;
using System.Net;
using Newtonsoft.Json;
using System.Web;

namespace RESSWATCH
{
    public partial class RESSWATCHServer : ServiceBase
    {
        private System.Timers.Timer ScrapperTimer = null;

        public RESSWATCHServer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                 StartServer();
            }
            catch (Exception E)
            {
                Logger.WriteException(E);
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (ScrapperTimer != null)
                {
                    ScrapperTimer.Elapsed -= new System.Timers.ElapsedEventHandler(ScrapperTimer_Elapsed);
                    ScrapperTimer.Stop();
                    Logger.WriteLog("DB timer stopped ", LogLevel.GENERALLOG);
                }

                Logger.WriteLog("Service gracefully terminated", LogLevel.GENERALLOG);
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
        }

        private void TraceConfig(string Path, int RotateFrequency, int FileSize, bool Communication, bool Database, bool Telephony, bool General)
        {
            try
            {
                Logger.EnableCOMMSLOG = Communication;
                Logger.EnableDBLOG = Database;
                Logger.EnableGENERALLOG = General;
                Logger.EnableTELEPHONYLOG = Telephony;
                Logger.EnableERRORLOG = Logger.EnableSECURELOG = true;
                Logger.FilePath = Path;
                Logger.LogFileName = "RSSWatch Logs";
                Logger.LogFileSize = FileSize;
                Logger.NoOfLogFiles = RotateFrequency;
                Logger.ActivateOptions();
                Logger.WriteLog("Init logs", LogLevel.GENERALLOG);
            }
            catch (Exception E)
            {
                string strEx = E.Message;
            }
        }

        public void StartServer()
        {
            try
            {
                TraceConfig(Path.GetDirectoryName(Application.ExecutablePath) + "\\Logs", 20, 3000, true, true, true, true);

                Logger.WriteLog("DBConServer has started.", LogLevel.GENERALLOG);

                DataAccess.CON_STR = System.Configuration.ConfigurationSettings.AppSettings["SQLConStr"];

                if (!DataAccess.DatabaseAvailable)
                {
                    Logger.WriteLog("SQL is not connected.", LogLevel.DBLOG);
                }
                else
                    Logger.WriteLog("SQL is connected.", LogLevel.DBLOG);

                Load();
                //ParseHeaders();
                //ScrapperTimer = new System.Timers.Timer();
                //ScrapperTimer.Elapsed += new System.Timers.ElapsedEventHandler(ScrapperTimer_Elapsed);
                //ScrapperTimer.Interval = 1000;
                //ScrapperTimer.Start();
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
        }

        private void Load()
        {
            try
            {
                while (true)
                {
                    Logger.WriteLog("Started...........",LogLevel.GENERALLOG);
                    try
                    {
                        ParseHeaders();
                    }
                    catch (Exception exp)
                    {
                        Logger.WriteException(exp);
                    }
                    Logger.WriteLog("Going to sleep...........", LogLevel.GENERALLOG);
                    Thread.Sleep(87000000);
                }
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
        }
        private void ScrapperTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ScrapperTimer.Stop();
            try
            {
                ParseHeaders();
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
            ScrapperTimer.Start();
        }

        private static void PostProduct(ProductDetail objInput)
        {
            try
            {
                Thread.Sleep(3000);
                bool IsUpdate = false;
                string ShopifyID = string.Empty;
                DataTable dtProd = BL.Product.Get_ProductByparentID(objInput.ParentID);
                if (dtProd != null && dtProd.Rows.Count > 0)
                {
                    IsUpdate = true;
                    ShopifyID = dtProd.Rows[0]["ShopifyID"].ToString();
                }

                string URL = "https://stayfrozty.myshopify.com/admin/api/2020-07/products.json";
                if (IsUpdate)
                    URL = "https://stayfrozty.myshopify.com/admin/api/2020-07/products/" + ShopifyID + ".json";

                string content = PrepareProductrequest(objInput, ShopifyID);
                string MethodType = "POST";
                if (IsUpdate)
                    MethodType = "PUT";

                APICallResponse result = BrowseURLToString(URL, "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", content, MethodType);
                ProductType ProductRes = JsonConvert.DeserializeObject<ProductType>(result.ResponseContent);

                if (ProductRes == null)
                    Logger.WriteLog("Failed to post product Content:" + content, LogLevel.ERRORLOG);
                else if (ProductRes.errors != null)
                {
                    Logger.WriteLog("Failed to post product Content:" + content, LogLevel.ERRORLOG);
                    Logger.WriteLog("Error Details" + JsonConvert.SerializeObject(ProductRes.errors), LogLevel.ERRORLOG);
                }
                else if (!IsUpdate)
                {
                    string ParentID = objInput.ParentID;
                    ShopifyID = ProductRes.product.id.ToString();
                    for (int i = 0; i < ProductRes.product.variants.Count; i++)
                    {
                        string VarientID = ProductRes.product.variants[i].id.ToString();
                        string VarientSKU = ProductRes.product.variants[i].sku;
                        BL.Product.Add(ParentID, ShopifyID, VarientID, VarientSKU);
                        Logger.WriteLog("Product Add => ParentIDP:" + ParentID + ", ShopifyID:" + ShopifyID + ", VarientID:" + VarientID + ", VarientSKU:" + VarientSKU, LogLevel.DBLOG);
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
        }

        private static string PrepareProductrequest(ProductDetail objInput, string ShopifyID)
        {
            try
            {
                Shopify.Json.Product prodInfo = new Shopify.Json.Product();
                {
                    if (ShopifyID != string.Empty)
                    {
                        long ID = 0;
                        long.TryParse(ShopifyID, out ID);
                        prodInfo.id = ID;
                    }

                    //prodInfo.body_html = "<strong>Description</strong><br/>" + objInput.ProductDesc + "<br/><strong>Return Detail</strong><br/>" + objInput.ReturnDetail + "<br/><strong>Delivery Detail</strong><br/>" + objInput.DeliverDetail;
                    prodInfo.body_html = "<h3>Return Policies:</h3><h4>Normal conditions of return apply:</h4><p>Return your purchase to us within 18 days of dispatch of your order by post with your despatch note as proof of purchase for a refund.</p><p>All goods which you are returning must be unused, have all tags attached and be in their original condition (including packaging and labelling). If you use the goods or remove or tamper with any of the tags attached to the goods, you will lose your right to return the goods under our returns policy.</p><h3>Unless faulty, we are unable to offer an exchange or refund on:</h3><ul><li>jewellery worn in a piercing</li><li>items that are sealed for personal hygiene reasons where they have been unsealed</li></ul><p>Delivery and return costs are Free over 1000 kr purchase.</p><p>The above policy does not affect your statutory rights including your rights in relation to faulty goods or your right to cancel orders made online. If you wish to exercise your cancellation rights please see the \"Statutory Right to Cancel\" section below.</p><p>Complete the returns form on your despatch note and include it with the item. If you no longer have the despatch note, put the following information on a piece of paper and include it with the item:</p><ul><li>Name, Billing address and a contact telephone number</li><li>Email address</li><li>Order number</li><li>Description of the product</li></ul><h3>Reason for return using the following codes:</h3><ul><li>Faulty</li><li>Incorrect</li><li>Arrived too late</li><li>Unwanted gift</li><li>Unsuitable</li><li>Doesn't fit</li><li>Other</li></ul><h3>Repack the item as securely as possible and send it to:</h3><h4>Order Fulfilment Experts Ltd?</h4><h4>Unit A2 | Upper Nashenden Farm | Stoney Lane | Rochester | Kent | ME1 3QJ</h4><p>We are not responsible for items that are lost in transit or damaged on their way back to us. so make sure to take pictures for insurance claims.</p><p>Your refund will normally be processed within 7 working days of receiving your return but it may take slightly longer to appear on your bank statement depending on what country you are from. We will refund the cost of the item, but not the postage costs or delivery charges. This does not affect your legal or statutory rights and is subject to your right of cancellation.</p><h3>Cancellation:</h3><p>If your contract is cancelled in full, we will provide you with a refund for the item(s) within 14 days of our receipt.</p>";
                    prodInfo.created_at = DateTime.Now.ToString();
                    prodInfo.product_type = objInput.Category;
                    prodInfo.published_scope = "GLOBAL";
                    prodInfo.published_at = DateTime.Now.ToString("s");
                    prodInfo.title = HttpUtility.HtmlDecode(objInput.ProductTitle);
                    prodInfo.vendor = "OWN";
                    prodInfo.tags = "";
                    prodInfo.metafields_global_title_tag = "";
                    prodInfo.metafields_global_description_tag = "";
                    prodInfo.handle = objInput.ProductName.ToLower();
                    prodInfo.published = true;

                    prodInfo.images = new List<Shopify.Json.Image>();
                    foreach (var img in objInput.Images)
                    {
                        Shopify.Json.Image objimg = new Shopify.Json.Image();
                        objimg.src = img;
                        prodInfo.images.Add(objimg);
                    }

                    prodInfo.variants = new List<Shopify.Json.Variant>();

                    foreach (var varient in objInput.Variation)
                    {
                        Shopify.Json.Variant varientdetails = new Shopify.Json.Variant();
                        if (prodInfo.id > 0)
                        {
                            DataTable dTVar = BL.Product.GetVarientBySKU(prodInfo.id.ToString(), varient.VariationSKU);
                            long ID = 0;
                            long.TryParse(dTVar.Rows[0]["VarientID"].ToString(), out ID);
                            varientdetails.id = ID;
                        }

                        varientdetails.sku = varient.VariationSKU;
                        varientdetails.barcode = "";
                        varientdetails.option1 = varient.Option;
                        varientdetails.price = objInput.ProductPrice;
                        varientdetails.title = HttpUtility.HtmlDecode(objInput.ProductTitle).Trim();
                        varientdetails.inventory_quantity = Convert.ToInt32(varient.Qty);
                        varientdetails.fulfillment_service = "manual";
                        varientdetails.inventory_policy = "continue";
                        varientdetails.inventory_management = "shopify";
                        varientdetails.grams = "";
                        varientdetails.product_id = prodInfo.id;
                        varientdetails.requires_shipping = true;
                        prodInfo.variants.Add(varientdetails);
                    }
                }
                string tempContent = JsonConvert.SerializeObject(prodInfo);

                string content = "{";
                content += "\"product\": ";
                content += tempContent;
                content += "}";
                return content;
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
            return string.Empty;
        }
        private static APICallResponse BrowseURLToString(string URL, string ClientToken, string data, string HttpMethodType)
        {
            APICallResponse resp = new APICallResponse();
            try
            {
                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(URL);
                authRequest.Headers.Add("X-Shopify-Access-Token", ClientToken);
                authRequest.Headers.Add("bypass-multi-location-support", "true");
                authRequest.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());

                authRequest.Method = HttpMethodType;
                authRequest.ContentType = "application/json";
                if (data != null && data != "")
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (StreamWriter writer = new StreamWriter(authRequest.GetRequestStream()))
                        {
                            writer.Write(data);
                            writer.Close();
                        }
                    }
                }

                resp.RequestHeaders = authRequest.Headers;
                HttpWebResponse response = (HttpWebResponse)authRequest.GetResponse();
                string result = null;

                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    result = sr.ReadToEnd();
                    sr.Close();
                }

                resp.ResponseContent = result;
                resp.HTTPStatus = response.StatusCode;
                resp.ResponseHeaders = response.Headers;
                return resp;
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
            return resp;
        }

        private void ParseHeaders()
        {
            try
            {
                string url = "https://www.tkmaxx.com";
                HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.LoadFromBrowser(url);

                HtmlNodeCollection headerNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'level-2-navigation-container')]");
                List<string> lstSiteURLs = new List<string>();
                foreach (HtmlNode item in headerNodes)
                {
                    var links = item.Descendants("a");
                    try
                    {
                        foreach (HtmlNode l in links)
                        {
                            if (l.InnerText.Trim().ToLower().Equals("read more") || (l.InnerText.Trim().ToLower().Equals("view all") && links.Count() > 1))
                                continue;

                            lstSiteURLs.Add(l.GetAttributeValue("href", string.Empty));
                        }
                    }
                    catch (Exception exp)
                    {
                        Logger.WriteException(exp);
                    }
                }

                ParseProductLinks(lstSiteURLs);
            }
            catch (Exception exp)
            {
                Logger.WriteException(exp);
            }
        }

        void ParseProductLinks(List<string> lstSiteURLs)
        {
            foreach (string link in lstSiteURLs)
            {
                string url = $"https://www.tkmaxx.com/uk/en/{link}";
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--window-size=40,40");
                var chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
                ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions);
                driver.Navigate().GoToUrl(url);
                Thread.Sleep(10000);
                var source = driver.PageSource;
                driver.Quit();

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(source);

                string filter = "//a[@href]";
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(filter);

                List<string> lstProducts = new List<string>();
                foreach (HtmlNode item in nodes)
                {
                    string hrefValue = item.GetAttributeValue("href", string.Empty);
                    if (item.OuterHtml.Contains("c-product-card"))
                        lstProducts.Add(hrefValue);
                }
                ParseProductDetail(lstProducts);
            }
        }

        void ParseProductDetail(List<string> lstProducts)
        {
            List<ProductDetail> lstProduct = new List<ProductDetail>();
            foreach (string link in lstProducts)
            {
                try
                {
                    ProductDetail p = new ProductDetail();
                    string url = $"https://www.tkmaxx.com{link}";
                    HtmlAgilityPack.HtmlWeb web = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc = web.LoadFromBrowser(url);

                    string[] arrCat = link.Split('/');
                    if (arrCat.Length <= 1)
                        arrCat = link.Split('\\');
                    p.Category = arrCat[4];
                    p.ParentID = arrCat[arrCat.Length - 1];

                    HtmlNode node = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-details')]").FirstOrDefault();
                    p.ProductName = node.Descendants("p").FirstOrDefault().InnerText;
                    p.ProductTitle = node.Descendants("h1").FirstOrDefault().InnerText;

                    node = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-price')]").FirstOrDefault();
                    p.ProductPrice = node.Descendants("h2").FirstOrDefault().InnerText;

                    node = doc.DocumentNode.SelectNodes("//div[contains(@class, 'pdp-tabs-product-description')]").FirstOrDefault();
                    p.ProductDesc = node.InnerText;
                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'pdp-tabs-product-feature-list')]");
                    foreach (HtmlNode item in nodes)
                    {
                        foreach (HtmlNode des in item.Descendants("li"))
                        {
                            p.ProductDesc += System.Environment.NewLine + des.InnerText;
                        }

                    }


                    node = doc.DocumentNode.SelectNodes("//div[contains(@class, 'pdp-tabs-product-feature-list tabs-list-right')]").FirstOrDefault();
                    IEnumerable<HtmlNode> htmlNodes = node.Descendants("p");
                    if (htmlNodes != null && htmlNodes.Count() > 1)
                        p.DeliverDetail = htmlNodes.ToList()[1].InnerText;
                    try
                    {
                        node = doc.DocumentNode.SelectNodes("//ul[contains(@class, 'delivery-charges-list')]").FirstOrDefault();
                        if (node != null)
                        {
                            foreach (HtmlNode des in node.Descendants("p"))
                            {
                                p.DeliverDetail += System.Environment.NewLine + des.InnerText;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }

                    try
                    {
                        nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'tab-details')]");
                        if (nodes != null && nodes.Count > 2)
                        {
                            p.ReturnDetail = nodes[2].InnerText;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }

                    p.Variation = new List<Varient>();
                    node = doc.DocumentNode.SelectNodes("//select[contains(@class, 'variant-select')]").FirstOrDefault();

                    foreach (HtmlNode des in node.Descendants("option"))
                    {
                        Varient v = new Varient();
                        v.VariationSKU = des.GetAttributeValue("value", string.Empty);
                        v.Option = des.InnerText;
                        v.Qty = des.GetAttributeValue("data-stock-value", string.Empty);
                        p.Variation.Add(v);
                    }

                    try
                    {
                        nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'thumb_')]");
                        p.Images = new List<string>();
                        foreach (HtmlNode item in nodes)
                        {
                            HtmlNode des = item.Descendants("img").FirstOrDefault();
                            string imgURL = des.GetAttributeValue("src", string.Empty);
                            p.Images.Add(imgURL);

                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }

                    lstProduct.Add(p);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
            }

            foreach (var prod in lstProduct)
            {
                try
                {
                    PostProduct(prod);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }

            }
        }
    }
}
