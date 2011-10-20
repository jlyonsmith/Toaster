using System;
using System.Collections.Generic;
using System.Text;

namespace Toaster
{
    public class AssertFailedException : Exception
    {
        public AssertFailedException() {}
        
        public AssertFailedException(string message) : base(message) {}
    }
}
