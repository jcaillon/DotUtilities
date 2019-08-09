#region header
// ========================================================================
// Copyright (c) 2019 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParameterStringTokenizer.cs) is part of DotUtilities.
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
using System.Text;
using DotUtilities.Extensions;
using DotUtilities.ParameterString;

namespace DotUtilities {

    /// <summary>
    /// This class "tokenize" the input data into tokens of various types, it implements a visitor pattern.
    /// </summary>
    /// <remarks>
    /// This class can tokenize quoted arguments (<see cref="StringExtensions.ToQuotedArgs"/>).
    /// Its goal is to obtain executable arguments from a string passed by the user.
    /// - escape white spaces with " (e.g. "my value")
    /// - expect " inside " to be escaped with "" (e.g. "my ""quoted"" value")
    /// can read:
    /// "value ""quoted""" -> value "quoted"
    /// "-option" -> -option
    /// -option="my value with space" -> -option=my value with space
    /// opt,"my value"," my ""pass" -> opt,my value, my "pass
    /// </remarks>
    public class ParameterStringTokenizer {

        /// <summary>
        /// New instance, immediately tokenize the <paramref name="data"/> string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ParameterStringTokenizer New(string data) {
            var obj = new ParameterStringTokenizer();
            obj.Start(data);
            return obj;
        }

        /// <summary>
        /// End of file char.
        /// </summary>
        protected const char Eof = (char) 0;

        /// <summary>
        /// The data to read.
        /// </summary>
        protected string _data;

        /// <summary>
        /// The lenght of the <see cref="_data"/>.
        /// </summary>
        protected int _dataLength;

        /// <summary>
        /// Current cursor position.
        /// </summary>
        protected int _pos;

        /// <summary>
        /// Starting cursor position.
        /// </summary>
        protected int _startPos;

        /// <summary>
        /// The token position for the visitor pattern.
        /// </summary>
        protected int _tokenPos = -1;

        /// <summary>
        /// The list of tokens generated.
        /// </summary>
        protected List<ParameterStringToken> _tokenList;

        /// <summary>
        /// Returns the tokens list
        /// </summary>
        public List<ParameterStringToken> TokensList => _tokenList;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ParameterStringTokenizer() { }

        /// <summary>
        /// Tokenize the data.
        /// </summary>
        /// <param name="data"></param>
        protected void Start(string data) {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _dataLength = _data.Length;
            Tokenize();
        }

        /// <summary>
        /// Move the cursor to the first token.
        /// </summary>
        public virtual void MoveToFirstToken() {
            _tokenPos = 0;
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// Move to the next token, return true if it can.
        /// </summary>
        public virtual bool MoveToNextToken() {
            return ++_tokenPos < _tokenList.Count;
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// peek at the current pos + x token of the list, returns a new TokenEof if can't find
        /// </summary>
        public virtual ParameterStringToken PeekAtToken(int x) {
            return _tokenPos + x >= _tokenList.Count || _tokenPos + x < 0 ? new ParameterStringTokenEof("") : _tokenList[_tokenPos + x];
        }

        /// <summary>
        /// To use this lexer as an enumerator,
        /// peek at the current pos + x token of the list, returns a new TokenEof if can't find
        /// </summary>
        public virtual ParameterStringToken MoveAndPeekAtToken(int x) {
            _tokenPos += x;
            return _tokenPos >= _tokenList.Count || _tokenPos < 0 ? new ParameterStringTokenEof("") : _tokenList[_tokenPos];
        }

        /// <summary>
        /// Call this method to actually tokenize the string
        /// </summary>
        protected void Tokenize() {
            if (_data == null) {
                return;
            }

            if (_tokenList == null) {
                _tokenList = new List<ParameterStringToken>();
            }

            ParameterStringToken parameterStringToken;
            do {
                _startPos = _pos;
                parameterStringToken = GetNextToken();
                _tokenList.Add(parameterStringToken);
            } while (!(parameterStringToken is ParameterStringTokenEof));

            // clean
            _data = null;
        }

        /// <summary>
        /// Peek forward x chars
        /// </summary>
        protected char PeekAtChr(int x) {
            return _pos + x >= _dataLength ? Eof : _data[_pos + x];
        }

        /// <summary>
        /// peek backward x chars
        /// </summary>
        protected char PeekAtChrReverse(int x) {
            return _pos - x < 0 ? Eof : _data[_pos - x];
        }

        /// <summary>
        /// Read to the next char,
        /// indirectly adding the current char (_data[_pos]) to the current token
        /// </summary>
        protected void ReadChr() {
            _pos++;
        }

        /// <summary>
        /// Returns the current value of the token
        /// </summary>
        /// <returns></returns>
        protected string GetTokenValue() {
            return _data.Substring(_startPos, _pos - _startPos);
        }

        /// <summary>
        /// returns the next token of the string
        /// </summary>
        /// <returns></returns>
        protected virtual ParameterStringToken GetNextToken() {
            var ch = PeekAtChr(0);

            // END OF FILE reached
            if (ch == Eof)
                return new ParameterStringTokenEof(GetTokenValue());

            if (char.IsWhiteSpace(ch)) {
                return CreateWhitespaceToken();
            }
            return CreateToken(ch);
        }

        /// <summary>
        /// Is the char an option character.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        protected virtual bool IsOptionCharacter(char ch) {
            return ch == '-';
        }

        /// <summary>
        /// Read and create a whitespace token.
        /// </summary>
        /// <returns></returns>
        protected virtual ParameterStringToken CreateWhitespaceToken() {
            ReadChr();
            while (true) {
                var ch = PeekAtChr(0);
                if (ch == '\t' || ch == ' ' || ch == '\r' || ch == '\n')
                    ReadChr();
                else
                    break;
            }
            return new ParameterStringTokenWhiteSpace(GetTokenValue());
        }

        /// <summary>
        /// Create a new token by reading characters.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        protected virtual ParameterStringToken CreateToken(char ch) {
            var openedQuote = false;
            var sb = new StringBuilder();

            while (true) {
                ch = PeekAtChr(0);
                if (ch == Eof) {
                    break;
                }

                // quote char
                if (ch == '"') {
                    ReadChr();
                    ch = PeekAtChr(0);
                    if (ch == Eof) {
                        break;
                    }
                    if (ch == '"') {
                        sb.Append(ch);
                        ReadChr();
                        // correctly escaped quote, continue
                        continue;
                    }
                    openedQuote = !openedQuote;
                }

                if (!openedQuote && char.IsWhiteSpace(ch)) {
                    break;
                }

                sb.Append(ch);
                ReadChr();
            }

            var value = sb.ToString();
            return IsOptionCharacter(value[0]) ? (ParameterStringToken) new ParameterStringTokenOption(value) : new ParameterStringTokenValue(value);
        }

    }
}
