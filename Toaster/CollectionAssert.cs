using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Collections;

namespace Toaster
{
    public static class CollectionAssert
    {
        private class ObjectComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                if (!object.Equals(x, y))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static void AreEqual(ICollection expected, ICollection actual)
        {
            AreEqual(expected, actual, string.Empty, null);
        }

        public static void AreEqual(ICollection expected, ICollection actual, IComparer comparer, string message)
        {
            AreEqual(expected, actual, comparer, message, null);
        }

        public static void AreEqual(ICollection expected, ICollection actual, string message, params object[] parameters)
        {
            string reason = string.Empty;
            if (!AreCollectionsEqual(expected, actual, new ObjectComparer(), ref reason))
            {
                Assert.HandleFail("CollectionAssert.AreEqual", (string)TestingResources.CollectionEqualReason(message, reason), parameters);
            }
        }

        public static void AreEqual(ICollection expected, ICollection actual, IComparer comparer, string message, params object[] parameters)
        {
            string reason = string.Empty;
            if (!AreCollectionsEqual(expected, actual, comparer, ref reason))
            {
                Assert.HandleFail("CollectionAssert.AreEqual", 
                    (string)TestingResources.CollectionEqualReason(message, reason), parameters);
            }
        }

        public static void AreEquivalent(ICollection expected, ICollection actual)
        {
            AreEquivalent(expected, actual, String.Empty, null);
        }

        public static void AreEquivalent(ICollection expected, ICollection actual, string message)
        {
            AreEquivalent(expected, actual, message, null);
        }

        public static void AreEquivalent(ICollection expected, ICollection actual, string message, params object[] parameters)
        {
            if ((expected == null) != (actual == null))
            {
                Assert.HandleFail("CollectionAssert.AreEquivalent", message, parameters);
            }
            
            if (!object.ReferenceEquals(expected, actual) && (expected != null))
            {
                int num;
                int num2;
                object obj2;
                
                if (expected.Count != actual.Count)
                {
                    string str = (string)TestingResources.ElementNumbersDontMatch((message == null) ? string.Empty : Assert.ReplaceNulls(message), expected.Count, actual.Count);
                    Assert.HandleFail("CollectionAssert.AreEquivalent", str, parameters);
                }
                
                if ((expected.Count != 0) && FindMismatchedElement(expected, actual, out num, out num2, out obj2))
                {
                    string str2 = (string)TestingResources.ActualHasMismatchedElements((message == null) ? string.Empty : Assert.ReplaceNulls(message), num.ToString(CultureInfo.CurrentCulture.NumberFormat), Assert.ReplaceNulls(obj2), num2.ToString(CultureInfo.CurrentCulture.NumberFormat));
                    Assert.HandleFail("CollectionAssert.AreEquivalent", str2, parameters);
                }
            }
        }

        private static bool FindMismatchedElement(ICollection expected, ICollection actual, out int expectedCount, out int actualCount, out object mismatchedElement)
        {
            int num;
            int num2;
            Dictionary<object, int> elementCounts = GetElementCounts(expected, out num);
            Dictionary<object, int> dictionary2 = GetElementCounts(actual, out num2);
            if (num2 != num)
            {
                expectedCount = num;
                actualCount = num2;
                mismatchedElement = null;
                return true;
            }
            foreach (object obj2 in elementCounts.Keys)
            {
                elementCounts.TryGetValue(obj2, out expectedCount);
                dictionary2.TryGetValue(obj2, out actualCount);
                if (expectedCount != actualCount)
                {
                    mismatchedElement = obj2;
                    return true;
                }
            }
            expectedCount = 0;
            actualCount = 0;
            mismatchedElement = null;
            return false;
        }

        private static Dictionary<object, int> GetElementCounts(ICollection collection, out int nullCount)
        {
            Dictionary<object, int> dictionary = new Dictionary<object, int>();
            nullCount = 0;
            foreach (object obj2 in collection)
            {
                if (obj2 == null)
                {
                    nullCount++;
                }
                else
                {
                    int num;
                    dictionary.TryGetValue(obj2, out num);
                    num++;
                    dictionary[obj2] = num;
                }
            }
            return dictionary;
        }

        private static bool AreCollectionsEqual(ICollection expected, ICollection actual, IComparer comparer, ref string reason)
        {
            Assert.CheckParameterNotNull(comparer, "Assert.AreCollectionsEqual", "comparer", string.Empty, new object[0]);

            if (!object.ReferenceEquals(expected, actual))
            {
                if ((expected == null) || (actual == null))
                {
                    return false;
                }
                if (expected.Count != actual.Count)
                {
                    reason = (string)TestingResources.NumberOfElementsDiff;
                    return false;
                }
                IEnumerator enumerator = expected.GetEnumerator();
                IEnumerator enumerator2 = actual.GetEnumerator();
                for (int i = 0; enumerator.MoveNext() && enumerator2.MoveNext(); i++)
                {
                    if (0 != comparer.Compare(enumerator.Current, enumerator2.Current))
                    {
                        reason = (string)TestingResources.ElementsAtIndexDontMatch(i);
                        return false;
                    }
                }
                reason = (string)TestingResources.BothCollectionsSameElements;
                return true;
            }
            reason = (string)TestingResources.BothCollectionsSameReference(string.Empty);
            return true;
        }
    }
}
