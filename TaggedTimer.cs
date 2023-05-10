using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhlozLib
{
    public class TaggedTimer : System.Timers.Timer
    {
        public TaggedTimer(double interval)
            : base(interval)
        {
        }

        public object Tag { get; set; }
    }
}
