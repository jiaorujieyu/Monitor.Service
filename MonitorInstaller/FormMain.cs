using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace MonitorInstaller
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void btn_install_Click(object sender, EventArgs e)
        {
            var apiKey = this.tb_key.Text.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("请按上面图片的指导，输入API密钥");
                return;
            }

            try
            {
                string appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonitorService");
                string dataZipPath = Path.Combine(Application.StartupPath, "data.zip");
                string installBatPath = Path.Combine(appPath, "install.bat");
                string configPath = Path.Combine(appPath, "ScreenshotHelper.exe.config");

                // 检查data.zip是否存在
                if (!File.Exists(dataZipPath))
                {
                    MessageBox.Show($"找不到安装文件：{dataZipPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 创建安装目录
                if (!Directory.Exists(appPath))
                {
                    Directory.CreateDirectory(appPath);
                }

                // 解压data.zip文件
                ExtractZipFile(dataZipPath, appPath);

                // 修改配置文件中的ApiKey
                if (File.Exists(configPath))
                {
                    UpdateConfigApiKey(configPath, apiKey);
                }
                else
                {
                    MessageBox.Show("找不到配置文件：ScreenshotHelper.exe.config", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 运行install.bat
                if (File.Exists(installBatPath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c cd /d \"{appPath}\" && install.bat",
                        WorkingDirectory = appPath,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal
                    };

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                    }

                    MessageBox.Show("安装成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("找不到安装脚本：install.bat", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"安装失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 解压ZIP文件
        /// </summary>
        private void ExtractZipFile(string zipPath, string destinationPath)
        {
            ZipFile.ExtractToDirectory(zipPath, destinationPath);
        }

        /// <summary>
        /// 更新配置文件中的ApiKey
        /// </summary>
        private void UpdateConfigApiKey(string configPath, string apiKey)
        {
            string content = File.ReadAllText(configPath);

            // 使用正则表达式替换ApiKey的值
            string pattern = @"(<add\s+key=""ApiKey""\s+value="")[^""]*(""\s*/>)";
            string replacement = $"${{1}}{apiKey}${{2}}";

            content = System.Text.RegularExpressions.Regex.Replace(content, pattern, replacement);

            File.WriteAllText(configPath, content);
        }

        private void btn_uninstall_Click(object sender, EventArgs e)
        {
            try
            {
                string appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonitorService");
                string uninstallBatPath = Path.Combine(appPath, "uninstall.bat");

                // 检查安装目录是否存在
                if (!Directory.Exists(appPath))
                {
                    MessageBox.Show("未找到已安装的程序", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 执行uninstall.bat
                if (File.Exists(uninstallBatPath))
                {
                    DialogResult result = MessageBox.Show("确定要卸载 MonitorService 吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                    {
                        return;
                    }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c cd /d \"{appPath}\" && uninstall.bat",
                        WorkingDirectory = appPath,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal
                    };

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                    }
                }

                // 移除MonitorService文件夹
                if (Directory.Exists(appPath))
                {
                    Directory.Delete(appPath, true);
                }

                MessageBox.Show("卸载成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"卸载失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            string appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonitorService");
            string configPath = Path.Combine(appPath, "ScreenshotHelper.exe.config");
            string restartBatPath = Path.Combine(appPath, "restart.bat");
            var apiKey = this.tb_key.Text.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("请按上面图片的指导，输入API密钥");
                return;
            }

            try
            {
                // 检查安装目录是否存在
                if (!Directory.Exists(appPath))
                {
                    MessageBox.Show("未找到已安装的程序", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 更新配置文件中的ApiKey
                UpdateConfigApiKey(configPath, apiKey);

                // 执行restart.bat重启服务
                if (File.Exists(restartBatPath))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c cd /d \"{appPath}\" && restart.bat",
                        WorkingDirectory = appPath,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal
                    };

                    using (Process process = Process.Start(psi))
                    {
                        process.WaitForExit();
                    }
                }

                MessageBox.Show("API密钥已重置，服务已重启！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
