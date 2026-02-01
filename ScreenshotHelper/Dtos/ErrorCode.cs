using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenshotHelper.Dtos
{
    // <summary>
    /// 返回码
    /// </summary>
    public enum ErrorCode
    {
        [Description("请求成功")]
        SUCCESS = 200,

        [Description("参数错误")]
        PARAM_ERROR = 101,

        [Description("记录已存在")]
        EXIST_ERROR = 102,

        [Description("记录不存在")]
        NOT_EXIST_ERROR = 103,

        [Description("验证码错误")]
        CAPTCHA_ERROR = 104,

        [Description("登录错误")]
        LOGIN_ERROR = 105,

        [Description("Bad Request")]
        BAD_REQUEST = 400,

        [Description("未授权")]
        DENY = 401,

        [Description("服务端错误")]
        GLOBAL_ERROR = 500,
    }
}
