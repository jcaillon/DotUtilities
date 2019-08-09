#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (TypeUtils.cs) is part of DotUtilities.
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
using System.Reflection;
using DotUtilities.Attributes;

namespace DotUtilities {

    /// <summary>
    /// Class that exposes utility methods.
    /// </summary>
    public static partial class Utils {

        /// <summary>
        /// Browse every public properties of an object searching for string properties (can also dive into classes and Ienumerable of classes)
        /// allows to replace the current string value by another one
        /// </summary>
        /// <param name="instanceType"></param>
        /// <param name="instance"></param>
        /// <param name="stringReplacementFunction"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void ForEachPublicPropertyStringInObject(Type instanceType, object instance, Func<PropertyInfo, string, string> stringReplacementFunction) {
            var properties = instanceType.GetProperties();
            foreach (var property in properties) {
                if (!property.CanRead || !property.CanWrite || property.PropertyType.IsNotPublic) {
                    continue;
                }
                if (Attribute.GetCustomAttribute(property, typeof(ReplaceStringPropertyAttribute), true) is ReplaceStringPropertyAttribute attribute && attribute.SkipReplace) {
                    continue;
                }

                var obj = property.GetValue(instance);
                switch (obj) {
                    case string strObj:
                        property.SetValue(instance, stringReplacementFunction(property, strObj));
                        break;
                    case IEnumerable listItem:
                        if (listItem is IList<string> ilistOfStrings) {
                            for (int i = 0; i < ilistOfStrings.Count; i++) {
                                ilistOfStrings[i] = stringReplacementFunction(property, ilistOfStrings[i]);
                            }
                        } else if (property.PropertyType.UnderlyingSystemType.GenericTypeArguments.Length > 0) {
                            foreach (var item in listItem) {
                                if (item != null) {
                                    ForEachPublicPropertyStringInObject(item.GetType(), item, stringReplacementFunction);
                                }
                            }
                        }
                        break;
                    default:
                        if (property.PropertyType.IsClass && obj != null) {
                            ForEachPublicPropertyStringInObject(property.PropertyType, obj, stringReplacementFunction);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// For a given object, replace all properties with the empty lists by the value null.
        /// </summary>
        /// <param name="obj"></param>
        public static void ReplaceEmptyListsByNull(object obj) {
            if (obj == null) {
                return;
            }
            var objType = obj.GetType();
            foreach (var property in objType.GetProperties()) {
                if (!property.CanRead || !property.CanWrite || property.PropertyType.IsNotPublic) {
                    continue;
                }
                var propValue = property.GetValue(obj);
                if (propValue == null) {
                    continue;
                }
                switch (propValue) {
                    case IList listItem:
                        if (listItem.Count == 0) {
                            property.SetValue(obj, null);
                        } else {
                            foreach (var item in listItem) {
                                ReplaceEmptyListsByNull(item);
                            }

                        }
                        break;
                    default:
                        if (property.PropertyType.IsClass) {
                            ReplaceEmptyListsByNull(propValue);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Allows to set default values to certain public properties of an object using a static method retuning the type of the property.
        /// Does not replace non null values.
        /// Set the name of the static method using <see cref="DefaultValueMethodAttribute"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static void SetDefaultValues(object obj) {
            if (obj == null) {
                return;
            }
            var objType = obj.GetType();
            foreach (var property in objType.GetProperties()) {
                if (!property.CanRead || !property.CanWrite || property.PropertyType.IsNotPublic) {
                    continue;
                }
                var propValue = property.GetValue(obj);
                if (Attribute.GetCustomAttribute(property, typeof(DefaultValueMethodAttribute), true) is DefaultValueMethodAttribute attribute && !string.IsNullOrEmpty(attribute.MethodName)) {
                    if (propValue == null) {
                        var methodInfo = objType.GetMethod(attribute.MethodName, BindingFlags.Public | BindingFlags.Static| BindingFlags.FlattenHierarchy);
                        if (methodInfo != null) {
                            propValue = methodInfo.Invoke(null, null);
                            property.SetValue(obj, propValue); // invoke static method
                        }
                    }
                }
                switch (propValue) {
                    case IEnumerable listItem:
                        if (property.PropertyType.UnderlyingSystemType.GenericTypeArguments.Length > 0) {
                            foreach (var item in listItem) {
                                if (item != null) {
                                    SetDefaultValues(item);
                                }
                            }
                        }
                        break;
                    default:
                        if (property.PropertyType.IsClass && propValue != null) {
                            SetDefaultValues(propValue);
                        }
                        break;
                }
            }
        }
    }
}
