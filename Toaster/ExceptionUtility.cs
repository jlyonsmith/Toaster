using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Toaster
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionUtility
    {
        /// <summary>
        /// Determine if the exception is a critical runtime exception.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsCriticalException(Exception e)
        {
            return
                e is NullReferenceException || 
                e is StackOverflowException || 
                e is OutOfMemoryException || 
                e is ThreadAbortException || 
                e is IndexOutOfRangeException || 
                e is AccessViolationException;
        }
    }
}
