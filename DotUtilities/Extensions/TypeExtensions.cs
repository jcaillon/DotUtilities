#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (TypeExtensions.cs) is part of DotUtilities.
//
// DotUtilities is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// DotUtilities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with DotUtilities. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotUtilities.Attributes;

namespace DotUtilities.Extensions {

    /// <summary>
    /// A collection of extensions for types.
    /// </summary>
    public static class TypeExtension {

        /// <summary>
        /// Wraps this object instance into an IEnumerable&lt;T&gt;
        /// consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the object. </typeparam>
        /// <param name="item"> The instance that will be wrapped. </param>
        /// <returns> An IEnumerable&lt;T&gt; consisting of a single item. </returns>
        public static IEnumerable<T> Yield<T>(this T item) {
            yield return item;
        }

        /// <summary>
        /// Set a value to this instance, by its property name
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetValueOf(this object instance, string propertyName, object value) {
            var fieldInfo = instance.GetType().GetField(propertyName);
            if (fieldInfo == null) {
                var propertyInfo = instance.GetType().GetProperty(propertyName);
                if (propertyInfo == null) {
                    return false;
                }
                propertyInfo.SetValue(instance, value, null);
                return true;
            }
            fieldInfo.SetValue(instance, value);

            return true;
        }

        /// <summary>
        /// Get a value from this instance, by its property name
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetValueOf(this object instance, string propertyName) {
            var fieldInfo = instance.GetType().GetField(propertyName);
            if (fieldInfo == null) {
                var propertyInfo = instance.GetType().GetProperty(propertyName);
                if (propertyInfo == null) {
                    return null;
                }
                return propertyInfo.GetValue(instance, null);
            }
            return fieldInfo.GetValue(instance);
        }

        /// <summary>
        /// Returns true of the given object has the given method
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static bool HasMethod(this object objectToCheck, string methodName) {
            try {
                var type = objectToCheck.GetType();
                return type.GetMethod(methodName) != null;
            } catch (AmbiguousMatchException) {
                // ambiguous means there is more than one result,
                // which means: a method with that name does exist
                return true;
            }
        }

