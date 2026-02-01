using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace MonitorService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            serviceInstaller.ServiceName = "Monitor Service"; // 服务名称
            serviceInstaller.DisplayName = "系统监控服务";   // 显示名称
            serviceInstaller.Description = "该服务监控系统运行情况，停止之后无法记录系统异常"; // 描述信息
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
        }
    }
}
