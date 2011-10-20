using System;
using System.Collections.Generic;
using System.Text;

namespace Toaster
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TestClassAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestMethodAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestInitializeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestCleanupAttribute : Attribute
    {
        #region Instance Constructors

        public TestCleanupAttribute()
        {
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AssemblyInitializeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AssemblyCleanupAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ClassInitializeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ClassCleanupAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OrderAttribute : Attribute
    {
        #region Public Properties
        public short Order { get; set; }

        #endregion

        #region Instance Constructors

        public OrderAttribute()
        {
            Order = short.MaxValue;
        }

        public OrderAttribute(short order)
        {
            Order = order;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ExpectedExceptionAttribute : Attribute
    {
        #region Public Properties
        public Type ExceptionType { get; set; }

        #endregion

        #region Instance Constructors

        public ExpectedExceptionAttribute(Type exceptionType)
        {
            ExceptionType = exceptionType;
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DeploymentItemAttribute : Attribute
    {
        #region Public Properties
        public string Path { get; private set; }
        public string OutputDirectory { get; set; }

        #endregion

        #region Instance Constructors
        public DeploymentItemAttribute(string path)
        {
            Path = path;
            OutputDirectory = String.Empty;
        }

        public DeploymentItemAttribute(string path, string outputDirectory)
        {
            Path = path;
            OutputDirectory = outputDirectory;
        }

        #endregion
    }
}
