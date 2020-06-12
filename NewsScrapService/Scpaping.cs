using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewsScrapService
{
    public class Scraping
    {
        public Scraping() { }

        public static string Scrap(string url, Method method, List<Querystring> paramList)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            string resultText = string.Empty;
            string resResult = string.Empty;

            try
            {
                string cookie = string.Empty;

                Uri uri = new Uri(url); // string 을 Uri 로 형변환
                HttpWebRequest wReq = (HttpWebRequest)WebRequest.Create(uri); // WebRequest 객체 형성 및 HttpWebRequest 로 형변환
                wReq.Method = method.ToString(); // 전송 방법 "GET" or "POST"
                wReq.UserAgent = "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 73.0.3683.103 Safari / 537.36";
                //"Mozilla /4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
                //"Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
                //".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
                //"InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";

                wReq.Accept = "text/html, */*; q=0.01";
                
                wReq.ServicePoint.Expect100Continue = false;
                wReq.CookieContainer = new CookieContainer();
                wReq.CookieContainer.SetCookies(uri, cookie); // 넘겨줄 쿠키가 있을때 CookiContainer 에 저장

                // post 파라미터 처리
                if (method == Method.POST)
                {
                    // wReq.ContentType = "application/json";  // 파라미터 타입
                    wReq.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                    //byte[] byteArray = Encoding.UTF8.GetBytes("{ idkey: \"5M4240\", gd_seq: \"GD84\", yyyymmdd: \"20190425\", sd_date: \"20190425\" }");
                    byte[] byteArray = Encoding.UTF8.GetBytes("idkey=5M4240&gd_seq=GD84&yyyymmdd=20190524&sd_date=20190524");
                    //byte[] byteArray = Encoding.UTF8.GetBytes(MakeJsonString(paramList));
                    wReq.ContentLength = byteArray.Length; // 바이트수 지정

                    using (Stream dataStream = wReq.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                    }
                }

                // response 처리
                using (HttpWebResponse wRes = (HttpWebResponse)wReq.GetResponse())
                {
                    HttpStatusCode statusCode = wRes.StatusCode;
                    Console.WriteLine(" statusCode : {0}", statusCode);

                    Stream respPostStream = wRes.GetResponseStream();
                    using (StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("UTF-8"), true))
                    {
                        resResult = readerPost.ReadToEnd();
                    }
                }

                Console.WriteLine(resResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);
                resResult = ex.Message;
            }


            return resResult;
        }

        public static string RequestUrlMsdn(string url, Method method, List<Querystring> paramList)
        {
            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url);
            // Set the Method property of the request to POST.
            request.Method = method.ToString();
            // Create POST data and convert it to a byte array.
            string postData = MakeQueryString(paramList);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            return null;
        }

        public static async Task<string> RequestHttpClient(string url, Method method, List<Querystring> paramList)
        {
            HttpClient client = new HttpClient();

            string returnValue = string.Empty;
            if (method == Method.POST)
            {
                var values = new Dictionary<string, string>();
                foreach (var data in paramList)
                {
                    values.Add(data.Name, data.Value);
                }

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                returnValue = responseString;
            }
            else
            {
                var paramString = MakeQueryString(paramList);
                if (string.IsNullOrEmpty(paramString) == false)
                    url = string.Format("{0}?{1}", url, MakeQueryString(paramList));
                
                var responseString = await client.GetStringAsync(url);
                returnValue = responseString;
            }

            return returnValue;
        }

        private static string MakeJsonString(List<Querystring> paramList)
        {
            StringBuilder data = new StringBuilder();

            if (paramList == null || paramList.Count == 0)
                return data.ToString();

            int count = 1;
            data.Append("{ ");
            foreach (var qs in paramList)
            {
                if (count > 1)
                {
                    data.Append(",");
                }
                data.Append(string.Format("\"{0}\":\"{1}\"", qs.Name, qs.Value));
                count++;
            }
            data.Append(" }");

            return data.ToString();
        }

        private static string MakeQueryString(List<Querystring> paramList)
        {
            StringBuilder data = new StringBuilder();

            if (paramList == null || paramList.Count == 0)
                return data.ToString();

            int count = 1;

            foreach (var qs in paramList)
            {
                if (count > 1)
                {
                    data.Append("&");
                }
                data.Append(string.Format("{0}={1}", qs.Name, qs.Value));
                count++;
            }

            return data.ToString();
        }

        #region 인코딩
        private static String StringToEncoding(String str)
        {
            Encoding encode = System.Text.Encoding.GetEncoding("euc-kr");
            byte[] byteencode = encode.GetBytes(str);

            return encode.GetString(byteencode, 0, byteencode.Length);
        }

        private static String StringToEncoding(byte[] byteencode)
        {
            Encoding encode = System.Text.Encoding.GetEncoding("euc-kr");

            return encode.GetString(byteencode, 0, byteencode.Length);
        }
        #endregion
    }

    public enum Method { GET = 1, POST = 2 }

    public class Querystring
    {
        public Querystring(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}
