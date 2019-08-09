#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ProcessArgs.cs) is part of Oetools.Utilities.
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
using System.Text;
using Oetools.Utilities.Lib.Extension;
using Oetools.Utilities.Lib.ParameterStringParser;

namespace Oetools.Utilities.Lib {

    /// <summary>
    /// A collection of arguments for a process.
    /// The main point of this class is to provide a clean way to feed arguments to a <see cref="ProcessIo"/>.
    /// Using this class as an input instead of a string, we are able to control how the arguments are sent to the called program and
    /// escape them correctly depending on the executing platform.
    /// Also, there is no need to worry about how to write the argument string prior to using it in a <see cref="ProcessIo"/>.
    /// </summary>
    public class ProcessArgs : IEnumerable<string> {

        /// <summary>
        /// The list of tokens composing the process arguments.
        /// </summary>
        protected IList<string> tokens = new List<string>();

        /// <summary>
        /// New process args.
        /// </summary>
        public ProcessArgs() { }

        /// <summary>
        /// Append a new argument.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public virtual ProcessArgs Append(string arg) {
            if (arg != null) {
                tokens.Add(arg);
            }
            return this;
        }

        /// <summary>
        /// Append a collection of arguments.
        /// </summary>
        /// <param name="processArgs"></param>
        /// <returns></returns>
        public virtual ProcessArgs Append(ProcessArgs processArgs) {
            if (processArgs != null) {
                foreach (var arg in processArgs) {
                    Append(arg);
                }
            }
            return this;
        }

        /// <summary>
        /// Append one or more arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual ProcessArgs Append(params object[] args) {
            if (args != null) {
                foreach (var o in args) {
                    Append(o);
                }
            }
            return this;
        }

        /// <summary>
        /// Append one or more arguments.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public virtual ProcessArgs Append(object arg) {
            if (arg != null) {
                switch (arg) {
                    case ProcessArgs processArgs:
                        Append(processArgs);
                        break;
                    case string stringArg:
                        Append(stringArg);
                        break;
                    case IEnumerable listItem:
                        foreach (var item in listItem) {
                            Append(item);
                        }
                        break;
                    default:
                        Append(arg.ToString());
                        break;
                }
            }
            return this;
        }

        /// <summary>
        /// Append an array of arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual ProcessArgs Append(string[] args) {
            if (args != null) {
                foreach (var stringArg in args) {
                    Append(stringArg);
                }
            }
            return this;
        }

        /// <summary>
        /// Append arguments from an argument string.
        /// This should be used when a user passes several arguments via a full string.
        /// Double quotes are used to specify arguments containing spaces.
        /// To use a double quote inside a quoted argument, double it.
        /// e.g. -opt "my ""quoted"" value" is a correctly formatted string.
        /// </summary>
        /// <param name="argString"></param>
        /// <returns></returns>
        public virtual ProcessArgs AppendFromQuotedArgs(string argString) {
            if (argString != null) {
                var tokenizer = ParameterStringTokenizer.New(argString);
                while (tokenizer.MoveToNextToken()) {
                    var token = tokenizer.PeekAtToken(0);
                    if (token is ParameterStringTokenOption || token is ParameterStringTokenValue) {
                        Append(token.Value);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Returns the value of the argument located just after <paramref name="option"/>.
        /// Of null if not found.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public virtual string GetValueForOption(string option, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase) {
            for (int i = 0; i < tokens.Count - 1; i++) {
                if (tokens[i].Equals(option, comparison)) {
                    return tokens[i + 1];
                }
            }
            return null;
        }

        /// <summary>
        /// Remove the argument.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="comparison"></param>
        public virtual void Remove(string arg, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase) {
            var toRem = new List<int>();
            for (int i = tokens.Count - 1; i >= 0; i--) {
                if (tokens[i].Equals(arg, comparison)) {
                    toRem.Add(i);
                }
            }
            foreach (var i in toRem) {
                tokens.RemoveAt(i);
            }
        }

        /// <summary>
        /// Prepare a string representing the arguments of a command line application.
        /// Format each argument so that it is correctly interpreted by the receiving program.
        /// </summary>
        /// <param name="isWindows"></param>
        /// <returns></returns>
        public string ToCliArgs(bool? isWindows = null) {
            return string.Join(" ", this.Where(a => a != null).Select(a => ToCliArg(a, isWindows)));
        }


        /// <summary>
        /// String representation of those arguments. Arguments with spaces are quoted.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.ToQuotedArgs();

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator() => tokens.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Prepare a string representing an argument of a cmd line interface so that it is interpreted as a single argument.
        /// Uses double quote when the string contains whitespaces.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isWindows"></param>
        /// <remarks>
        /// In linux, you escape double quotes with \"
        /// In windows, you escape doubles quotes with \" ("" is also correct but we don't do that here)
        /// In linux, you need to escape \ with \\ but not in windows
        /// </remarks>
        /// <returns></returns>
        public static string ToCliArg(string text, bool? isWindows = null) {
            if (text == null) {
                return null;
            }
            if (string.IsNullOrEmpty(text)) {
                return @"""""";
            }

            var hasWhiteSpaces = false;

            var sb = new StringBuilder();

            if (isWindows ?? Utils.IsRuntimeWindowsPlatform) {
                // https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/
                var textLength = text.Length;
                for (int i = 0; i < textLength; ++i) {
                    var backslashes = 0;

                    // Consume all backslashes
                    while (i < textLength && text[i] == '\\') {
                        backslashes++;
                        i++;
                    }

                    if (i == textLength) {
                        if (hasWhiteSpaces) {
                            // Escape any backslashes at the end of the arg when the argument is also quoted.
                            // This ensures the outside quote is interpreted as an argument delimiter
                            sb.Append('\\', 2 * backslashes);
                        } else {
                            // At then end of the arg, which isn't quoted,
                            // just add the backslashes, no need to escape
                            sb.Append('\\', backslashes);
                        }
                    } else if (text[i] == '"') {
                        // Escape any preceding backslashes and the quote
                        sb.Append('\\', 2 * backslashes + 1);
                        sb.Append('"');
                    } else {
                        if (char.IsWhiteSpace(text[i])) {
                            hasWhiteSpaces = true;
                        }
                        // Output any consumed backslashes and the character
                        sb.Append('\\', backslashes);
                        sb.Append(text[i]);
                    }
                }

            } else {
                for (int i = 0; i < text.Length; ++i) {
                    if (text[i] == '"') {
                        sb.Append('\\');
                    } else if (text[i] == '\\') {
                        sb.Append('\\');
                    } else if (char.IsWhiteSpace(text[i])) {
                        hasWhiteSpaces = true;
                    }
                    sb.Append(text[i]);
                }
            }

            return hasWhiteSpaces ? $"\"{sb.Append('"')}" : sb.ToString();
        }
    }
}
