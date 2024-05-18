using System.Drawing;

namespace SnekScrem
{
	enum Direction
	{
		Up = 1,
		Down = 2,
		Left = 4,
		Right = 8,
		None = 0,
	}

	static class Extensions
	{
		public static int Distance(Point a, Point b)
		{

			var x = Math.Abs(a.X - b.X);
			var y = Math.Abs(a.Y - b.Y);
			return x + y;
		}

		public static Size Delta(this Direction direction) => direction switch
		{
			Direction.Up => new(0, -1),
			Direction.Down => new(0, 1),
			Direction.Left => new(-1, 0),
			Direction.Right => new(1, 0),
			_ => new(0, 0)
		};

		public static Direction Opposite(this Direction direction) => direction switch
		{
			Direction.Up => Direction.Down,
			Direction.Down => Direction.Up,
			Direction.Left => Direction.Right,
			Direction.Right => Direction.Left,
			Direction.None => Direction.None,
			_ => throw new ArgumentException()
		};

		public static bool IsHorizontal(this Direction direction) => (direction | Direction.Up | Direction.Down) != 0;

		public static bool IsVertical(this Direction direction) => (direction | Direction.Left | Direction.Right) != 0;
	}
}
