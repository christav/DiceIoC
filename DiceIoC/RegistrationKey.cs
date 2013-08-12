using System;

namespace DiceIoC
{
    struct RegistrationKey
    {
        public readonly string Name;
        public readonly Type Type;

        public RegistrationKey(string name, Type type)
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
    }
}
