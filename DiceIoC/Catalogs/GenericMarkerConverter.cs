using System;
using System.Linq;
using System.Reflection;

namespace DiceIoC.Catalogs
{
    /// <summary>
    /// Helper class that converts types, or sets of types,
    /// or reflection objects, from marked open generics
    /// to the required actual types.
    /// </summary>
    class GenericMarkerConverter
    {
        private readonly Type[] substitutionTypes;

        public GenericMarkerConverter(Type[] substitutionTypes)
        {
            this.substitutionTypes = substitutionTypes;
        }

        public Type OpenToClosed(Type t)
        {
            return ReplaceNonMarkedType(t) ??
                ReplaceMarker(t) ??
                ReplaceMarkedType(t);
        }

        public ConstructorInfo OpenToClosed(ConstructorInfo constructor, Type closedTypeToConstruct)
        {
            var arguments = constructor.GetParameters().Select(p => OpenToClosed(p.ParameterType));
            return closedTypeToConstruct.GetConstructor(arguments.ToArray());
        }

        private Type ReplaceNonMarkedType(Type t)
        {
            return GenericMarkers.IsMarkedGeneric(t) ? null : t;
        }

        private Type ReplaceMarker(Type t)
        {
            int index = GenericMarkers.GenericMarkerIndex(t);
            if (index != -1)
            {
                return substitutionTypes[index];
            }
            return null;
        }

        private Type ReplaceMarkedType(Type t)
        {
            Type genericType = t.GetGenericTypeDefinition();
            var genericArgs = t.GetGenericArguments();
            var closedArgs = genericArgs.Select(OpenToClosed).ToArray();
            return genericType.MakeGenericType(closedArgs);
        }
    }
}
