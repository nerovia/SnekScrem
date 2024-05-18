using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnekScrem
{
	static class ConsoleCanvas
	{
		static ConsoleColor Foreground;
		static ConsoleColor Background;
		static Point CursorPosition = Point.Empty;
		static HashSet<Point> DrawBuffer = new();
		static HashSet<Point> EraseBuffer = new();

		static public Size Size => new(Console.WindowWidth, Console.WindowHeight); 

		static public void Begin()
		{
			Foreground = Console.ForegroundColor;
			Background = Console.BackgroundColor;
			Console.CursorVisible = false;
			(CursorPosition.X, CursorPosition.Y) = Console.GetCursorPosition();
		}

		/// <summary>
		/// Resets the Console to its initial state when <see cref="Begin"/> was last called
		/// </summary>
		static public void End()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			Console.CursorVisible = true;
			Console.SetCursorPosition(CursorPosition.X, CursorPosition.Y);
		}

		/// <summary>
		/// Erases everything that has been drawn before the
		/// </summary>
		static public void Clean()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			foreach (var pos in EraseBuffer.Except(DrawBuffer))
			{
				Console.SetCursorPosition(pos.X, pos.Y);
				Console.Write(' ');
			}
			(DrawBuffer, EraseBuffer) = (EraseBuffer, DrawBuffer);
			DrawBuffer.Clear();
		}

		static public void Draw(Point pos, char c) => Draw(pos, c, null, null);

		static public void Draw(Point pos, char c, ConsoleColor? foreground, ConsoleColor? background)
		{
			DrawBuffer.Add(pos);
			Console.ForegroundColor = foreground ?? Foreground;
			Console.BackgroundColor = background ?? Background;
			Console.SetCursorPosition(pos.X, pos.Y);
			Console.Write(c);
		}

	}
}
