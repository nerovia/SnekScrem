using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SnekScrem
{
	sealed class ConsoleFrame
	{
		public static ConsoleFrame Current { get; private set; } = null!;

		private ConsoleFrame()
		{
			Foreground = Console.ForegroundColor;
			Background = Console.BackgroundColor;
			(CursorPosition.X, CursorPosition.Y) = Console.GetCursorPosition();
			Console.CursorVisible = false;
		}

		ConsoleColor Foreground;
		ConsoleColor Background;
		Point CursorPosition = Point.Empty;
		HashSet<Cell> DrawBuffer = new(CellPositionComparer.Default);
		HashSet<Cell> EraseBuffer = new(CellPositionComparer.Default);

		public static ConsoleFrame Begin()
		{
			if (Current != null)
				throw new Exception();
			return Current = new ConsoleFrame();
		}

		/// <summary>
		/// Resets the Console to its initial state when <see cref="Begin"/> was last called
		/// </summary>
		public void End()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			Console.CursorVisible = true;
			Console.SetCursorPosition(CursorPosition.X, CursorPosition.Y);
			Current = null!;
		}

		/// <summary>
		/// Erases everything that has been drawn before the
		/// </summary>
		public ConsoleFrame Commit()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			foreach (var cell in EraseBuffer.Except(DrawBuffer, CellPositionComparer.Default))
			{
				Console.SetCursorPosition(cell.Position.X, cell.Position.Y);
				Console.Write(' ');
			}

			foreach (var color in DrawBuffer.GroupBy(it => (it.Foreground, it.Background)))
			{
				Console.ForegroundColor = color.Key.Foreground ?? Foreground;
				Console.BackgroundColor = color.Key.Background ?? Background;
				foreach (var cell in color)
				{
					Console.SetCursorPosition(cell.Position.X, cell.Position.Y);
					Console.Write(cell.Glyph);
				}
			}

			(DrawBuffer, EraseBuffer) = (EraseBuffer, DrawBuffer);
			DrawBuffer.Clear();
			return this;
		}

		public ConsoleFrame Clear()
		{
			DrawBuffer.Clear();
			return this;
		}

		public ConsoleFrame Draw(Point pos, char glyph) => Draw(pos, glyph, null, null);

		public ConsoleFrame Draw(Point pos, char glyph, ConsoleColor? foreground, ConsoleColor? background)
		{
			var cell = new Cell(pos, glyph, foreground, background);
			DrawBuffer.Remove(cell);
			DrawBuffer.Add(cell);
			return this;
		}
	}

	record Cell(Point Position, char Glyph, ConsoleColor? Foreground = null, ConsoleColor? Background = null);

	class CellPositionComparer : IEqualityComparer<Cell>
	{
		public static readonly CellPositionComparer Default = new CellPositionComparer();

		public bool Equals(Cell? x, Cell? y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;
			return x.Position == y.Position;
		}

		public int GetHashCode([DisallowNull] Cell obj)
		{
			return obj.Position.GetHashCode();
		}
	}

	
}
