/*
 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * Author : Alaa Ben Fatma
 * Email : alaabenfatma@yahoo.fr
 * ----------------------------------------------------------------------
 */

using System.Diagnostics;
using System.Net;


namespace SQL注入
{
    public class Tools
    {
        /// <summary>
        /// Check if the webpage is realy infected
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool Infected(string url)
        {
            string html = "";
            try
            {
                html = new WebClient().DownloadString(url + " '");
            }
            catch
            {
                // ignored
            }
            return html.Contains("You have an error in your SQL syntax;");
        }
        /// <summary>
        /// Send Commands to the target
        /// </summary>
        /// <param name="url"></param>
        /// <param name="command"></param>
        public void SendCommand(string url, string command)
        {
            url = url + command;
            if (true) Process.Start(url);
        }
        /// <summary>
        /// Read the source code of the webpage
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string ReadContent(string url)
        {
            string html = new WebClient().DownloadString(url);
            return html;
        }
        /// <summary>
        /// Get the IP of the website
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetIp(string url)
        {
            var
            address = Dns.GetHostAddresses(url)[0].ToString();
            return address;
        }
        /// <summary>
        /// Check if the web page exists
        /// </summary>
        /// <param name="url">The target</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool CheckExistence(string url, int timeout)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //Setting the Request method HEAD, you can also use GET too.
            Debug.Assert(request != null, "request != null");
            request.Method = "HEAD";
            request.Timeout = timeout;
            //Getting the Web Response.
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response != null)
                return true;
            else
                return false;
        }
    }
}
