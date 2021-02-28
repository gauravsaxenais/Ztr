namespace ZTR.Framework.Business
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>Clones objects using reflection</summary>
    /// <remarks>
    ///   <para>
    ///     This type of cloning is a lot faster than cloning by serialization and
    ///     incurs no set-up cost, but requires cloned types to provide a default
    ///     constructor in order to work.
    ///   </para>
    /// </remarks>
    public class ReflectionCloner
    {
        /// <summary>
        ///   Creates a shallow clone of the specified object, reusing any referenced objects
        /// </summary>
        /// <typeparam name="TCloned">Type of the object that will be cloned</typeparam>
        /// <param name="objectToClone">Object that will be cloned</param>
        /// <returns>A shallow clone of the provided object</returns>
        public static TCloned ShallowFieldClone<TCloned>(TCloned objectToClone)
        {
            Type originalType = objectToClone.GetType();
            if (typeof(TCloned).IsClass || typeof(TCloned).IsArray)
            {
                if (objectToClone == null)
                {
                    return default;
                }
            }
            if (originalType.IsPrimitive || (originalType == typeof(string)))
            {
                return objectToClone; // Being value types, primitives are copied by default
            }
            else if (originalType.IsArray)
            {
                return (TCloned)ShallowCloneArray(objectToClone);
            }
            else if (originalType.IsValueType)
            {
                return objectToClone; // Value types can be copied directly
            }
            else
            {
                return (TCloned)ShallowCloneComplexFieldBased(objectToClone);
            }
        }

        /// <summary>
        ///   Creates a shallow clone of the specified object, reusing any referenced objects
        /// </summary>
        /// <typeparam name="TCloned">Type of the object that will be cloned</typeparam>
        /// <param name="objectToClone">Object that will be cloned</param>
        /// <returns>A shallow clone of the provided object</returns>
        public static TCloned ShallowPropertyClone<TCloned>(TCloned objectToClone)
        {
            Type originalType = objectToClone.GetType();
            if (typeof(TCloned).IsClass || typeof(TCloned).IsArray)
            {
                if (objectToClone == null)
                {
                    return default;
                }
            }
            if (originalType.IsPrimitive || (originalType == typeof(string)))
            {
                return objectToClone; // Being value types, primitives are copied by default
            }
            else if (originalType.IsArray)
            {
                return (TCloned)ShallowCloneArray(objectToClone);
            }
            else if (originalType.IsValueType)
            {
                return (TCloned)ShallowCloneComplexPropertyBased(objectToClone);
            }
            else
            {
                return (TCloned)ShallowCloneComplexPropertyBased(objectToClone);
            }
        }

        /// <summary>
        ///   Creates a deep clone of the specified object, also creating clones of all
        ///   child objects being referenced
        /// </summary>
        /// <typeparam name="TCloned">Type of the object that will be cloned</typeparam>
        /// <param name="objectToClone">Object that will be cloned</param>
        /// <returns>A deep clone of the provided object</returns>
        public static TCloned DeepFieldClone<TCloned>(TCloned objectToClone)
        {
            object objectToCloneAsObject = objectToClone;
            if (objectToClone == null)
            {
                return default;
            }
            else
            {
                return (TCloned)DeepCloneSingleFieldBased(objectToCloneAsObject);
            }
        }

        /// <summary>
        ///   Creates a deep clone of the specified object, also creating clones of all
        ///   child objects being referenced
        /// </summary>
        /// <typeparam name="TCloned">Type of the object that will be cloned</typeparam>
        /// <param name="objectToClone">Object that will be cloned</param>
        /// <returns>A deep clone of the provided object</returns>
        public static TCloned DeepPropertyClone<TCloned>(TCloned objectToClone)
        {
            object objectToCloneAsObject = objectToClone;
            if (objectToClone == null)
            {
                return default;
            }
            else
            {
                return (TCloned)DeepCloneSinglePropertyBased(objectToCloneAsObject);
            }
        }

        /// <summary>Clones a complex type using field-based value transfer</summary>
        /// <param name="original">Original instance that will be cloned</param>
        /// <returns>A clone of the original instance</returns>
        private static object ShallowCloneComplexFieldBased(object original)
        {
            Type originalType = original.GetType();
            object clone = FormatterServices.GetUninitializedObject(originalType);

            FieldInfo[] fieldInfos = GetFieldInfosIncludingBaseClasses(
              originalType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );
            for (int index = 0; index < fieldInfos.Length; ++index)
            {
                FieldInfo fieldInfo = fieldInfos[index];
                object originalValue = fieldInfo.GetValue(original);
                if (originalValue != null)
                {
                    // Everything's just directly assigned in a shallow clone
                    fieldInfo.SetValue(clone, originalValue);
                }
            }

            return clone;
        }

        /// <summary>Clones a complex type using property-based value transfer</summary>
        /// <param name="original">Original instance that will be cloned</param>
        /// <returns>A clone of the original instance</returns>
        private static object ShallowCloneComplexPropertyBased(object original)
        {
            Type originalType = original.GetType();
            object clone = Activator.CreateInstance(originalType);

            PropertyInfo[] propertyInfos = originalType.GetProperties(
              BindingFlags.Public | BindingFlags.NonPublic |
              BindingFlags.Instance | BindingFlags.FlattenHierarchy
            );
            for (int index = 0; index < propertyInfos.Length; ++index)
            {
                PropertyInfo propertyInfo = propertyInfos[index];
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    Type propertyType = propertyInfo.PropertyType;
                    object originalValue = propertyInfo.GetValue(original, null);
                    if (originalValue != null)
                    {
                        if (propertyType.IsPrimitive || (propertyType == typeof(string)))
                        {
                            // Primitive types can be assigned directly
                            propertyInfo.SetValue(clone, originalValue, null);
                        }
                        else if (propertyType.IsValueType)
                        {
                            // Value types are seen as part of the original type and are thus recursed into
                            propertyInfo.SetValue(clone, ShallowCloneComplexPropertyBased(originalValue), null);
                        }
                        else if (propertyType.IsArray)
                        { // Arrays are assigned directly in a shallow clone
                            propertyInfo.SetValue(clone, originalValue, null);
                        }
                        else
                        { // Complex types are directly assigned without creating a copy
                            propertyInfo.SetValue(clone, originalValue, null);
                        }
                    }
                }
            }

            return clone;
        }

        /// <summary>Clones an array using field-based value transfer</summary>
        /// <param name="original">Original array that will be cloned</param>
        /// <returns>A clone of the original array</returns>
        private static object ShallowCloneArray(object original)
        {
            return ((Array)original).Clone();
        }

        /// <summary>Copies a single object using field-based value transfer</summary>
        /// <param name="original">Original object that will be cloned</param>
        /// <returns>A clone of the original object</returns>
        private static object DeepCloneSingleFieldBased(object original)
        {
            Type originalType = original.GetType();
            if (originalType.IsPrimitive || (originalType == typeof(string)))
            {
                return original; // Creates another box, does not reference boxed primitive
            }
            else if (originalType.IsArray)
            {
                return DeepCloneArrayFieldBased((Array)original, originalType.GetElementType());
            }
            else
            {
                return DeepCloneComplexFieldBased(original);
            }
        }

        /// <summary>Clones a complex type using field-based value transfer</summary>
        /// <param name="original">Original instance that will be cloned</param>
        /// <returns>A clone of the original instance</returns>
        private static object DeepCloneComplexFieldBased(object original)
        {
            Type originalType = original.GetType();
            object clone = FormatterServices.GetUninitializedObject(originalType);
            FieldInfo[] fieldInfos = GetFieldInfosIncludingBaseClasses(
              originalType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            );
            for (int index = 0; index < fieldInfos.Length; ++index)
            {
                FieldInfo fieldInfo = fieldInfos[index];
                Type fieldType = fieldInfo.FieldType;
                object originalValue = fieldInfo.GetValue(original);
                if (originalValue != null)
                {
                    // Primitive types can be assigned directly
                    if (fieldType.IsPrimitive || (fieldType == typeof(string)))
                    {
                        fieldInfo.SetValue(clone, originalValue);
                    }
                    else if (fieldType.IsArray)
                    { // Arrays need to be cloned element-by-element
                        fieldInfo.SetValue(
                          clone,
                          DeepCloneArrayFieldBased((Array)originalValue, fieldType.GetElementType())
                        );
                    }
                    else
                    { // Complex types need to be cloned member-by-member
                        fieldInfo.SetValue(clone, DeepCloneSingleFieldBased(originalValue));
                    }
                }
            }

            return clone;
        }

        /// <summary>Clones an array using field-based value transfer</summary>
        /// <param name="original">Original array that will be cloned</param>
        /// <param name="elementType">Type of elements the original array contains</param>
        /// <returns>A clone of the original array</returns>
        private static object DeepCloneArrayFieldBased(Array original, Type elementType)
        {
            if (elementType.IsPrimitive || (elementType == typeof(string)))
            {
                return original.Clone();
            }

            int dimensionCount = original.Rank;

            // Find out the length of each of the array's dimensions, also calculate how
            // many elements there are in the array in total.
            var lengths = new int[dimensionCount];
            int totalElementCount = 0;
            for (int index = 0; index < dimensionCount; ++index)
            {
                lengths[index] = original.GetLength(index);
                if (index == 0)
                {
                    totalElementCount = lengths[index];
                }
                else
                {
                    totalElementCount *= lengths[index];
                }
            }

            // Knowing the number of dimensions and the length of each dimension, we can
            // create another array of the exact same sizes.
            Array clone = Array.CreateInstance(elementType, lengths);

            // If this is a one-dimensional array (most common type), do an optimized copy
            // directly specifying the indices
            if (dimensionCount == 1)
            {

                // Clone each element of the array directly
                for (int index = 0; index < totalElementCount; ++index)
                {
                    object originalElement = original.GetValue(index);
                    if (originalElement != null)
                    {
                        clone.SetValue(DeepCloneSingleFieldBased(originalElement), index);
                    }
                }

            }
            else
            { // Otherwise use the generic code for multi-dimensional arrays

                var indices = new int[dimensionCount];
                for (int index = 0; index < totalElementCount; ++index)
                {

                    // Determine the index for each of the array's dimensions
                    int elementIndex = index;
                    for (int dimensionIndex = dimensionCount - 1; dimensionIndex >= 0; --dimensionIndex)
                    {
                        indices[dimensionIndex] = elementIndex % lengths[dimensionIndex];
                        elementIndex /= lengths[dimensionIndex];
                    }

                    // Clone the current array element
                    object originalElement = original.GetValue(indices);
                    if (originalElement != null)
                    {
                        clone.SetValue(DeepCloneSingleFieldBased(originalElement), indices);
                    }

                }

            }

            return clone;
        }

        /// <summary>
        ///   Returns all the fields of a type, working around a weird reflection issue
        ///   where explicitly declared fields in base classes are returned, but not
        ///   automatic property backing fields.
        /// </summary>
        /// <param name="type">Type whose fields will be returned</param>
        /// <param name="bindingFlags">Binding flags to use when querying the fields</param>
        /// <returns>All of the type's fields, including its base types</returns>
        private static FieldInfo[] GetFieldInfosIncludingBaseClasses(
          Type type, BindingFlags bindingFlags
        )
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfos;
            }
            else
            { // Otherwise, collect all types up to the furthest base class
                var fieldInfoList = new List<FieldInfo>(fieldInfos);
                while (type.BaseType != typeof(object))
                {
                    type = type.BaseType;
                    fieldInfos = type.GetFields(bindingFlags);

                    // Look for fields we do not have listed yet and merge them into the main list
                    for (int index = 0; index < fieldInfos.Length; ++index)
                    {
                        bool found = false;

                        for (int searchIndex = 0; searchIndex < fieldInfoList.Count; ++searchIndex)
                        {
                            bool match =
                              (fieldInfoList[searchIndex].DeclaringType == fieldInfos[index].DeclaringType) &&
                              (fieldInfoList[searchIndex].Name == fieldInfos[index].Name);

                            if (match)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            fieldInfoList.Add(fieldInfos[index]);
                        }
                    }
                }

                return fieldInfoList.ToArray();
            }
        }

        /// <summary>Copies a single object using property-based value transfer</summary>
        /// <param name="original">Original object that will be cloned</param>
        /// <returns>A clone of the original object</returns>
        private static object DeepCloneSinglePropertyBased(object original)
        {
            Type originalType = original.GetType();
            if (originalType.IsPrimitive || (originalType == typeof(string)))
            {
                return original; // Creates another box, does not reference boxed primitive
            }
            else if (originalType.IsArray)
            {
                return DeepCloneArrayPropertyBased((Array)original, originalType.GetElementType());
            }
            else
            {
                return DeepCloneComplexPropertyBased(original);
            }
        }

        /// <summary>Clones a complex type using property-based value transfer</summary>
        /// <param name="original">Original instance that will be cloned</param>
        /// <returns>A clone of the original instance</returns>
        private static object DeepCloneComplexPropertyBased(object original)
        {
            Type originalType = original.GetType();
            object clone = Activator.CreateInstance(originalType);

            PropertyInfo[] propertyInfos = originalType.GetProperties(
              BindingFlags.Public | BindingFlags.NonPublic |
              BindingFlags.Instance | BindingFlags.FlattenHierarchy
            );
            for (int index = 0; index < propertyInfos.Length; ++index)
            {
                PropertyInfo propertyInfo = propertyInfos[index];
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    Type propertyType = propertyInfo.PropertyType;
                    object originalValue = propertyInfo.GetValue(original, null);
                    if (originalValue != null)
                    {
                        if (propertyType.IsPrimitive || (propertyType == typeof(string)))
                        {
                            // Primitive types can be assigned directly
                            propertyInfo.SetValue(clone, originalValue, null);
                        }
                        else if (propertyType.IsArray)
                        { // Arrays need to be cloned element-by-element
                            propertyInfo.SetValue(
                              clone,
                              DeepCloneArrayPropertyBased((Array)originalValue, propertyType.GetElementType()),
                              null
                            );
                        }
                        else
                        { // Complex types need to be cloned member-by-member
                            propertyInfo.SetValue(clone, DeepCloneSinglePropertyBased(originalValue), null);
                        }
                    }
                }
            }

            return clone;
        }

        /// <summary>Clones an array using property-based value transfer</summary>
        /// <param name="original">Original array that will be cloned</param>
        /// <param name="elementType">Type of elements the original array contains</param>
        /// <returns>A clone of the original array</returns>
        private static object DeepCloneArrayPropertyBased(Array original, Type elementType)
        {
            if (elementType.IsPrimitive || (elementType == typeof(string)))
            {
                return original.Clone();
            }

            int dimensionCount = original.Rank;

            // Find out the length of each of the array's dimensions, also calculate how
            // many elements there are in the array in total.
            var lengths = new int[dimensionCount];
            int totalElementCount = 0;
            for (int index = 0; index < dimensionCount; ++index)
            {
                lengths[index] = original.GetLength(index);
                if (index == 0)
                {
                    totalElementCount = lengths[index];
                }
                else
                {
                    totalElementCount *= lengths[index];
                }
            }

            // Knowing the number of dimensions and the length of each dimension, we can
            // create another array of the exact same sizes.
            Array clone = Array.CreateInstance(elementType, lengths);

            // If this is a one-dimensional array (most common type), do an optimized copy
            // directly specifying the indices
            if (dimensionCount == 1)
            {

                // Clone each element of the array directly
                for (int index = 0; index < totalElementCount; ++index)
                {
                    object originalElement = original.GetValue(index);
                    if (originalElement != null)
                    {
                        clone.SetValue(DeepCloneSinglePropertyBased(originalElement), index);
                    }
                }

            }
            else
            { // Otherwise use the generic code for multi-dimensional arrays

                var indices = new int[dimensionCount];
                for (int index = 0; index < totalElementCount; ++index)
                {

                    // Determine the index for each of the array's dimensions
                    int elementIndex = index;
                    for (int dimensionIndex = dimensionCount - 1; dimensionIndex >= 0; --dimensionIndex)
                    {
                        indices[dimensionIndex] = elementIndex % lengths[dimensionIndex];
                        elementIndex /= lengths[dimensionIndex];
                    }

                    // Clone the current array element
                    object originalElement = original.GetValue(indices);
                    if (originalElement != null)
                    {
                        clone.SetValue(DeepCloneSinglePropertyBased(originalElement), indices);
                    }

                }
            }

            return clone;
        }
    }
}
