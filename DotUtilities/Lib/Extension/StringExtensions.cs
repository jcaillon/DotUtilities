#region header
// ========================================================================
// Copyright (c) 2018 - Julien Caillon (julien.caillon@gmail.com)
// This file (StringExtensions.cs) is part of Oetools.Utilities.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Oetools.Utilities.Test")]

namespace Oetools.Utilities.Lib.Extension {

    /// <summary>
    /// A collection of extensions for strings.
    /// </summary>
    public static class StringExtensions {

        /// <summary>
        /// Converts a valid version string to a version object. (vX.X.X.X-suffix)
        /// </summary>
        /// <param name="versionString"></param>
        /// <returns></returns>
        public static Version ToVersion(this string versionString) {
            var idx = versionString.IndexOf('-');
            versionString = idx > 0 ? versionString.Substring(0, idx) : versionString;
            versionString = versionString.TrimStart('v');
            var nbDots = versionString.Length - versionString.Replace(".", "").Length;
            for (int i = 0; i < 3 - nbDots; i++) {
                versionString += ".0";
            }
            return new Version(versionString);
        }

        /// <summary>
        /// Returns either the original string or a default if the original string is null or empty (whitespaces only)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string TakeDefaultIfNeeded(this string source, string defaultValue) {
            return string.IsNullOrWhiteSpace(source) ? defaultValue : source;
        }

        /// <summary>
        ///     Converts a string to an object of the given type
        /// </summary>
        public static object ConvertFromStr(this string value, Type destType) {
            try {
                if (destType == typeof(string))
                    return value;
                return TypeDescriptor.GetConverter(destType).ConvertFromInvariantString(value);
            } catch (Exception) {
                return destType.IsValueType ? Activator.CreateInstance(destType) : null;
            }
        }

        /// <summary>
        /// Equivalent to Equals but case insensitive, also handles null case
        /// </summary>
        /// <param name="s"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool EqualsCi(this string s, string comp) {
            if (s == null || comp == null) {
                return s == null && comp == null;
            }
            return s.Equals(comp, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Case insensitive contains
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static bool ContainsCi(this string source, string toCheck, StringComparison comparison = StringComparison.Ordinal) {
            return source.IndexOf(toCheck, comparison) >= 0;
        }

        /// <summary>
        /// Checks if a string has correct place horders, return false if they are opened and not closed
        /// i.e. : "^zf^ez$f$" return true with tags ^ start $ end and depth 2
        /// </summary>
        /// <param name="source"></param>
        /// <param name="openPo"></param>
        /// <param name="closePo"></param>
        /// <param name="maxDepth"></param>
        /// <param name="comparison"></param>
        public static void ValidatePlaceHolders(this string source, string openPo = "{{", string closePo = "}}", int maxDepth = 0, StringComparison comparison = StringComparison.Ordinal) {
            source.ReplacePlaceHolders(null, openPo, closePo, maxDepth, comparison);
        }

        /// <summary>
        /// Replace the place holders in a string by a value
        /// </summary>
        /// <remarks>will throw errors, you have to validate that the source is correct first using <see cref="ValidatePlaceHolders"/></remarks>
        /// <remarks>will throw errors if your replacement string contains an open or clo</remarks>
        /// <param name="source"></param>
        /// <param name="replacementFunction"></param>
        /// <param name="openPo"></param>
        /// <param name="closePo"></param>
        /// <param name="maxDepth"></param>
        /// <param name="comparison"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public static string ReplacePlaceHolders(this string source, Func<string, string> replacementFunction, string openPo = "{{", string closePo = "}}", int maxDepth = 0, StringComparison comparison = StringComparison.Ordinal) {
            if (string.IsNullOrEmpty(source)) {
                return source;
            }
            var startPosStack = new Stack<int>();
            var osb = source;
            int idx = 0;
            do {
                var idxStart = osb.IndexOf(openPo, idx, comparison);
                var idxEnd = osb.IndexOf(closePo, idx, comparison);
                if (idxStart >= 0 && (idxEnd < 0 || idxStart < idxEnd)) {
                    idx = idxStart + openPo.Length - 1;
                    startPosStack.Push(idxStart);
                } else if (idxEnd >= 0) {
                    idx = idxEnd + closePo.Length - 1;
                    if (idxEnd >= 0) {
                        if (startPosStack.Count == 0) {
                            throw new Exception($"Invalid symbol {closePo} found at column {idx} (no corresponding {openPo}).");
                        }
                        var lastStartPos = startPosStack.Pop();
                        if (replacementFunction != null) {
                            // we need to replace this closed place holder
                            var variableName = osb.Substring(lastStartPos + openPo.Length, idxEnd - (lastStartPos + openPo.Length));
                            var variableValue = replacementFunction(variableName);
                            if (variableValue != null) {
                                if (variableValue.IndexOf(openPo, 0, comparison) >= 0) {
                                    throw new Exception($"The place holder value can't contain {openPo}.");
                                }
                                osb = osb.Remove(lastStartPos, idxEnd + closePo.Length - lastStartPos).Insert(lastStartPos, variableValue);
                                idx = lastStartPos;
                            }
                        }
                    }
                }
                if (maxDepth > 0 && startPosStack.Count > maxDepth) {
                    throw new Exception($"Max depth inclusion of {maxDepth} reached at column {idx}.");
                }
                idx++;
            } while (idx > 0 && idx <= osb.Length - 1);

            if (startPosStack.Count != 0) {
                throw new Exception($"Unbalanced number or {openPo} and {closePo}).");
            }

            return osb;
        }

