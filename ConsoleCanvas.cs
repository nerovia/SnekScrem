using System;
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

		public ConsoleColor Foreground { get; set; }
		public ConsoleColor Background { get; set; }

		Dictionary<Point, Cell> DrawBuffer = new();
		Dictionary<Point, Cell> EraseBuffer = new();
		static readonly CellPositionComparer comparer = new();

		public void Begin()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			Console.Clear();
			Console.CursorVisible = false;
		}

		public void Apply()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			// don't erase cells at positions that will be drawn
			foreach (var cell in EraseBuffer.Values.Except(DrawBuffer.Values, comparer))
			{
				Console.SetCursorPosition(cell.Position.X, cell.Position.Y);
				Console.Write(' ');
			}

			foreach (var group in DrawBuffer.Values.GroupBy(it => new CellKey(it.Foreground, it.Background)))
			{
				Console.ForegroundColor = group.Key.foreground ?? Foreground;
				Console.BackgroundColor = group.Key.background ?? Background;
				// don't draw cells that haven't changed
				foreach (var cell in group)
				{
					Console.SetCursorPosition(cell.Position.X, cell.Position.Y);
					Console.Write(cell.Glyph);
				}
			}

			(DrawBuffer, EraseBuffer) = (EraseBuffer, DrawBuffer);
			DrawBuffer.Clear();
			

		}

		public void Draw(Point pos, char c, bool transient = true) => Draw(pos, c, null, null, transient);

		public void Draw(Point pos, char c, ConsoleColor? foreground, ConsoleColor? background,  bool transient = true)
		{
			if (transient)
			{
				var cell = new Cell(pos, c, foreground, background);
				if (DrawBuffer.ContainsKey(pos))
					DrawBuffer[pos] = cell;
				else
					DrawBuffer.Add(pos, cell);
			}
		}

	}

	record Cell(Point Position, char Glyph, ConsoleColor? Foreground = null, ConsoleColor? Background = null);

	class CellPositionComparer : IEqualityComparer<Cell>
	{
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

	record struct CellKey(ConsoleColor? foreground, ConsoleColor? background) : IComparable<CellKey>
	{
		static int ColorIndex(ConsoleColor? color)
		{
			return color.HasValue ? (int)color.Value : -1;
		}

		public int CompareTo(CellKey other)
		{
			int result = ColorIndex(foreground) - ColorIndex(other.foreground);
			if (result != 0) return result;
			return ColorIndex(background) - ColorIndex(other.background);
		}
	}
}
