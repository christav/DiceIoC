using System;
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
