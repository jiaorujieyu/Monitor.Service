using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.S3
{
    /// <summary>
    /// S3存储提供商类型
    /// </summary>
    public enum S3Provider
    {
        /// <summary>
        /// AWS S3
        /// </summary>
        Aws,

        /// <summary>
        /// Cloudflare R2
        /// </summary>
        CloudflareR2,

        /// <summary>
        /// MinIO
        /// </summary>
        MinIO,

        /// <summary>
        /// 其他S3兼容存储
        /// </summary>
        Other
    }
}
