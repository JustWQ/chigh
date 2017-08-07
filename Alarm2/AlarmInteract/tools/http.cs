using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SCSV.AlarmInteract
{
    /// <summary>    
    /// 上传数据参数    
    /// </summary>    
    public class UploadEventArgs : EventArgs
    {
        int bytesSent;
        int totalBytes;
        /// <summary>    
        /// 已发送的字节数    
        /// </summary>    
        public int BytesSent
        {
            get { return bytesSent; }
            set { bytesSent = value; }
        }
        /// <summary>    
        /// 总字节数    
        /// </summary>    
        public int TotalBytes
        {
            get { return totalBytes; }
            set { totalBytes = value; }
        }
    }
    /// <summary>    
    /// 下载数据参数    
    /// </summary>    
    public class DownloadEventArgs : EventArgs
    {
        int bytesReceived;
        int totalBytes;
        byte[] receivedData;
        /// <summary>    
        /// 已接收的字节数    
        /// </summary>    
        public int BytesReceived
        {
            get { return bytesReceived; }
            set { bytesReceived = value; }
        }
        /// <summary>    
        /// 总字节数    
        /// </summary>    
        public int TotalBytes
        {
            get { return totalBytes; }
            set { totalBytes = value; }
        }
        /// <summary>    
        /// 当前缓冲区接收的数据    
        /// </summary>    
        public byte[] ReceivedData
        {
            get { return receivedData; }
            set { receivedData = value; }
        }
    }

    public class HttpProc
    {
        Encoding encoding = Encoding.UTF8;
        string respHtml = "";
        WebProxy proxy;
        static CookieContainer cc;
        /// <summary>
        /// 请求头
        /// </summary>
        WebHeaderCollection requestHeaders;
        /// <summary>
        /// 返回头
        /// </summary>
        WebHeaderCollection responseHeaders;
        int bufferSize = 15240;
        public event EventHandler<UploadEventArgs> UploadProgressChanged;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;
     
        /// <summary>    
        /// 创建WebClient的实例    
        /// </summary>    
        public HttpProc()
        {
            requestHeaders = new WebHeaderCollection();
            responseHeaders = new WebHeaderCollection();
        }
        
        /// <summary>    
        /// 获取响应头集合    
        /// </summary>    
        public WebHeaderCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }

        /// <summary>    
        /// 向指定URL发送字节数据    
        /// </summary>    
        /// <param name="url">网址</param>    
        /// <param name="postData">发送的数据</param>     
        public string Post(string url, string postData)
        {
            try
            {
                HttpWebRequest request = CreateRequest(url, "POST");
                //  request.ContentType = "application/x-www-form-urlencoded";
                request.ContentType = "application/json";
                request.ContentLength = postData.Length;
                request.KeepAlive = true;
                PostData(request, encoding.GetBytes(postData));
                respHtml = encoding.GetString(GetData(request));
                return respHtml;
            }
            catch (Exception ex)
            {
                if (ex.GetType().ToString() == "System.Net.WebException")
                {
                    HttpWebResponse myResponse = (HttpWebResponse)((System.Net.WebException)ex).Response;
                    if (myResponse != null)
                    {
                        Stream errData = myResponse.GetResponseStream();
                        StreamReader reader = new StreamReader(errData);
                        Logger.Log.ErrorFormat("{0} {1}", new Object[] { url, reader.ReadToEnd() });
                    }
                    else
                    {
                        Logger.Log.ErrorFormat("{0} {1}", new Object[] { url, ex.Message });
                    }
                }
            }

            return null;
        }

        /// <summary>    
        /// 读取请求返回的数据    
        /// </summary>    
        /// <param name="request">请求对象</param>      
        private byte[] GetData(HttpWebRequest request)
        {
            // 获取远程响应
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();

            Stream stream = response.GetResponseStream();
            responseHeaders = response.Headers;

            DownloadEventArgs args = new DownloadEventArgs();
            if (responseHeaders[HttpResponseHeader.ContentLength] != null)
                args.TotalBytes = Convert.ToInt32(responseHeaders[HttpResponseHeader.ContentLength]);

            MemoryStream ms = new MemoryStream();
            int count = 0;
            byte[] buf = new byte[bufferSize];
            while ((count = stream.Read(buf, 0, buf.Length)) > 0)
            {
                ms.Write(buf, 0, count);
                if (this.DownloadProgressChanged != null)
                {
                    args.BytesReceived += count;
                    args.ReceivedData = new byte[count];
                    Array.Copy(buf, args.ReceivedData, count);
                    this.DownloadProgressChanged(this, args);
                }
            }
            stream.Close();
            #region 解压    
            if (ResponseHeaders[HttpResponseHeader.ContentEncoding] != null)
            {
                MemoryStream msTemp = new MemoryStream();
                count = 0;
                buf = new byte[100];
                switch (ResponseHeaders[HttpResponseHeader.ContentEncoding].ToLower())
                {
                    case "gzip":
                        GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress);
                        while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                        {
                            msTemp.Write(buf, 0, count);
                        }
                        return msTemp.ToArray();
                    case "deflate":
                        DeflateStream deflate = new DeflateStream(ms, CompressionMode.Decompress);
                        while ((count = deflate.Read(buf, 0, buf.Length)) > 0)
                        {
                            msTemp.Write(buf, 0, count);
                        }
                        return msTemp.ToArray();
                    default:
                        break;
                }
            }
            #endregion
            return ms.ToArray();
        }

        /// <summary>    
        /// 发送请求数据    
        /// </summary>    
        /// <param name="request">请求对象</param>    
        /// <param name="postData">请求发送的字节数组</param>    
        private void PostData(HttpWebRequest request, byte[] postData)
        {
            int offset = 0;
            int sendBufferSize = bufferSize;
            int remainBytes = 0;
            Stream stream = request.GetRequestStream();
            UploadEventArgs args = new UploadEventArgs();
            args.TotalBytes = postData.Length;
            while ((remainBytes = postData.Length - offset) > 0)
            {
                if (sendBufferSize > remainBytes) sendBufferSize = remainBytes;
                stream.Write(postData, offset, sendBufferSize);
                offset += sendBufferSize;
                if (this.UploadProgressChanged != null)
                {
                    args.BytesSent = offset;
                    this.UploadProgressChanged(this, args);
                }
            }
            stream.Close();
        }
        /// <summary>    
        /// 创建HTTP请求    
        /// </summary>    
        /// <param name="url">URL地址</param>     
        private HttpWebRequest CreateRequest(string url, string method)
        {
            Uri uri = new Uri(url);

            if (uri.Scheme == "https")
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(this.CheckValidationResult);

            // Set a default policy level for the "http:" and "https" schemes.    
            HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            HttpWebRequest.DefaultCachePolicy = policy;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = false;
            request.AllowWriteStreamBuffering = false;
            request.Method = method;
            if (proxy != null)
                request.Proxy = proxy;
            request.CookieContainer = cc;
            foreach (string key in requestHeaders.Keys)
            {
                request.Headers.Add(key, requestHeaders[key]);
            }
            requestHeaders.Clear();
            return request;
        }
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
    }
}
