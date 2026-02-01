using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.S3
{
    /// <summary>
    /// S3配置
    /// </summary>
    public class S3Config
    {
        /// <summary>
        /// 服务端点URL
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// 访问密钥ID
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// 秘密访问密钥
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// 区域（对于AWS是必需的）
        /// </summary>
        public string Region { get; set; } = "auto";

        /// <summary>
        /// 存储桶名称
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// 是否使用HTTPS
        /// </summary>
        public bool UseHttps { get; set; } = true;

        /// <summary>
        /// 是否强制使用路径样式访问
        /// </summary>
        public bool ForcePathStyle { get; set; } = true;

        /// <summary>
        /// 提供商类型
        /// </summary>
        public S3Provider Provider { get; set; }
    }
}
