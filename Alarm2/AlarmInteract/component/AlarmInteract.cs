using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCSV.AlarmInteract.component
{
    abstract class AlarmInteractClass
    {
        public string master;
        public EventHandler<Msg> handler;

        public abstract bool Start(string addr);
        public abstract void Stop();
    }
}
