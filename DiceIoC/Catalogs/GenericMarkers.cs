using System;
using System.Linq;

namespace DiceIoC.Catalogs
{
    /// <summary>
    /// Class supporting detection of and conversion
    /// of types, method infos, etc. from generic
    /// marker interfaces to the actual desired
    /// interfaces.
    /// </summary>
    public class GenericMarkers
    {
        // The generic marker types
        private static readonly Type[] genericMarkers = {
            typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4),
            typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9),
            typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14),
            typeof(T15), typeof(T16)
        };

        /// <summary>
        /// Does the type <paramref name="t"/> contain any generic
        /// parameters that are one of the marker types for open generics?
        /// </summary>
        /// <param name="t">Type to check</param>
        /// <returns>true if  it contains any open generic markers, false if not.</returns>
        public static bool IsMarkedGeneric(Type t)
        {
            return IsGenericMarkerType(t) || 
                (t.IsGenericType && t.GetGenericArguments().Any(IsMarkedGeneric));
        }

        public static bool IsGenericMarkerType(Type t)
        {
            return genericMarkers.Any(marker => marker.IsAssignableFrom(t));
        }

        public static int GenericMarkerIndex(Type t)
        {
            for (int i = 0; i < genericMarkers.Length; ++i)
            {
                if (genericMarkers[i].IsAssignableFrom(t))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool IsGenericMarkerType(Type t, int index)
        {
            return genericMarkers[index].IsAssignableFrom(t);
        }

        public static bool MarkersAreInProperOrder(Type t)
        {
            Type[] genericArgs = t.GetGenericArguments();
            return !genericArgs.Where((t1, i) => IsGenericMarkerType(t1) && !genericMarkers[i].IsAssignableFrom(t1)).Any();
        }

        public static bool IsValidMarkedGeneric(Type t)
        {
            return IsMarkedGeneric(t) && MarkersAreInProperOrder(t);
        }
    }
}
