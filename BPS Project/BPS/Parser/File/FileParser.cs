﻿/*
 * MIT License
 *
 * Copyright (c) 2021 Carlos Eduardo de Borba Machado
 *
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BPS UnitTest")]
namespace BPSLib.Parser.File
{
	/// <summary>
	/// Class <c>FileParser</c> manages the file parser.
	/// </summary>
	internal class FileParser
	{
		/// <summary>
		/// Generated BPSFile.
		/// </summary>
		internal BPSFile BPSFile { get; }

		/// <summary>
		/// To parse input.
		/// </summary>
		internal string Input { get; private set; }

		// control vars
		private List<Token> _tokens;
		private Token _curToken;
		private int _curIndex = -1;

		private string _key;
		private object _value;
		private readonly Stack<List<object>> _arrStack = new Stack<List<object>>();

		private const int CONTEXT_KEY = 0;
		private const int CONSTEXT_ARRAY = 1;
		private int _context = CONTEXT_KEY;

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		internal FileParser()
		{
			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

			BPSFile = new BPSFile();
			Input = "";
		}

		/// <summary>
		/// Constructor setting the input.
		/// </summary>
		/// <param name="input">the input to be parsed.</param>
		internal FileParser(string input)
		{
			CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

			BPSFile = new BPSFile();
			Input = input;
		}

		#endregion Constructors


		#region Methods

		#region Public

		/// <summary>
		/// Do the file parser.
		/// </summary>
		internal void Parse()
		{
			var lexer = new FileLexer(Input);
			lexer.Parse();
			_tokens = lexer.Tokens;
			Start();
		}

		#endregion Public

		#region Private

		private void Start()
		{
			NextToken();
			Statement();
			ConsumeToken(TokenCategory.EOF);
		}

		private void Statement()
		{
			switch (_curToken.Category)
			{
				case TokenCategory.KEY:
					Key();
					break;
				default:
					break;
			}
		}

		private void Key()
		{
			_key = _curToken.Image;
			NextToken();
			ConsumeToken(TokenCategory.DATA_SEP);
			Value();
			ConsumeToken(TokenCategory.END_OF_DATA);
			Statement();
		}

		private void Value()
		{
			switch (_curToken.Category)
			{
				case TokenCategory.OPEN_ARRAY:
					OpenArray();
					NextToken();
					Array();
					break;
				case TokenCategory.STRING:
					String();
					break;
				case TokenCategory.CHAR:
					Char();
					break;
				case TokenCategory.INTEGER:
					Integer();
					break;
				case TokenCategory.FLOAT:
					Float();
					break;
				case TokenCategory.BOOL:
					Bool();
					break;
				case TokenCategory.NULL:
					Null();
					break;
				default:
					throw new Exception("Invalid token '" + _curToken.Image + "' encountered.");
			}
		}

		private void Array()
		{
			switch (_curToken.Category)
			{
				case TokenCategory.OPEN_ARRAY:
					OpenArray();
					NextToken();
					Array();
					break;
				case TokenCategory.STRING:
					String();
					ArraySel();
					break;
				case TokenCategory.CHAR:
					Char();
					ArraySel();
					break;
				case TokenCategory.INTEGER:
					Integer();
					ArraySel();
					break;
				case TokenCategory.FLOAT:
					Float();
					ArraySel();
					break;
				case TokenCategory.BOOL:
					Bool();
					ArraySel();
					break;
				case TokenCategory.NULL:
					Null();
					ArraySel();
					break;
				default:
					throw new Exception("Invalid token '" + _curToken.Image + "' encountered.");
			}
		}

		private void ArraySel()
		{
			switch (_curToken.Category)
			{
				case TokenCategory.ARRAY_SEP:
					NextToken();
					Array();
					break;
				case TokenCategory.CLOSE_ARRAY:
					CloseArray();
					NextToken();
					ArraySel();
					break;
				case TokenCategory.END_OF_DATA:
				case TokenCategory.EOF:
					break;
				default:
					throw new Exception("Invalid token '" + _curToken.Image + "' encountered.");
			}
		}

		private void String()
		{
			_value = _curToken.Image.Substring(1, _curToken.Image.Length - 2);
			SetValue();
		}

		private void Char()
		{
			_value = char.Parse(_curToken.Image.Substring(1, _curToken.Image.Length - 2).Replace("\\", string.Empty));
			SetValue();
		}

		private void Integer()
		{
			_value = int.Parse(_curToken.Image);
			SetValue();
		}

		private void Float()
		{
			_value = float.Parse(_curToken.Image);
			SetValue();
		}

		private void Bool()
		{
			_value = bool.Parse(_curToken.Image);
			SetValue();
		}

		private void Null()
		{
			_value = null;
			SetValue();
		}

		private void SetValue()
		{
			if (_context == CONSTEXT_ARRAY)
			{
				_arrStack.Peek().Add(_value);
			}
			else
			{
				BPSFile.Add(_key, _value);
			}
			NextToken();
		}

		private void OpenArray()
		{
			if (_arrStack.Count == 0)
			{
				_context = CONSTEXT_ARRAY;
				_arrStack.Push(new List<object>());
			}
			else
			{
				var newD = new List<object>();
				_arrStack.Peek().Add(newD);
				_arrStack.Push(newD);
			}
		}

		private void CloseArray()
		{
			if (_arrStack.Count > 1)
			{
				_arrStack.Pop();
			}
			else
			{
				_context = CONTEXT_KEY;
				BPSFile.Add(_key, _arrStack.Pop());
			}
		}

		// parser controls

		private void NextToken()
		{
			if (++_curIndex < _tokens.Count)
			{
				_curToken = _tokens[_curIndex];
			}
		}

		private void ConsumeToken(TokenCategory category)
		{
			if (!_curToken.Category.Equals(category))
			{
				throw new Exception("Invalid token '" + _curToken.Image + "' encountered.");
			}
			NextToken();
		}

		#endregion Private

		#endregion Methods

	}
}
