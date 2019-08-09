#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (CollectionsExtensions.cs) is part of DotUtilities.
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
using System.Collections.Generic;
using System.Linq;

namespace DotUtilities.Extensions {

    /// <summary>
    /// A collection of extensions for collections.
    /// </summary>
    public static class CollectionsExtensions {
        /// <summary>
        /// Converts an IEnumerable to a HashSet, optionally add to an existing hashset
        /// </summary>
        /// <typeparam name="T">The IEnumerable type</typeparam>
        /// <param name="enumerable">The IEnumerable</param>
        /// <param name="existingHashSet"></param>
        /// <returns>A new HashSet</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, HashSet<T> existingHashSet = null) {
            if (existingHashSet == null) {
                existingHashSet = new HashSet<T>();
            }

            foreach (T item in enumerable) {
                if (!existingHashSet.Contains(item)) {
                    existingHashSet.Add(item);
                }
            }

            return existingHashSet;
        }

        /// <summary>
        /// Same as Union, expect one or both arguments can be null; will always return an empty list in the worse case
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="enumerable2"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> UnionHandleNull<T>(this IEnumerable<T> enumerable, IEnumerable<T> enumerable2) {
            var output = new List<T>();
            if (enumerable != null) {
                output.AddRange(enumerable);
            }
            if (enumerable2 != null) {
                output.AddRange(enumerable2);
            }
            return output;
        }

        /// <summary>
        ///     Same as ToList but returns an empty list on Null instead of an exception
        /// </summary>
        public static List<T> ToNonNullList<T>(this IEnumerable<T> obj) {
            return obj == null ? new List<T>() : obj.ToList();
        }

        /// <summary>
        ///     Same as ToList but returns an empty list on Null instead of an exception
        /// </summary>
        public static IEnumerable<T> ToNonNullEnumerable<T>(this IEnumerable<T> obj) {
            return obj ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the specified comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <param name="comparer">The equality comparer to use to determine whether or not keys are equal.
        /// If null, the default equality comparer for <c>TSource</c> is used.</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return _(); IEnumerable<TSource> _()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                        yield return element;
                }
            }
        }
    }
}