        /// <summary>
        /// Converts a camel case string to a string separated with the given separator.
        /// Ex: from ThisSentence to this_sentence.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string CamelCaseToSeparator(this string str, string separator = "_") {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? separator + x.ToString() : x.ToString())).ToLower();
        }

        /// <summary>
        /// A simple quote to use for result display
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PrettyQuote(this string text) {
            return $"`{text}`";
        }

        /// <summary>
        /// A quoter function:
        /// - surround by double quote if the text contains spaces
        /// - escape double quote with another double quote
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static string ToQuotedArg(this string arg) {
            if (arg == null) {
                return null;
            }
            if (string.IsNullOrEmpty(arg)) {
                return @"""""";
            }
            var isQuoted = IsSurroundedWithDoubleQuotes(arg);
            var hasWhiteSpaces = false;

            var sb = new StringBuilder();

            for (int i = isQuoted ? 1 : 0; i < arg.Length - (isQuoted ? 1 : 0); ++i) {
                if (arg[i] == '"') {
                    sb.Append('"');
                } else if (char.IsWhiteSpace(arg[i])) {
                    hasWhiteSpaces = true;
                }
                sb.Append(arg[i]);
            }

            return hasWhiteSpaces ? $"\"{sb.Append('"')}" : sb.ToString();
        }

        /// <inheritdoc cref="ToQuotedArg"/>
        public static string ToQuotedArgs(this IEnumerable<string> args) => args == null ? null : string.Join(" ", args.Where(a => a != null).Select(ToQuotedArg));

        /// <summary>
        /// Remove double quotes from a string.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string StripQuotes(this string source) {
            if (string.IsNullOrEmpty(source)) {
                return source;
            }
            return source.Length > 1 && source[0] == source[source.Length - 1] && source[0] == '"' ? source.Length - 2 > 0 ? source.Substring(1, source.Length - 2) : "" : source;
        }

        /// <summary>
        /// Returns true if a string is surrounded with double quotes.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static bool IsSurroundedWithDoubleQuotes(string argument) {
            if (string.IsNullOrEmpty(argument) || argument.Length <= 1) {
                return false;
            }
            return argument[0] == '"' && argument[argument.Length - 1] == '"';
        }

        private static Regex _uriRegex;
        private static Regex UriRegex => _uriRegex ?? (_uriRegex = new Regex(@"^(?<baseUri>((?<protocol>[^:\/@]*):\/\/)?((?<user>[^:\/@]*)(:(?<pwd>[^:\/@]*))?@(?<host>[^:\/@]*)(:(?<port>[^:\/@]*))?|(?<host2>[^:\/@]*)(:(?<port2>[^:\/@]*))?))(?<path>\/.*)?$", RegexOptions.Compiled));

        /// <summary>
        /// Parses the given URI into strings.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="baseUri"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relativePath"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static bool ParseUri(this string uri, out string baseUri, out string protocol, out string userName, out string passWord, out string host, out int port, out string relativePath) {
            var match = UriRegex?.Match(uri.Replace("\\", "/"));
            if (match != null && match.Success) {
                baseUri = match.Groups["baseUri"].Value;
                relativePath = match.Groups["path"].Success ? match.Groups["path"].Value : null;
                protocol = match.Groups["protocol"].Success ? match.Groups["protocol"].Value : null;
                port = 0;
                if (match.Groups["user"].Success) {
                    userName = match.Groups["user"].Value;
                    passWord = match.Groups["pwd"].Success ? match.Groups["pwd"].Value : null;
                    host = match.Groups["host"].Value;
                    if (!string.IsNullOrWhiteSpace(match.Groups["port"].Value)) {
                        int.TryParse(match.Groups["port"].Value, out port);
                    }
                } else {
                    userName = null;
                    passWord = null;
                    host = match.Groups["host2"].Value;
                    if (!string.IsNullOrWhiteSpace(match.Groups["port2"].Value)) {
                        int.TryParse(match.Groups["port2"].Value, out port);
                    }
                }
                return true;
            }
            protocol = null;
            baseUri = null;
            relativePath = null;
            userName = null;
            passWord = null;
            host = null;
            port = 0;
            return false;
        }

        /// <summary>
        /// Parses the given WEB HTTP URI.
        /// </summary>
        /// <param name="httpUri"></param>
        /// <returns></returns>
        public static WebProxy ParseWebProxy(this string httpUri) {
            if (ParseUri(httpUri, out _, out string protocol, out string user, out string pwd, out string host, out int port, out _)) {
                return new WebProxy($"{host}:{(port > 0 ? port : protocol?.EndsWith("s", StringComparison.OrdinalIgnoreCase) ?? false ? 443 : 80)}") {
                    UseDefaultCredentials = false,
                    Credentials = string.IsNullOrEmpty(user) ? null : new NetworkCredential(user, pwd)
                };
            }
            return null;
        }

        /// <summary>
        /// Parse a web proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static bool ParseWebProxy(this string uri, out string host, out int port, out string user, out string pwd) {
            if (ParseUri(uri, out _, out string protocol, out user, out pwd, out host, out port, out _)) {
                if (port == 0) {
                    port = protocol?.EndsWith("s", StringComparison.OrdinalIgnoreCase) ?? false ? 443 : 80;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Parses the given FTP URI into strings.
        /// </summary>
        /// <param name="ftpUri"></param>
        /// <param name="ftpBaseUri"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static bool ParseFtpAddress(this string ftpUri, out string ftpBaseUri, out string userName, out string passWord, out string host, out int port, out string relativePath) {
            return ParseUri(ftpUri, out ftpBaseUri, out string protocol, out userName, out passWord, out host, out port, out relativePath) && !string.IsNullOrEmpty(protocol) && (protocol.Equals("ftp", StringComparison.OrdinalIgnoreCase) || protocol.Equals("ftps", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Tests whether or not a character is a letter from the ascii table.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsAsciiLetter(this char c) {
            return c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';
        }
    }
}