        /// <summary>
        /// Invoke the given method with the given parameters on the given object and returns its value
        /// Returns null if it fails
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object InvokeMethod(this object obj, string methodName, object[] parameters) {
            try {
                //Get the method information using the method info class
                MethodInfo mi = obj.GetType().GetMethod(methodName);
                return mi != null ? mi.Invoke(obj, parameters) : null;
            } catch (Exception) {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the constructor has a parameter-less constructor
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool HasDefaultConstructor(this Type t) {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Returns a new object that has the same public property values.
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDeepCopy<T>(this T obj) where T : class {
            return obj.DeepCopy<T>(null);
        }

        /// <summary>
        /// Copies all the public properties of one object to another.
        /// </summary>
        /// <remarks>
        /// <para>
        /// - If a property is a class and both the source and target values are not null, a sub deep copy is started on the properties of those 2 objects.
        /// - If a property is a list or an array, the source value will not replace the target value, it will just be added to it.
        /// - If a property is null in the source object, it won't replace the value in the target object.
        /// </para>
        /// </remarks>
        /// <param name="sourceObj">The source object, from which to copy the public values.</param>
        /// <param name="targetObj">The target object where will copy values to. This type should share common properties with the source object for the copy to do something. Can be null, in which case a new instance of type <typeparamref name="T"/> is created.</param>
        /// <typeparam name="T">The targeted type.</typeparam>
        /// <returns>The targeted object.</returns>
        /// <exception cref="Exception"></exception>
        public static T DeepCopy<T>(this object sourceObj, T targetObj) where T : class {
            var targetType = typeof(T);
            if (targetObj == null && (targetType.IsInterface || !HasDefaultConstructor(targetType))) {
                throw new Exception($"Can't deep copy to a new instance without a default constructor for type {targetType}.");
            }
            return (T) DeepCopyPublicProperties(sourceObj, targetType, targetObj);
        }

        /// <summary>
        /// Copies all the public properties of one object to another.
        /// </summary>
        /// <remarks>
        /// <para>
        /// - If a property is a class and both the source and target values are not null, a sub deep copy is started on the properties of those 2 objects.
        /// - If a property is a list or an array, the source value will not replace the target value, it will just be added to it.
        /// - If a property is null in the source object, it won't replace the value in the target object.
        /// </para>
        /// </remarks>
        /// <param name="sourceObj">The source object, from which to copy the public values.</param>
        /// <param name="targetObj">The target object where will copy values to. This type should share common properties with the source object for the copy to do something. Can be null, in which case a new instance of type <paramref name="targetType"/> will be created.</param>
        /// <param name="targetType"></param>
        /// <returns>The targeted object.</returns>
        /// <exception cref="Exception"></exception>
        public static object DeepCopy(this object sourceObj, object targetObj, Type targetType) {
            if (targetObj == null && (targetType.IsInterface || !HasDefaultConstructor(targetType))) {
                throw new Exception($"Can't deep copy to a new instance without a default constructor for type {targetType}.");
            }
            if (targetObj != null && !targetType.IsInstanceOfType(targetObj)) {
                throw new Exception($"The target object is not of type {targetType}.");
            }
            return DeepCopyPublicProperties(sourceObj, targetType, targetObj);
        }

        /// <summary>
        /// Create a new deep copy of an object, using its default constructor.
        /// </summary>
        /// <param name="sourceObj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static object DeepCopyToNew(this object sourceObj, Type targetType) {
            if (!HasDefaultConstructor(targetType)) {
                throw new Exception($"Can't deep copy to a new instance without a default constructor for type {targetType}.");
            }
            return DeepCopyPublicProperties(sourceObj, targetType);
        }

        /// <summary>
        /// Copies all the public properties of one object to another
        /// </summary>
        /// <remarks>
        /// - If a property is a class and both the source and target values are not null, a sub deep copy is started on the properties of those 2 objects.
        /// - If a property is a list or an array, the source value will not replace the target value, it will just be added to it.
        /// - If a property is null in the source object, it won't replace the value in the target object.
        /// </remarks>
        /// <param name="sourceObj">The source object, from which to copy the public values.</param>
        /// <param name="targetType"></param>
        /// <param name="targetObj">The target object where will copy values to. This type should share common properties with the source object for the copy to do something. Can be null, in which case a new instance of type <paramref name="targetType"/> is created.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static object DeepCopyPublicProperties(object sourceObj, Type targetType, object targetObj = null) {
            if (sourceObj == null) {
                return targetObj;
            }
            if (!targetType.IsClass && !targetType.IsInterface) {
                return sourceObj;
            }
            if (targetType == typeof(string)) {
                return sourceObj;
            }
            if (targetObj == null) {
                targetObj = Activator.CreateInstance(targetType);
            }

            var sourceProperties = sourceObj.GetType().GetProperties();
            var targetProperties = targetType.GetProperties();
            foreach (var sourceProperty in sourceProperties) {
                if (!sourceProperty.CanRead || sourceProperty.PropertyType.IsNotPublic) {
                    continue;
                }
                var targetProperty = targetProperties.FirstOrDefault(x => x.Name == sourceProperty.Name);
                if (targetProperty == null || !targetProperty.CanWrite || targetProperty.PropertyType.IsNotPublic) {
                    continue;
                }
                if (Attribute.GetCustomAttribute(targetProperty, typeof(DeepCopyAttribute), true) is DeepCopyAttribute attribute && attribute.Ignore) {
                    continue;
                }
                if (sourceProperty.PropertyType != targetProperty.PropertyType) {
                    continue;
                }
                var obj = sourceProperty.GetValue(sourceObj);
                if (obj == null) {
                    continue;
                }
                switch (obj) {
                    case string _:
                        targetProperty.SetValue(targetObj, obj);
                        break;
                    case IList listItem:
                        if (sourceProperty.PropertyType.IsArray) {
                            var subtype = sourceProperty.PropertyType.GetElementType();
                            if (subtype == null) {
                                throw new Exception($"Unknown element type of array {sourceProperty.Name}.");
                            }
                            var targetArray = targetProperty.GetValue(targetObj) as IList;
                            var targetArrayCount = targetArray?.Count ?? 0;
                            var array = Array.CreateInstance(subtype, listItem.Count + targetArrayCount);
                            if (targetArray != null) {
                                for (int i = 0; i < targetArray.Count; i++) {
                                    array.SetValue(targetArray[i], i);
                                }
                            }
                            for (int i = 0; i < listItem.Count; i++) {
                                array.SetValue(listItem[i] != null ? DeepCopyPublicProperties(listItem[i], listItem[i].GetType()) : null, i + targetArrayCount);
                            }
                            targetProperty.SetValue(targetObj, array);

                        } else if (sourceProperty.PropertyType.UnderlyingSystemType.GenericTypeArguments.Length > 0) {
                            IList list;
                            if (targetProperty.GetValue(targetObj) is IList targetList) {
                                list = targetList;
                            } else {
                                list = (IList) Activator.CreateInstance(targetProperty.PropertyType);
                            }
                            foreach (var item in listItem) {
                                list.Add(item != null ? DeepCopyPublicProperties(item, item.GetType()) : null);
                            }
                            targetProperty.SetValue(targetObj, list);
                        }

                        break;
                    default:
                        if (sourceProperty.PropertyType.IsClass || sourceProperty.PropertyType.IsInterface) {
                            var targetObjValue = targetProperty.GetValue(targetObj);
                            targetProperty.SetValue(targetObj, DeepCopyPublicProperties(obj, obj.GetType(), targetObjValue));
                        } else {
                            targetProperty.SetValue(targetObj, obj);
                        }
                        break;
                }
            }
            return targetObj;
        }
    }
}
