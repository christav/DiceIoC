﻿using System;
using System.Linq;

namespace DiceIoC.Catalogs
{
    public struct RegistrationKey
    {
        public readonly string Name;
        public readonly Type Type;

        public RegistrationKey(Type type)
            : this(type, null)
        {
            
        }

        public RegistrationKey(Type type, string name)
        {
            Name = name;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj is RegistrationKey)
            {
                var r = (RegistrationKey) obj;
                return Type == r.Type &&
                       string.Compare(Name, r.Name, StringComparison.OrdinalIgnoreCase) == 0;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public bool IsOpenGenericRegistration
        {
            get
            {
                if (Type.IsGenericType)
                {
                    return Type.GetGenericArguments().Any(IsGenericMarkerType);
                }

                return false;
            }
        }

        public bool IsValidOpenGenericRegistration
        {
            get
            {
                return IsOpenGenericRegistration &&
                       MarkersAreInProperOrder();
            }
        }

        // The generic marker types
        private static readonly Type[] genericMarkers = {
            typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4),
            typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9),
            typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14),
            typeof(T15), typeof(T16)
        };

        public static bool IsGenericMarkerType(Type t)
        {
            return genericMarkers.Any(marker => marker.IsAssignableFrom(t));
        }

        public static bool IsGenericMarkerType(Type t, int index)
        {
            return genericMarkers[index].IsAssignableFrom(t);
        }

        private bool MarkersAreInProperOrder()
        {
            Type[] genericArgs = Type.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; ++i)
            {
                if (IsGenericMarkerType(genericArgs[i]) && !genericMarkers[i].IsAssignableFrom(genericArgs[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator ==(RegistrationKey a, RegistrationKey b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RegistrationKey a, RegistrationKey b)
        {
            return !a.Equals(b);
        }

        public static RegistrationKey For<T>()
        {
            return new RegistrationKey(typeof (T), null);
        }

        public static RegistrationKey For<T>(string name)
        {
            return new RegistrationKey(typeof (T), name);
        }
    }
}
