using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.Dtos
{
    /// <summary>
    /// 注册返回DTO
    /// </summary>
    public class RegisterResultDto
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public string PackageType { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
