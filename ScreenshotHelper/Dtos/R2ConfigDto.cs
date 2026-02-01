using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.Dtos
{
    public class R2ConfigDto
    {
        public string EndPoint { get; set; }

        public string AccessKey { get; set; }

        public string AccessId { get; set; }

        public string AccessSecret { get; set; }

        public List<BucketDto> Buckets { get; set; }

    }

    public class BucketDto
    {
        public string Name { get; set; }
        public string Domain { get; set; }
    }
}
