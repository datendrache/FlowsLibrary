//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhlozLib
{
    public interface ForwarderInterface
    {
        void Start();
        void Stop();
        void Dispose();
        Boolean sendDocument(BaseDocument Document);
        void StartSuspend();
        void EndSuspend();
        Boolean isSuspended();
        Boolean isRunning();
        void HeartBeat();
    }
}
