﻿/*
 * MIT License
 *
 * Copyright (c) 2021 Carlos Eduardo de Borba Machado
 *
 */

using System;
using System.Collections.Generic;

namespace BPSLib.Parser.File
{
	/// <summary>
	/// Class <c>FileLexer</c> manage lexical analisys from file parser.
	/// </summary>
	internal class FileLexer
	{
		// TODO: implements line and colllumn error
		// TODO: implements expecteds token

		#region Vars

		/// <summary>
		/// Generated list of tokens.
		/// </summary>
		internal List<Token> Tokens { get; }

		// control vars
		private readonly string _input;
		private string _curChar;
		private int _curIndex = 0;

		#endregion Vars


		#region Constructors

		internal FileLexer(string input)
		{
			Tokens = new List<Token>();
			_input = input;
		}

		#endregion Constructors


		#region Methods

		#region Public

		/// <summary>
		/// Do the lexical analisys of a BPS string document.
		/// </summary>
		internal void Parse()
		{
			// maybe need a refactoring
			while (!EndOfInput())
			{
				NextChar();

				// to skip the skip chars
				if (Symbols.IsSkip(_curChar))
				{
					continue;
				}

				// to skip comments
				if (_curChar.Equals(Symbols.HASH))
				{
					while (!EndOfInput() && !_curChar.Equals(Symbols.NEWLINE))
					{
						NextChar();
					}
				}
				// if is value
				else if (_curChar.Equals(Symbols.COLON))
				{
					while (!EndOfInput() && !_curChar.Equals(Symbols.SEMICOLON))
					{
						NextChar();
						if (_curChar.Equals(Symbols.COMMA))
						{
							NextChar();
							if (_curChar.Equals(Symbols.COMMA))
							{
								throw new Exception("[double comma] - Invalid character '" + _curChar + "' encountered.");
							}
						}
						while (!EndOfInput() && (_curChar.Equals(Symbols.SPACE) || _curChar.Equals(Symbols.NEWLINE)))
						{
							NextChar();
						}
						if (_curChar.Equals(Symbols.SEMICOLON))
						{
							break;
						}
						var lexeme = _curChar;

						// open array
						if (_curChar.Equals(Symbols.LEFT_BRACKETS))
						{
							Tokens.Add(new Token(TokenCategory.OPEN_ARRAY, lexeme));
						}
						// close array
						else if (_curChar.Equals(Symbols.RIGHT_BRACKETS))
						{
							Tokens.Add(new Token(TokenCategory.CLOSE_ARRAY, lexeme));
						}
						// is a value
						else
						{
							// string
							if (_curChar.Equals(Symbols.QUOTE) || _curChar.Equals(Symbols.DQUOTE))
							{
								var closeQuote = _curChar;
								var beforeChar = _curChar;
								NextChar();
								while (!EndOfInput() && (!_curChar.Equals(closeQuote) && !beforeChar.Equals("\\")))
								{
									beforeChar = _curChar;
									lexeme += _curChar;
									NextChar();
								}
								lexeme += _curChar;
								Tokens.Add(new Token(TokenCategory.STRING, lexeme));
							}
							// numeric
							else if (char.IsDigit(char.Parse(_curChar)) || _curChar.Equals(Symbols.DOT) || _curChar.Equals(Symbols.MINUS))
							{
								var dotted = _curChar.Equals(Symbols.DOT);
								NextChar();
								while (!EndOfInput() && (char.IsDigit(char.Parse(_curChar)) || _curChar.Equals(Symbols.DOT)))
								{
									if (_curChar.Equals(Symbols.DOT))
									{
										if (dotted)
										{
											throw new Exception("[double dot] - Invalid character '" + _curChar + "' encountered.");
										}
										else
										{
											dotted = true;
										}
									}
									lexeme += _curChar;
									NextChar();
								}
								// float or int
								if (lexeme.Contains(Symbols.DOT))
								{
									Tokens.Add(new Token(TokenCategory.FLOAT, lexeme));
								}
								else
								{
									Tokens.Add(new Token(TokenCategory.INTEGER, lexeme));
								}
								PreviousChar();
							}
							// boolean
							else if (_curChar.Equals("t") || _curChar.Equals("f"))
							{
								NextChar();
								while (!EndOfInput() && char.IsLetter(char.Parse(_curChar)))
								{
									lexeme += _curChar;
									NextChar();
								}
								// true or false
								if (lexeme.Equals("true"))
								{
									Tokens.Add(new Token(TokenCategory.TRUE, lexeme));
								}
								else if (lexeme.Equals("false"))
								{
									Tokens.Add(new Token(TokenCategory.FALSE, lexeme));
								}
								else
								{
									throw new Exception("Invalid lexeme encountered: '" + lexeme + "'.\nExpected: true or false;");
								}
								PreviousChar();
							}
							else
							{
								throw new Exception("Invalid character '" + _curChar + "' encountered.");
							}
						}
					}
				}
				// if is key
				else if (char.IsLetter(char.Parse(_curChar)) || _curChar.Equals("_"))
				{
					var lexeme = _curChar;
					NextChar();

					while (!EndOfInput() && !_curChar.Equals(Symbols.COLON) && (_curChar.Equals("_") || char.IsLetterOrDigit(char.Parse(_curChar))))
					{
						lexeme += _curChar;
						NextChar();
					}
					_curIndex--;
					Tokens.Add(new Token(TokenCategory.KEY, lexeme));
				}
				else
				{
					throw new Exception("Invalid character '" + _curChar + "' encountered.");
				}
			}

			Tokens.Add(new Token(TokenCategory.EOF, ""));
		}

		#endregion Public


		#region Private

		/// <summary>
		/// Verify if the input is in the end.
		/// </summary>
		/// <returns>True if is in the end.</returns>
		private bool EndOfInput()
		{
			return _curIndex >= _input.Length;
		}

		/// <summary>
		/// Get the next char to <v>_curChar</v>.
		/// </summary>
		private void NextChar()
		{
			if (_curIndex < _input.Length)
			{
				_curChar = _input[_curIndex++].ToString();
			}
		}

		/// <summary>
		/// Get the previous char to <v>_curChar</v>.
		/// </summary>
		private void PreviousChar()
		{
			if (_curIndex - 2 > 0)
			{
				_curChar = _input[--_curIndex - 1].ToString();
			}
		}

		#endregion Private

		#endregion Methods
	}
}
