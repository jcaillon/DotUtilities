#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (FileList.cs) is part of Oetools.Utilities.
//
// Oetools.Utilities is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Oetools.Utilities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Oetools.Utilities. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oetools.Utilities.Lib {

    /// <summary>
    /// Class to handle a list of unique paths.
    /// The point of this implementation is to quickly find out if a path exists in this list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PathList<T> : IEnumerable<T> where T : IPathListItem {

        private Dictionary<string, T> _dic = new Dictionary<string, T>(Utils.IsRuntimeWindowsPlatform ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

        /// <summary>
        /// Gets or sets the element with the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public T this[string path] {
            get => !string.IsNullOrEmpty(path) && _dic.ContainsKey(path) ? _dic[path] : default(T);
            set {
                if (path == null) {
                    throw new ArgumentNullException(nameof(path));
                }
                if (_dic.ContainsKey(path)) {
                    _dic[path] = value;
                } else {
                    _dic.Add(path, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets an element using its <see cref="IPathListItem.Path"/> property.
        /// </summary>
        /// <param name="item"></param>
        public T this[T item] {
            get => this[item?.Path];
            set => this[item?.Path] = value;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() {
            return _dic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Add an element using <see cref="IPathListItem.Path"/> as a unique key. Throws an exception if already exists.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(T item) {
            if (item?.Path == null) {
                throw new ArgumentNullException(nameof(item));
            }
            _dic.Add(item.Path, item);
        }

        /// <summary>
        /// Add a range of elements using <see cref="IPathListItem.Path"/> as a unique key. Throws an exception if one already exists.
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(IEnumerable<T> list) {
            if (list != null) {
                foreach (var item in list.Where(item => item?.Path != null)) {
                    _dic.Add(item.Path, item);
                }
            }
        }

        /// <summary>
        /// Add a range of elements using <see cref="IPathListItem.Path"/> as a unique key. Does NOT throw an exception if one already exists (does nothing).
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int TryAddRange(IEnumerable<T> list) {
            int nbAdded = 0;
            if (list != null) {
                foreach (var item in list.Where(item => item?.Path != null && !_dic.ContainsKey(item.Path))) {
                    _dic.Add(item.Path, item);
                    nbAdded++;
                }
            }
            return nbAdded;
        }

        /// <summary>
        /// Add an element using <see cref="IPathListItem.Path"/> as a unique key. Does NOT throw an exception if already exists (does nothing).
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryAdd(T item) {
            if (item?.Path == null) {
                throw new ArgumentNullException(nameof(item));
            }
            if (!_dic.ContainsKey(item.Path)) {
                _dic.Add(item.Path, item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clear (empty) the collection.
        /// </summary>
        public void Clear() {
            _dic.Clear();
        }

        /// <summary>
        /// Test if the collection contains a given element using <see cref="IPathListItem.Path"/> as a unique key.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item) {
            return Contains(item?.Path);
        }

        /// <summary>
        /// Test if the collection contains a given element using <see cref="IPathListItem.Path"/> as a unique key.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Contains(string path) {
            return !string.IsNullOrEmpty(path) && _dic.ContainsKey(path);
        }

        /// <summary>
        /// Removes the element from the collection using <see cref="IPathListItem.Path"/> as a unique key.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Remove(T item) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            return _dic.Remove(item.Path);
        }

        /// <summary>
        /// Returns the number of objects in the collection.
        /// </summary>
        public int Count => _dic.Count;

        /// <summary>
        /// Returns a new list with transformed items.
        /// </summary>
        /// <param name="selector"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public PathList<TResult> CopySelect<TResult>(Func<T, TResult> selector) where TResult : IPathListItem {
            var output = new PathList<TResult>();
            foreach (var item in this) {
                output.Add(selector(item));
            }
            return output;
        }

        /// <summary>
        /// Returns of a copy of this list, with only the items selected
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public PathList<T> CopyWhere(Func<T, bool> selector) {
            var output = new PathList<T>();
            foreach (var item in this) {
                if (selector(item)) {
                    output.Add(item);
                }
            }
            return output;
        }

        /// <summary>
        /// Apply a transformation to each path (key). If the resulting new path is not existing, it will be added back to the list.
        /// </summary>
        /// <param name="pathTransformFunction"></param>
        public void ApplyPathTransformation(Func<T, T> pathTransformFunction) {
            foreach (var key in _dic.Keys.ToList()) {
                var transformedItem = pathTransformFunction(_dic[key]);
                if (!_dic.ContainsKey(transformedItem.Path)) {
                    _dic.Add(transformedItem.Path, transformedItem);
                    _dic.Remove(key);
                } else if (!key.Equals(transformedItem.Path, Utils.IsRuntimeWindowsPlatform ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) {
                    _dic.Remove(key);
                }
            }
        }
    }
}
