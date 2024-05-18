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
	class ConsoleCanvas
	{
		ConsoleColor Foreground;
		ConsoleColor Background;
		Point CursorPosition = Point.Empty;
		HashSet<Point> DrawBuffer = new();
		HashSet<Point> EraseBuffer = new();

		public Size Size => new(Console.WindowWidth, Console.WindowHeight); 

		public void Begin()
		{
			Foreground = Console.ForegroundColor;
			Background = Console.BackgroundColor;
			Console.CursorVisible = false;
			(CursorPosition.X, CursorPosition.Y) = Console.GetCursorPosition();
		}

		public void End()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			Console.CursorVisible = true;
			Console.SetCursorPosition(CursorPosition.X, CursorPosition.Y);
		}

		public void Clean()
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

		public void Draw(Point pos, char c, bool transient = true) => Draw(pos, c, null, null);

		public void Draw(Point pos, char c, ConsoleColor? foreground, ConsoleColor? background)
		{
			DrawBuffer.Remove(pos);
			DrawBuffer.Add(pos);
			Console.ForegroundColor = foreground ?? Foreground;
			Console.BackgroundColor = background ?? Background;
			Console.SetCursorPosition(pos.X, pos.Y);
			Console.Write(c);
		}

	}
}
