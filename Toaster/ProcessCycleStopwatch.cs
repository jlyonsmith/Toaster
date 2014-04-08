using System;

namespace Toaster
{
    public class ProcessCycleStopwatch
    {
        public ProcessCycleStopwatch()
        {
        }

        public static bool IsSupported
        {
            get
            {
                return false;
            }
        }

        public long ElapsedCycles
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static ProcessCycleStopwatch StartNew()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}

