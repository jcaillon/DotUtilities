#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (AttributesExtensions.cs) is part of DotUtilities.
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace DotUtilities.Extensions {

    /// <summary>
    /// A collection of extensions for attributes.
    /// </summary>
    public static class AttributesExtensions {

        /// <summary>
        /// Get the attribute of type <typeparamref name="T"/> of the member <paramref name="memberName"/> from this type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttributeFrom<T>(this Type type, string memberName) where T : Attribute {
            var memberInfos = type.GetMember(memberName);
            if (memberInfos.Length > 0) {
                return (T) Attribute.GetCustomAttribute(memberInfos[0], typeof(T), true);
            }
            return null;
        }

        /// <summary>
        /// Returns the xml element or attribute name of a property
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetXmlName(this MemberInfo memberInfo) {
            // TODO : cache in dictionnary?
            var element = Attribute.GetCustomAttribute(memberInfo, typeof(XmlElementAttribute), true) as XmlElementAttribute;
            if (element == null) {
                var attribute = Attribute.GetCustomAttribute(memberInfo, typeof(XmlAttributeAttribute), true) as XmlAttributeAttribute;
                return attribute?.AttributeName ?? memberInfo.Name;
            }
            return element.ElementName;
        }

        /// <summary>
        /// Returns the xml element or attribute name of a property
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectPropertyNameOf"></param>
        /// <returns></returns>
        public static string GetXmlName(this Type objectType, string objectPropertyNameOf) {
            // TODO : cache in dictionnary?
            var element = objectType.GetAttributeFrom<XmlElementAttribute>(objectPropertyNameOf);
            if (element == null) {
                var attribute = objectType.GetAttributeFrom<XmlAttributeAttribute>(objectPropertyNameOf);
                if (attribute == null) {
                    var array = objectType.GetAttributeFrom<XmlArrayAttribute>(objectPropertyNameOf);
                    return array?.ElementName ?? objectPropertyNameOf;
                }
                return attribute.AttributeName ?? objectPropertyNameOf;
            }
            return element.ElementName ?? objectPropertyNameOf;
        }

        /// <summary>
        /// Returns the xml root name of a type
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static string GetXmlName(this Type objectType) {
            // TODO : cache in dictionnary?
            var root = Attribute.GetCustomAttribute(objectType, typeof(XmlRootAttribute), true) as XmlRootAttribute;
            return root?.ElementName ?? objectType.Name;
        }

        /// <summary>
        /// Use : var name = player.GetAttributeFrom DisplayAttribute>("PlayerDescription").Name;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static T GetAttributeFrom<T>(this object instance, string memberName) where T : Attribute {
            var attrType = typeof(T);
            var fieldInfo = instance.GetType().GetField(memberName);
            if (fieldInfo == null) {
                var propertyInfo = instance.GetType().GetProperty(memberName);
                if (propertyInfo == null) {
                    return (T) Convert.ChangeType(null, typeof(T));
                }
                return (T) propertyInfo.GetCustomAttributes(attrType, false).FirstOrDefault();
            }
            return (T) fieldInfo.GetCustomAttributes(attrType, false).FirstOrDefault();
        }

        /// <summary>
        /// Returns the attribute array for the given Type T and the given value,
        /// not to self : don't use that on critical path -> reflection is costly
        /// </summary>
        public static T[] GetAttributes<T>(this Enum value) where T : Attribute {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    var attributeArray = (T[]) Attribute.GetCustomAttributes(field, typeof(T), true);
                    return attributeArray;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the attribute for the given Type T and the given value,
        /// not to self : dont use that on critical path -> reflection is costly
        /// </summary>
        public static T GetAttribute<T>(this Enum value) where T : Attribute {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    var attribute = Attribute.GetCustomAttribute(field, typeof(T), true) as T;
                    return attribute;
                }
            }
            return null;
        }

        /// <summary>
        /// Decorate enum values with [Description("Description for Foo")] and get their description with x.Foo.GetDescription()
        /// </summary>
        public static string GetDescription(this Enum value) {
            var attr = value.GetAttribute<DescriptionAttribute>();
            return attr != null ? attr.Description : null;
        }
    }
}
