using System.Drawing;

namespace SnekScrem
{
	enum Direction
	{
		Up = -1,
		Down = 1,
		Left = 2,
		Right = -2,
		None = 0,
	}

	static class Extension
	{
		public static Size Delta(this Direction direction) => direction switch
		{
			Direction.Up => new(0, -1),
			Direction.Down => new(0, 1),
			Direction.Left => new(-1, 0),
			Direction.Right => new(1, 0),
			Direction.None => new(0, 0),
			_ => throw new ArgumentException()
		};

		public static Direction Inverse(this Direction direction) => (Direction)(-(int)direction);

		public static bool IsHorizontal(this Direction direction) => Math.Abs((int)direction) == 2;

		public static bool IsVertical(this Direction direction) => Math.Abs((int)direction) == 1;
	}
}
