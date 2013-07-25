using System;

namespace DiceIoC
{
    class RegistrationKey
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }

        public RegistrationKey(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public override bool Equals(object obj)
        {
            var r = obj as RegistrationKey;
            return r != null &&
                Type == r.Type &&
                string.Compare(Name, r.Name, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
    }
}
