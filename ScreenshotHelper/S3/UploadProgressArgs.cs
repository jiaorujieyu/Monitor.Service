using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.S3
{
    /// <summary>
    /// 上传进度事件参数
    /// </summary>
    public class UploadProgressArgs : EventArgs
    {
        public string FilePath { get; set; }
        public long TransferredBytes { get; set; }
        public long TotalBytes { get; set; }
        public double ProgressPercentage { get; set; }
    }
}
