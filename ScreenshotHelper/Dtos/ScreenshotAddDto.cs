using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.Dtos
{
    /// <summary>
    /// 截图添加DTO
    /// </summary>
    public class ScreenshotAddDto
    {
        /// <summary>
        /// 客户端ID
        /// </summary>

        public int ClientId { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// OCR内容
        /// </summary>
        public string OcrContent { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime ScreenshotDate { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
    }
}
