using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Toaster
{
    public delegate void AssertDelegate();

    public class Assert
    {
        public static void IsTrue(bool condition)
        {
            IsTrue(condition, string.Empty, null);
        }

        public static void IsTrue(bool condition, string message)
        {
            IsTrue(condition, message, null);
        }

        public static void IsTrue(bool condition, string message, params object[] parameters)
        {
            if (!condition)
            {
                HandleFail("Assert.IsTrue", message, parameters);
            }
        }

        public static void IsFalse(bool condition)
        {
            IsFalse(condition, string.Empty, null);
        }

        public static void IsFalse(bool condition, string message)
        {
            IsFalse(condition, message, null);
        }

        public static void IsFalse(bool condition, string message, params object[] parameters)
        {
            if (condition)
            {
                HandleFail("Assert.IsFalse", message, parameters);
            }
        }

        public static void AreEqual<T>(T expected, T actual)
        {
            AreEqual<T>(expected, actual, null);
        }
    
        public static void AreEqual<T>(T expected, T actual, string message, params object[] parameters)
        {
            if (!object.Equals(expected, actual))
            {
                string str;
                
                if (((actual != null) && (expected != null)) && !actual.GetType().Equals(expected.GetType()))
                {
                    str = (string)TestingResources.AreEqualDifferentTypesFailMsg(
                        (message == null) ? string.Empty : ReplaceNulls(message), 
                        ReplaceNulls(expected), expected.GetType().FullName, 
                        ReplaceNulls(actual), actual.GetType().FullName);
                }
                else
                {
                    str = (string)TestingResources.AreEqualFailMsg(
                        (message == null) ? string.Empty : ReplaceNulls(message), 
                        ReplaceNulls(expected), 
                        ReplaceNulls(actual));
                }

                HandleFail("Assert.AreEqual", str, parameters);
            }
        }

        public static void AreNotEqual<T>(T notExpected, T actual)
        {
            AreNotEqual<T>(notExpected, actual, string.Empty, null);
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message)
        {
            AreNotEqual<T>(notExpected, actual, message, null);
        }

        public static void AreNotEqual<T>(T notExpected, T actual, string message, params object[] parameters)
        {
            if (object.Equals(notExpected, actual))
            {
                string str = (string)TestingResources.AreNotEqualFailMsg((message == null) ? string.Empty : ReplaceNulls(message), ReplaceNulls(notExpected), ReplaceNulls(actual));
                HandleFail("Assert.AreNotEqual", str, parameters);
            }
        }

        public static void IsNull(object value)
        {
            IsNull(value, string.Empty, null);
        }

        public static void IsNull(object value, string message)
        {
            IsNull(value, message, null);
        }

        public static void IsNull(object value, string message, params object[] parameters)
        {
            if (value != null)
            {
                HandleFail("Assert.IsNull", message, parameters);
            }
        }

        public static void IsNotNull(object value)
        {
            IsNotNull(value, string.Empty, null);
        }

        public static void IsNotNull(object value, string message)
        {
            IsNotNull(value, message, null);
        }

        public static void IsNotNull(object value, string message, params object[] parameters)
        {
            if (value == null)
            {
                HandleFail("Assert.IsNotNull", message, parameters);
            }
        }

        public static void IsInstanceOfType(object value, Type expectedType)
        {
            IsInstanceOfType(value, expectedType, string.Empty, null);
        }

        public static void IsInstanceOfType(object value, Type expectedType, string message)
        {
            IsInstanceOfType(value, expectedType, message, null);
        }

        public static void IsInstanceOfType(object value, Type expectedType, string message, params object[] parameters)
        {
            if (expectedType == null)
            {
                HandleFail("Assert.IsInstanceOfType", message, parameters);
            }
            if (!expectedType.IsInstanceOfType(value))
            {
                string str = (string)TestingResources.IsInstanceOfFailMsg(
                    (message == null) ? string.Empty : ReplaceNulls(message), 
                    expectedType.ToString(), 
                    (value == null) ? ((object)TestingResources.Common_NullInMessages) : ((object)value.GetType().ToString()));
                HandleFail("Assert.IsInstanceOfType", str, parameters);
            }
        }

        public static void Throws(AssertDelegate assertDelegate, Type expectedType)
        {
            Assert.Throws(assertDelegate, expectedType, string.Empty, null);
        }

        public static void Throws(AssertDelegate assertDelegate, Type expectedType, string message)
        {
            Assert.Throws(assertDelegate, expectedType, message, null);
        }

        public static void Throws(AssertDelegate assertDelegate, Type expectedType, string message, params object[] parameters)
        {
            if (assertDelegate == null || expectedType == null || 
                !expectedType.IsSubclassOf(typeof(Exception)))
            {
                HandleFail("Assert.Throws", message, parameters);
            }

            try
            {
                assertDelegate.Invoke();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, expectedType);
                return;
            }

            HandleFail("Assert.Throws", message, parameters);
        }

        public static void Fail()
        {
            Assert.Fail(String.Empty);
        }

        public static void Fail(string message)
        {
            HandleFail("Assert.Fail", message);
        }

        public static void Fail(string message, params object[] parameters)
        {
            HandleFail("Assert.Fail", message, parameters);
        }

        internal static void HandleFail(string assertionName, string message, params object[] parameters)
        {
            string str = string.Empty;
            
            if (!string.IsNullOrEmpty(message))
            {
                if (parameters == null)
                {
                    str = message;
                }
                else
                {
                    str = string.Format(CultureInfo.CurrentCulture, ReplaceNulls(message), parameters);
                }
            }
            throw new AssertFailedException(TestingResources.AssertionFailed(assertionName, str));
        }

        public static string ReplaceNullChars(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
        
            List<int> list = new List<int>();
        
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\0')
                {
                    list.Add(i);
                }
            }
        
            if (list.Count <= 0)
            {
                return input;
            }
        
            StringBuilder builder = new StringBuilder(input.Length + list.Count);
            int startIndex = 0;
        
            foreach (int num in list)
            {
                builder.Append(input.Substring(startIndex, num - startIndex));
                builder.Append(@"\0");
                startIndex = num + 1;
            }
            
            builder.Append(input.Substring(startIndex));
            
            return builder.ToString();
        }

        internal static string ReplaceNulls(object input)
        {
            if (input == null)
            {
                return TestingResources.Common_NullInMessages.ToString();
            }
            
            string str = input.ToString();
            
            if (str == null)
            {
                return TestingResources.Common_ObjectString.ToString();
            }
            
            return ReplaceNullChars(str);
        }

        internal static void CheckParameterNotNull(object param, string assertionName, string parameterName, string message, params object[] parameters)
        {
            if (param == null)
            {
                HandleFail(assertionName, (string)TestingResources.NullParameterToAssert(parameterName, message), parameters);
            }
        }
    }
}
