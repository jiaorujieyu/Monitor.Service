using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace ScreenshotHelper.Utils
{
    /// <summary>
    /// HTTP请求工具类
    /// </summary>
    public class HttpClientHelper
    {
        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// 默认请求头
        /// </summary>
        public Dictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpClientHelper() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="timeout">超时时间（毫秒）</param>
        public HttpClientHelper(int timeout)
        {
            Timeout = timeout;
        }

        /// <summary>
        /// 执行GET请求
        /// </summary>
        public HttpResponse Get(string url, Dictionary<string, string> headers = null)
        {
            return Get(url, null, headers);
        }

        /// <summary>
        /// 执行GET请求（泛型，返回JSON反序列化对象）
        /// </summary>
        public T Get<T>(string url, Dictionary<string, string> headers = null) where T : class
        {
            HttpResponse response = Get(url, headers);
            return response.ParseJson<T>();
        }

        /// <summary>
        /// 执行GET请求
        /// </summary>
        public HttpResponse Get(string url, Dictionary<string, string> queryParams, Dictionary<string, string> headers = null)
        {
            try
            {
                // 构建完整URL
                string fullUrl = url;
                if (queryParams != null && queryParams.Count > 0)
                {
                    fullUrl += "?" + BuildQueryString(queryParams);
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
                request.Method = "GET";
                request.Timeout = Timeout;

                // 添加默认请求头
                AddDefaultHeaders(request);
                // 添加自定义请求头
                AddCustomHeaders(request, headers);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return new HttpResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return new HttpResponse((HttpWebResponse)ex.Response);
                }
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 执行POST请求（表单数据）
        /// </summary>
        public HttpResponse Post(string url, Dictionary<string, string> formData, Dictionary<string, string> headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = Timeout;
                request.ContentType = "application/x-www-form-urlencoded";

                // 添加默认请求头
                AddDefaultHeaders(request);
                // 添加自定义请求头
                AddCustomHeaders(request, headers);

                // 添加表单数据
                if (formData != null && formData.Count > 0)
                {
                    byte[] data = Encoding.UTF8.GetBytes(BuildQueryString(formData));
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return new HttpResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return new HttpResponse((HttpWebResponse)ex.Response);
                }
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 执行POST请求（JSON数据）
        /// </summary>
        public HttpResponse PostJson(string url, string jsonData, Dictionary<string, string> headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = Timeout;
                request.ContentType = "application/json";

                // 添加默认请求头
                AddDefaultHeaders(request);
                // 添加自定义请求头
                AddCustomHeaders(request, headers);

                // 添加JSON数据
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return new HttpResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return new HttpResponse((HttpWebResponse)ex.Response);
                }
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 执行POST请求（表单数据，泛型返回）
        /// </summary>
        public T Post<T>(string url, Dictionary<string, string> formData, Dictionary<string, string> headers = null) where T : class
        {
            HttpResponse response = Post(url, formData, headers);
            return response.ParseJson<T>();
        }

        /// <summary>
        /// 执行POST请求（JSON数据，泛型返回）
        /// </summary>
        public T PostJson<T>(string url, string jsonData, Dictionary<string, string> headers = null) where T : class
        {
            HttpResponse response = PostJson(url, jsonData, headers);
            return response.ParseJson<T>();
        }

        /// <summary>
        /// 执行POST请求（对象序列化为JSON，泛型返回）
        /// </summary>
        public HttpResponse PostJson<TRequest>(string url, TRequest requestData, Dictionary<string, string> headers = null)
        {
            string jsonData = JsonConvert.SerializeObject(requestData);
            return PostJson(url, jsonData, headers);
        }

        /// <summary>
        /// 执行POST请求（对象序列化为JSON，泛型返回）
        /// </summary>
        public TResponse PostJson<TRequest, TResponse>(string url, TRequest requestData, Dictionary<string, string> headers = null)
            where TResponse : class
        {
            string jsonData = JsonConvert.SerializeObject(requestData);
            HttpResponse response = PostJson(url, jsonData, headers);
            return response.ParseJson<TResponse>();
        }

        /// <summary>
        /// 执行POST请求（原始数据）
        /// </summary>
        public HttpResponse Post(string url, byte[] data, string contentType, Dictionary<string, string> headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = Timeout;
                request.ContentType = contentType;

                // 添加默认请求头
                AddDefaultHeaders(request);
                // 添加自定义请求头
                AddCustomHeaders(request, headers);

                // 添加数据
                if (data != null && data.Length > 0)
                {
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return new HttpResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return new HttpResponse((HttpWebResponse)ex.Response);
                }
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 执行文件上传（Multipart）
        /// </summary>
        public HttpResponse UploadFile(string url, string filePath, string fileKey = "file", Dictionary<string, string> formData = null, Dictionary<string, string> headers = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"文件不存在: {filePath}");
                }

                string boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = Timeout;
                request.ContentType = "multipart/form-data; boundary=" + boundary;

                // 添加默认请求头
                AddDefaultHeaders(request);
                // 添加自定义请求头
                AddCustomHeaders(request, headers);

                using (var stream = request.GetRequestStream())
                {
                    // 添加表单数据
                    if (formData != null)
                    {
                        foreach (var kvp in formData)
                        {
                            string formDataHeader = "--" + boundary + "\r\n" +
                                                  "Content-Disposition: form-data; name=\"" + kvp.Key + "\"\r\n\r\n";
                            byte[] formDataHeaderBytes = Encoding.UTF8.GetBytes(formDataHeader);
                            stream.Write(formDataHeaderBytes, 0, formDataHeaderBytes.Length);
                            byte[] formDataValueBytes = Encoding.UTF8.GetBytes(kvp.Value + "\r\n");
                            stream.Write(formDataValueBytes, 0, formDataValueBytes.Length);
                        }
                    }

                    // 添加文件
                    byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    string fileName = Path.GetFileName(filePath);
                    string fileHeader = "--" + boundary + "\r\n" +
                                       "Content-Disposition: form-data; name=\"" + fileKey + "\"; filename=\"" + fileName + "\"\r\n" +
                                       "Content-Type: " + GetContentType(fileName) + "\r\n\r\n";
                    byte[] fileHeaderBytes = Encoding.UTF8.GetBytes(fileHeader);
                    stream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

                    // 写入文件内容
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            stream.Write(buffer, 0, bytesRead);
                        }
                    }

                    // 结束边界
                    byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    stream.Write(trailer, 0, trailer.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return new HttpResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return new HttpResponse((HttpWebResponse)ex.Response);
                }
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 执行PUT请求
        /// </summary>
        public HttpResponse Put(string url, string jsonData, Dictionary<string, string> headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "PUT";
                request.Timeout = Timeout;
                request.ContentType = "application/json";

                // 添加默认请求头
                AddDefaultHeaders(request);
                // 添加自定义请求头
                AddCustomHeaders(request, headers);

                // 添加JSON数据
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return new HttpResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return new HttpResponse((HttpWebResponse)ex.Response);
                }
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 执行PUT请求（泛型返回）
        /// </summary>
        public T Put<T>(string url, string jsonData, Dictionary<string, string> headers = null) where T : class
        {
            HttpResponse response = Put(url, jsonData, headers);
            return response.ParseJson<T>();
        }

        /// <summary>
        /// 执行PUT请求（对象序列化为JSON，泛型返回）
        /// </summary>
        public TResponse Put<TRequest, TResponse>(string url, TRequest requestData, Dictionary<string, string> headers = null)
            where TResponse : class
        {
            string jsonData = JsonConvert.SerializeObject(requestData);
            HttpResponse response = Put(url, jsonData, headers);
            return response.ParseJson<TResponse>();
        }

        /// <summary>
        /// 执行DELETE请求
        /// </summary>
        public HttpResponse Delete(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "DELETE";
                request.Timeout = Timeout;

                // 添加默认请求头
                AddDefaultHeaders(request);
                // 添加自定义请求头
                AddCustomHeaders(request, headers);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return new HttpResponse(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    return new HttpResponse((HttpWebResponse)ex.Response);
                }
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 执行DELETE请求（泛型返回）
        /// </summary>
        public T Delete<T>(string url, Dictionary<string, string> headers = null) where T : class
        {
            HttpResponse response = Delete(url, headers);
            return response.ParseJson<T>();
        }

        /// <summary>
        /// 添加默认请求头
        /// </summary>
        private void AddDefaultHeaders(HttpWebRequest request)
        {
            if (DefaultHeaders != null)
            {
                foreach (var header in DefaultHeaders)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
        }

        /// <summary>
        /// 添加自定义请求头
        /// </summary>
        private void AddCustomHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
        }

        /// <summary>
        /// 构建查询字符串
        /// </summary>
        private string BuildQueryString(Dictionary<string, string> parameters)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;

            foreach (var kvp in parameters)
            {
                if (!first)
                {
                    sb.Append("&");
                }
                first = false;
                sb.Append(Uri.EscapeDataString(kvp.Key));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(kvp.Value));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 根据文件扩展名获取ContentType
        /// </summary>
        private string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".txt":
                    return "text/plain";
                case ".html":
                case ".htm":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".json":
                    return "application/json";
                case ".xml":
                    return "application/xml";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".svg":
                    return "image/svg+xml";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/zip";
                case ".rar":
                    return "application/x-rar-compressed";
                case ".7z":
                    return "application/x-7z-compressed";
                case ".mp3":
                    return "audio/mpeg";
                case ".mp4":
                    return "video/mp4";
                case ".avi":
                    return "video/x-msvideo";
                case ".mov":
                    return "video/quicktime";
                default:
                    return "application/octet-stream";
            }
        }
    }

    /// <summary>
    /// HTTP响应
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public System.Net.HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应内容（文本）
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 响应内容（字节数组）
        /// </summary>
        public byte[] ContentBytes { get; set; }

        /// <summary>
        /// 响应头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpResponse()
        {
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// 构造函数（从HttpWebResponse）
        /// </summary>
        public HttpResponse(HttpWebResponse response)
        {
            StatusCode = response.StatusCode;
            Success = (int)StatusCode >= 200 && (int)StatusCode < 300;

            // 读取响应头
            Headers = new Dictionary<string, string>();
            foreach (var key in response.Headers.AllKeys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    Headers[key] = response.Headers[key];
                }
            }

            // 读取响应内容
            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                responseStream.CopyTo(memoryStream);
                ContentBytes = memoryStream.ToArray();
                Content = Encoding.UTF8.GetString(ContentBytes);
            }
        }

        /// <summary>
        /// 获取指定响应头
        /// </summary>
        public string GetHeader(string name)
        {
            if (Headers.ContainsKey(name))
            {
                return Headers[name];
            }
            return null;
        }

        /// <summary>
        /// 尝试获取指定响应头
        /// </summary>
        public bool TryGetHeader(string name, out string value)
        {
            return Headers.TryGetValue(name, out value);
        }

        /// <summary>
        /// 解析JSON响应为指定类型
        /// </summary>
        public T ParseJson<T>() where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(Content))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<T>(Content);
            }
            catch
            {
                return null;
            }
        }
    }
}
