using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SCSV.AlarmInteract
{
    class Logger
    {
        static Logger()
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            //HttpHelper = new HttpJsonHelper();
        }

        /// <summary>
        /// 日志输出
        /// </summary>
        public readonly static ILog Log;
    }
}
