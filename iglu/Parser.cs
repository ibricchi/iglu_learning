﻿using Iglu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iglu
{
	class Parser
	{
		private class ParseError : Exception { }

		private readonly List<Token> tokens;
		private int current = 0;

		public Parser(List<Token> tokens)
		{
			this.tokens = tokens;
		}

		public Expr Parse()
		{
			try
			{
				return Expression();
			}
			catch (ParseError error)
			{
				return null;
			}
		}

		private bool Match(params TokenType[] types)
		{
			foreach(TokenType type in types)
			{
				if(Check(type))
				{
					Advance();
					return true;
				}
			}

			return false;
		}

		private bool Check(TokenType type)
		{
			if(IsAtEnd())
			{
				return false;
			}
			else
			{
				return Peek().type == type;
			}
		}

		private Token Advance()
		{
			if(!IsAtEnd())
			{
				current++;
			}
			return Previous();
		}

		private bool IsAtEnd()
		{
			return Peek().type == TokenType.EOF;
		}

		private Token Peek()
		{
			return tokens[current];
		}

		private Token Previous()
		{
			return tokens[current - 1];
		}

		private Token Consume(TokenType type, string message)
		{
			if(Check(type))
			{
				return Advance();
			}
			else
			{
				throw Error(Peek(), message);
			}
		}

		private ParseError Error(Token token, String message)
		{
			Program.Error(token, message);
			return new ParseError();
		}

		private void Synchronize()
		{
			Advance();

			while(!IsAtEnd())
			{
				if (Previous().type == TokenType.SEMICOLON)
				{
					return;
				}
				else
				{
					switch (Peek().type)
					{
						case TokenType.CLASS:
						case TokenType.COMMA:
						case TokenType.DEF:
						case TokenType.FOR:
						case TokenType.IF:
						case TokenType.LET:
						case TokenType.PRINT:
						case TokenType.RETURN:
						case TokenType.WHILE:
							return;
					}

					Advance();
				}
			}
		}

		private Expr Expression()
		{
			return Commation();
		}

		private Expr Commation() // terrible name I know
		{
			Expr expr = Ternary();

			while(Match(TokenType.COMMA))
			{
				Token oper = Previous();
				Expr right = Ternary();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Ternary()
		{
			Expr expr = Equality();

			while (Match(TokenType.QUESTION))
			{
				Token oper1 = Previous();
				Expr ifTrue = Expression();
				Consume(TokenType.COLON, "Expecting ':' as part of ternary operator.");
				Token oper2 = Previous();
				Expr ifFalse = Equality();

				Expr ifs = new Expr.Binary(ifTrue, oper2, ifFalse);
				expr = new Expr.Binary(expr, oper1, ifs);
			}

			return expr;
		}

		private Expr Equality()
		{
			Expr expr = Comparison();

			while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
			{
				Token oper = Previous();
				Expr right = Comparison();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Comparison()
		{
			Expr expr = Addition();

			while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
			{
				Token oper = Previous();
				Expr right = Addition();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Addition()
		{
			Expr expr = Multiplication();

			while(Match(TokenType.MINUS, TokenType.PLUS))
			{
				Token oper = Previous();
				Expr right = Addition();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Multiplication()
		{
			Expr expr = Unary();

			while(Match(TokenType.SLASH, TokenType.STAR))
			{
				Token oper = Previous();
				Expr right = Unary();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Unary()
		{
			if(Match(TokenType.BANG, TokenType.MINUS))
			{
				Token oper = Previous();
				Expr right = Unary();
				return new Expr.Unary(oper, right);
			}
			else
			{
				return Primary();
			}
		}

		private Expr Primary()
		{
			if(Match(TokenType.FALSE))
			{
				return new Expr.Literal(false);
			}
			else if(Match(TokenType.TRUE))
			{
				return new Expr.Literal(true);
			}
			else if(Match(TokenType.NULL))
			{
				return new Expr.Literal(null);
			}
			else if(Match(TokenType.NUMBER, TokenType.STRING))
			{
				return new Expr.Literal(Previous().literal);
			}
			else if(Match(TokenType.LEFT_PAREN))
			{
				Expr expr = Expression();
				Consume(TokenType.RIGHT_PAREN, "Expecting ')' after expression.");
				return new Expr.Grouping(expr);
			}
			else
			{
				throw Error(Peek(), "Expected an expression.");
			}
		}
	}
}
