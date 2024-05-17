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
		Point CursorPosition;
		HashSet<Cell> DrawBuffer = new(CellPositionComparer.Default);
		HashSet<Cell> EraseBuffer = new(CellPositionComparer.Default);

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

		public void Draw()
		{
			foreach (var group in DrawBuffer.GroupBy(it => (it.Foreground, it.Background)))
			{
				Console.ForegroundColor = group.Key.Foreground ?? Foreground;
				Console.BackgroundColor = group.Key.Background ?? Background;
				foreach (var cell in group)
				{
					Console.SetCursorPosition(cell.Position.X, cell.Position.Y);
					Console.Write(cell.Glyph);
				}
			}
		}

		public void Clear()
		{
			Console.ForegroundColor = Foreground;
			Console.BackgroundColor = Background;
			foreach (var cell in EraseBuffer.Except(DrawBuffer, CellPositionComparer.Default))
			{
				Console.SetCursorPosition(cell.Position.X, cell.Position.Y);
				Console.Write(' ');
			}
		}

		public void Refresh()
		{
			Clear();
			Draw();
			(DrawBuffer, EraseBuffer) = (EraseBuffer, DrawBuffer);
			DrawBuffer.Clear();
		}

		public void Add(Point pos, char c, bool transient = true) => Add(pos, c, null, null, transient);

		public void Add(Point pos, char c, ConsoleColor? foreground, ConsoleColor? background,  bool transient = true)
		{
			if (transient)
			{
				var cell = new Cell(pos, c, foreground, background);
				DrawBuffer.Remove(cell);
				DrawBuffer.Add(cell);
			}
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
