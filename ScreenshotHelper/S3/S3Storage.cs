using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScreenshotHelper.S3
{
    /// <summary>
    /// 通用S3存储操作类
    /// </summary>
    public class S3Storage : IDisposable
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Config _config;
        private bool _disposed;

        /// <summary>
        /// 上传进度事件
        /// </summary>
        public event EventHandler<UploadProgressArgs> UploadProgress;

        /// <summary>
        /// 构造函数
        /// </summary>
        public S3Storage(S3Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _s3Client = CreateS3Client(config);
        }

        /// <summary>
        /// 创建S3客户端
        /// </summary>
        private IAmazonS3 CreateS3Client(S3Config config)
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = config.ServiceUrl,
                ForcePathStyle = config.ForcePathStyle,
                UseHttp = !config.UseHttps,
                Timeout = TimeSpan.FromMinutes(5),
                MaxErrorRetry = 3
            };

            // 根据不同提供商进行特殊配置
            switch (config.Provider)
            {
                case S3Provider.Aws:
                    // AWS S3使用区域端点
                    if (!string.IsNullOrEmpty(config.Region))
                    {
                        s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region);
                        s3Config.ServiceURL = null; // AWS SDK会自动生成端点
                    }
                    break;

                case S3Provider.CloudflareR2:
                    // Cloudflare R2特殊配置
                    //s3Config.SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA256;
                    var credentials = new BasicAWSCredentials(config.AccessKey, config.SecretKey);
                    return new AmazonS3Client(credentials, s3Config);

                case S3Provider.MinIO:
                    // MinIO通常需要路径样式访问
                    s3Config.ForcePathStyle = true;
                    break;

                case S3Provider.Other:
                    // 其他S3兼容服务
                    break;
            }

            return new AmazonS3Client(
                config.AccessKey,
                config.SecretKey,
                s3Config
            );
        }

        /// <summary>
        /// 创建存储桶
        /// </summary>
        public void CreateBucket(string bucketName = null)
        {
            if (bucketName == null)
            {
                bucketName = _config.BucketName;
            }

            var request = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            _s3Client.PutBucket(request);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        public void UploadFile(string localFilePath, string s3Key = null, string contentType = null, Dictionary<string, string> metadata = null)
        {
            if (string.IsNullOrEmpty(localFilePath))
                throw new ArgumentNullException(nameof(localFilePath));

            if (!File.Exists(localFilePath))
                throw new FileNotFoundException($"文件不存在: {localFilePath}");
            if (s3Key == null)
            {
                s3Key = Path.GetFileName(localFilePath);
            }

            using (var fileTransferUtility = new TransferUtility(_s3Client))
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _config.BucketName,
                    Key = s3Key,
                    FilePath = localFilePath,
                    ContentType = contentType ?? GetContentType(localFilePath),
                    CannedACL = S3CannedACL.Private,
                    StorageClass = S3StorageClass.Standard,
                    PartSize = 16 * 1024 * 1024 // 16MB分片
                };

                // 添加元数据
                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        uploadRequest.Metadata.Add(kvp.Key, kvp.Value);
                    }
                }

                // 进度事件处理
                uploadRequest.UploadProgressEvent += (sender, e) =>
                {
                    UploadProgress?.Invoke(this, new UploadProgressArgs
                    {
                        FilePath = localFilePath,
                        TransferredBytes = e.TransferredBytes,
                        TotalBytes = e.TotalBytes,
                        ProgressPercentage = (double)e.TransferredBytes / e.TotalBytes * 100
                    });
                };

                fileTransferUtility.Upload(uploadRequest);
            }
        }

        /// <summary>
        /// 上传流
        /// </summary>
        public PutObjectResponse UploadStream(Stream stream, string s3Key, string contentType = "application/octet-stream", Dictionary<string, string> metadata = null)
        {
            var request = new PutObjectRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key,
                InputStream = stream,
                ContentType = contentType,
                CannedACL = S3CannedACL.Private,
                StorageClass = S3StorageClass.Standard,
                DisablePayloadSigning = true
            };

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    request.Metadata.Add(kvp.Key, kvp.Value);
                }
            }

            return _s3Client.PutObject(request);
        }

        public PutObjectResponse UploadBytes(byte[] data, string s3Key, string contentType = "application/octet-stream", Dictionary<string, string> metadata = null)
        {
            using (var stream = new MemoryStream(data))
            {
                return UploadStream(stream, s3Key, contentType, metadata);
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public void DownloadFile(string s3Key, string localFilePath)
        {
            var request = new GetObjectRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key
            };

            using (var response = _s3Client.GetObject(request))
            {
                using (var fileStream = File.Create(localFilePath))
                {
                    response.ResponseStream.CopyTo(fileStream);
                }
            }
        }

        /// <summary>
        /// 下载为流
        /// </summary>
        public Stream DownloadStream(string s3Key)
        {
            var request = new GetObjectRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key
            };

            return _s3Client.GetObject(request).ResponseStream;
        }

        /// <summary>
        /// 下载为字节数组
        /// </summary>
        public byte[] DownloadBytes(string s3Key)
        {
            using (var stream = DownloadStream(s3Key))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public void DeleteFile(string s3Key)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key
            };

            _s3Client.DeleteObject(request);
        }

        /// <summary>
        /// 批量删除文件
        /// </summary>
        public void DeleteFiles(List<string> s3Keys)
        {
            if (s3Keys == null || s3Keys.Count == 0)
                return;

            // S3批量删除最多支持1000个对象
            const int batchSize = 1000;

            for (int i = 0; i < s3Keys.Count; i += batchSize)
            {
                var batch = s3Keys.Skip(i).Take(batchSize).ToList();

                var request = new DeleteObjectsRequest
                {
                    BucketName = _config.BucketName,
                    Objects = batch.Select(key => new KeyVersion { Key = key }).ToList()
                };

                _s3Client.DeleteObjects(request);
            }
        }

        /// <summary>
        /// 列出文件
        /// </summary>
        public List<S3Object> ListFiles(string prefix = null, int maxKeys = 1000)
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _config.BucketName,
                Prefix = prefix,
                MaxKeys = maxKeys
            };

            var response = _s3Client.ListObjectsV2(request);
            return response.S3Objects;
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool FileExists(string s3Key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _config.BucketName,
                    Key = s3Key
                };

                _s3Client.GetObjectMetadata(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取文件URL（预签名URL，可用于临时访问）
        /// </summary>
        public string GetPreSignedUrl(string s3Key,DateTime expiration,ResponseHeaderOverrides headers = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key,
                Expires = expiration,
                Verb = HttpVerb.GET,
                ResponseHeaderOverrides = headers
            };

            return _s3Client.GetPreSignedURL(request);
        }

        /// <summary>
        /// 获取上传URL（预签名URL，可用于客户端直传）
        /// </summary>
        public string GetUploadPreSignedUrl(string s3Key,DateTime expiration,string contentType = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key,
                Expires = expiration,
                Verb = HttpVerb.PUT
            };

            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;
            }

            return _s3Client.GetPreSignedURL(request);
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        public void CopyFile(string sourceKey, string destinationKey, string destinationBucket = null)
        {
            if (destinationBucket == null)
            {
                destinationBucket = _config.BucketName;
            }

            var request = new CopyObjectRequest
            {
                SourceBucket = _config.BucketName,
                SourceKey = sourceKey,
                DestinationBucket = destinationBucket,
                DestinationKey = destinationKey,
                CannedACL = S3CannedACL.Private
            };

            _s3Client.CopyObject(request);
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        public GetObjectMetadataResponse GetFileInfo(string s3Key)
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key
            };

            return _s3Client.GetObjectMetadata(request);
        }

        /// <summary>
        /// 设置文件ACL
        /// </summary>
        public void SetFileAcl(string s3Key, S3CannedACL acl)
        {
            var request = new PutACLRequest
            {
                BucketName = _config.BucketName,
                Key = s3Key,
                CannedACL = acl
            };

            _s3Client.PutACL(request);
        }

        /// <summary>
        /// 根据文件扩展名获取ContentType
        /// </summary>
        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (extension)
            {
                case ".txt":
                    return "text/plain";
                case ".html":
                    return "text/html";
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
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".ppt":
                    return "application/vnd.ms-powerpoint";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
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
                case ".wmv":
                    return "video/x-ms-wmv";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                ListFiles(maxKeys: 1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _s3Client?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
