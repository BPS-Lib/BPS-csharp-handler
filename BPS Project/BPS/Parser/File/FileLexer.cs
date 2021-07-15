﻿/*
 * MIT License
 *
 * Copyright (c) 2021 Carlos Eduardo de Borba Machado
 *
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BPS UnitTest")]
namespace BPSLib.Parser.File
{
	/// <summary>
	/// Class <c>FileLexer</c> manage lexical analisys from file parser.
	/// </summary>
	internal class FileLexer
	{
		// TODO: implements line and column error
		// TODO: implements expecteds token

		#region Vars

		/// <summary>
		/// Generated list of tokens.
		/// </summary>
		internal List<Token> Tokens { get; }

		// control vars
		private readonly string _input = "";
		private char _curChar;
		private int _curIndex = 0;
		private int _curLine = 0;
		private int _curCollumn = -1;

		#endregion Vars


		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		internal FileLexer()
		{
			Tokens = new List<Token>();
		}

		/// <summary>
		/// Constructor setting the input.
		/// </summary>
		/// <param name="input">the input to be parsed.</param>
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
					if (_curChar.Equals(Symbols.NEWLINE))
					{
						++_curLine;
						_curCollumn = -1;
					}
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
								throw new Exception("Double comma encountered at line " + _curLine + " and collumn " + _curCollumn + ".");
							}
						}
						while (!EndOfInput() && Symbols.IsSkip(_curChar))
						{
							NextChar();
						}
						if (_curChar.Equals(Symbols.SEMICOLON))
						{
							break;
						}
						var lexeme = _curChar.ToString();

						// open array
						if (_curChar.Equals(Symbols.LEFT_BRACKETS))
						{
							Tokens.Add(new Token(TokenCategory.OPEN_ARRAY, lexeme, _curLine, _curCollumn));
						}
						// close array
						else if (_curChar.Equals(Symbols.RIGHT_BRACKETS))
						{
							Tokens.Add(new Token(TokenCategory.CLOSE_ARRAY, lexeme, _curLine, _curCollumn));
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
								Tokens.Add(new Token(TokenCategory.STRING, lexeme, _curLine, _curCollumn));
							}
							// numeric
							else if (char.IsDigit(_curChar) || _curChar.Equals(Symbols.DOT) || _curChar.Equals(Symbols.MINUS))
							{
								var dotted = _curChar.Equals(Symbols.DOT);
								NextChar();
								while (!EndOfInput() && (char.IsDigit(_curChar) || _curChar.Equals(Symbols.DOT)))
								{
									if (_curChar.Equals(Symbols.DOT))
									{
										if (dotted)
										{
											throw new Exception("Double dot encountered at line " + _curLine + " and collumn " + _curCollumn + ".");
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
								if (lexeme.Contains(Symbols.DOT.ToString()))
								{
									Tokens.Add(new Token(TokenCategory.FLOAT, lexeme, _curLine, _curCollumn));
								}
								else
								{
									Tokens.Add(new Token(TokenCategory.INTEGER, lexeme, _curLine, _curCollumn));
								}
								PreviousChar();
							}
							// boolean
							else if (_curChar.Equals('t') || _curChar.Equals('f'))
							{
								NextChar();
								while (!EndOfInput() && char.IsLetter(_curChar))
								{
									lexeme += _curChar;
									NextChar();
								}
								// true or false
								if (lexeme.Equals("true") || lexeme.Equals("false"))
								{
									Tokens.Add(new Token(TokenCategory.BOOL, lexeme, _curLine, _curCollumn));
								}
								else
								{
									throw new Exception("Invalid value: '" + lexeme + "' encountered at line " + _curLine + " and collumn " + _curCollumn + ". Expected: 'true' or 'false'.");
								}
								PreviousChar();
							}
							// null
							else if (_curChar.Equals('n'))
							{
								NextChar();
								while (!EndOfInput() && char.IsLetter(_curChar))
								{
									lexeme += _curChar;
									NextChar();
								}
								if (lexeme.Equals("null"))
								{
									Tokens.Add(new Token(TokenCategory.NULL, lexeme, _curLine, _curCollumn));
								}
								else
								{
									throw new Exception("Invalid value: '" + lexeme + "' encountered at line " + _curLine + " and collumn " + _curCollumn + ". Expected: 'null'.");
								}
								PreviousChar();
							}
							else
							{
								throw new Exception("Invalid character: '" + _curChar + "' encountered at line " + _curLine + " and collumn " + _curCollumn + ". Expected: 'null'.");
							}
						}
					}
				}
				// if is key
				else if (char.IsLetter(_curChar) || _curChar.Equals('_'))
				{
					var lexeme = _curChar.ToString();
					NextChar();

					while (!EndOfInput() && !_curChar.Equals(Symbols.COLON) && (_curChar.Equals('_') || char.IsLetterOrDigit(_curChar)))
					{
						lexeme += _curChar;
						NextChar();
					}
					PreviousChar();
					Tokens.Add(new Token(TokenCategory.KEY, lexeme, _curLine, _curCollumn));
				}
				else
				{
					throw new Exception("Invalid character: '" + _curChar + "' encountered at line " + _curLine + " and collumn " + _curCollumn + ". Expected: 'null'.");
				}
			}

			Tokens.Add(new Token(TokenCategory.EOF, "", _curLine, _curCollumn));
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
				_curChar = _input[_curIndex++];
				++_curCollumn;
			}
		}

		/// <summary>
		/// Get the previous char to <v>_curChar</v>.
		/// </summary>
		private void PreviousChar()
		{
			if (_curIndex - 2 > 0)
			{
				_curChar = _input[--_curIndex - 1];
				--_curCollumn;
			}
		}

		#endregion Private

		#endregion Methods
	}
}
