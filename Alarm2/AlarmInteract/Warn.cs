using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCSV.AlarmInteract
{
     public  class Warn
    {
        private string extensionNum;
        private string extensionTime;
        private int    extensionStatus;

        public string ExtensionNum { get => extensionNum; set => extensionNum = value; }
        public int ExtensionStatus { get=>  extensionStatus; set=> extensionStatus=value; }
        public string ExtensionTime { get => extensionTime; set => extensionTime = Convert.ToDateTime(value).ToLongTimeString(); }

    }
}
