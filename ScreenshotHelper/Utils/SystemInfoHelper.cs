using System;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;

namespace ScreenshotHelper.Utils
{
    /// <summary>
    /// 系统信息工具类
    /// </summary>
    public class SystemInfoHelper
    {
        #region Windows API

        [DllImport("kernel32.dll")]
        private static extern bool GetComputerNameW([Out] StringBuilder lpBuffer, ref int lpnSize);

        [DllImport("kernel32.dll")]
        private static extern bool GetUserNameW([Out] StringBuilder lpBuffer, ref int lpnSize);

        #endregion

        /// <summary>
        /// 获取计算机名称
        /// </summary>
        public static string GetComputerName()
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// 获取当前用户名称
        /// </summary>
        public static string GetUserName()
        {
            return Environment.UserName;
        }

        /// <summary>
        /// 获取操作系统名称
        /// </summary>
        public static string GetOperatingSystem()
        {
            try
            {
                var os = Environment.OSVersion;
                var platform = os.Platform;

                switch (platform)
                {
                    case PlatformID.Win32NT:
                        return "Windows NT";
                    case PlatformID.Win32Windows:
                        return "Windows 9x";
                    case PlatformID.Win32S:
                        return "Win32S";
                    case PlatformID.WinCE:
                        return "Windows CE";
                    case PlatformID.Unix:
                        return "Unix";
                    case PlatformID.MacOSX:
                        return "macOS";
                    default:
                        return "Unknown";
                }
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// 获取系统版本
        /// </summary>
        public static string GetSystemVersion()
        {
            try
            {
                // 使用WMI查询（适用于所有.NET Framework版本）
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        string version = os["Version"]?.ToString() ?? "";
                        return version;
                    }
                }
            }
            catch
            {
                // WMI查询失败
            }
            var osv = Environment.OSVersion;
            return $"{osv.Version.Major}.{osv.Version.Minor}.{osv.Version.Build}.{osv.Version.Revision}";
        }

        /// <summary>
        /// 获取系统详细版本信息（适用于Windows）
        /// </summary>
        public static string GetSystemDisplayVersion()
        {
            try
            {
                // 使用WMI查询（适用于所有.NET Framework版本）
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        string caption = os["Caption"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(caption))
                        {
                            // 清理字符串（移除不必要的部分）
                            caption = caption.Replace("Microsoft", "").Trim();
                            return caption;
                        }
                    }
                }
            }
            catch
            {
                // WMI查询失败
            }

            // 最终回退：使用Environment.OSVersion（可能不准确）
            var osVersion = Environment.OSVersion;
            return $"Windows {osVersion.Version.Major}.{osVersion.Version.Minor} (Build {osVersion.Version.Build})";
        }

        /// <summary>
        /// 获取机器码（基于CPU序列号和主板序列号生成）
        /// </summary>
        public static string GetMachineCode()
        {
            try
            {
                string cpuId = GetCpuId();
                string biosSerial = GetBiosSerialNumber();

                // 组合硬件信息生成机器码
                string combinedInfo = $"{cpuId}_{biosSerial}_{GetMacAddress()}";

                // 使用MD5生成唯一标识
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(combinedInfo));
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2").ToUpper());
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                // 如果获取硬件信息失败，使用备用方法
                return GenerateFallbackMachineCode();
            }
        }

        /// <summary>
        /// 获取CPU ID
        /// </summary>
        private static string GetCpuId()
        {
            try
            {
                string cpuInfo = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["ProcessorId"].Value?.ToString() ?? string.Empty;
                    break;
                }

                return cpuInfo;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取BIOS序列号
        /// </summary>
        private static string GetBiosSerialNumber()
        {
            try
            {
                string serialNumber = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_BIOS");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    serialNumber = mo.Properties["SerialNumber"].Value?.ToString() ?? string.Empty;
                    break;
                }

                return serialNumber;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取MAC地址
        /// </summary>
        private static string GetMacAddress()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    // 获取第一个物理网卡（非虚拟网卡）的MAC地址
                    if ((bool)mo["IPEnabled"])
                    {
                        string mac = mo["MacAddress"]?.ToString();
                        if (!string.IsNullOrEmpty(mac))
                        {
                            return mac.Replace(":", "");
                        }
                    }
                }

                return string.Empty;
            }
            catch
            {
                // 备用方法：获取本机IP地址
                try
                {
                    string hostName = Dns.GetHostName();
                    IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
                    foreach (IPAddress ip in ipAddresses)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ip.ToString();
                        }
                    }
                }
                catch
                {
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 生成备用机器码（当无法获取硬件信息时使用）
        /// </summary>
        private static string GenerateFallbackMachineCode()
        {
            try
            {
                string combinedInfo = $"{GetComputerName()}_{Environment.UserName}_{GetSystemVersion()}";

                using (MD5 md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(combinedInfo));
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2").ToUpper());
                    }
                    return sb.ToString();
                }
            }
            catch
            {
                return Guid.NewGuid().ToString("N").ToUpper();
            }
        }

        /// <summary>
        /// 获取系统信息摘要
        /// </summary>
        public static string GetSystemInfoSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"计算机名称: {GetComputerName()}");
            sb.AppendLine($"当前用户: {GetUserName()}");
            sb.AppendLine($"操作系统: {GetOperatingSystem()}");
            sb.AppendLine($"系统版本: {GetSystemDisplayVersion()}");
            sb.AppendLine($"机器码: {GetMachineCode()}");
            return sb.ToString();
        }

        /// <summary>
        /// 获取完整系统信息对象
        /// </summary>
        public static SystemInfo GetSystemInfo()
        {
            return new SystemInfo
            {
                ComputerName = GetComputerName(),
                UserName = GetUserName(),
                OperatingSystem = GetOperatingSystem(),
                SystemVersion = GetSystemVersion(),
                DisplayVersion = GetSystemDisplayVersion(),
                MachineCode = GetMachineCode()
            };
        }

        /// <summary>
        /// 获取系统架构（x86/x64）
        /// </summary>
        public static string GetSystemArchitecture()
        {
            return Environment.Is64BitOperatingSystem ? "x64" : "x86";
        }

        /// <summary>
        /// 获取进程架构（x86/x64）
        /// </summary>
        public static string GetProcessArchitecture()
        {
            return Environment.Is64BitProcess ? "x64" : "x86";
        }

        /// <summary>
        /// 获取处理器数量
        /// </summary>
        public static int GetProcessorCount()
        {
            return Environment.ProcessorCount;
        }

        /// <summary>
        /// 获取系统启动时间
        /// </summary>
        public static DateTime GetSystemBootTime()
        {
            try
            {
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                return DateTime.Now - uptime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }

    /// <summary>
    /// 系统信息实体类
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// 计算机名称
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// 系统版本
        /// </summary>
        public string SystemVersion { get; set; }

        /// <summary>
        /// 显示版本
        /// </summary>
        public string DisplayVersion { get; set; }

        /// <summary>
        /// 机器码
        /// </summary>
        public string MachineCode { get; set; }
    }
}
