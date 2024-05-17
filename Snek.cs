using System.Drawing;

namespace SnekScrem
{
	internal class Snek
	{
		readonly Queue<Point> nodes = new();
		
		public IEnumerable<Point> Nodes => nodes;
		public Point Position { get; private set; }
		public Direction Direction { get; private set; }
		public int Length { get; private set; }

		public void Reset(Point position, int length)
		{
			nodes.Clear();
			Length = length;
			Position = position;
			Direction = Direction.None;
		}

		public void Move()
		{
			if (nodes.Count >= Length)
				nodes.Dequeue();
			nodes.Enqueue(Position);
			Position += Direction.Delta();
		}

		public void Move(Direction direction)
		{
			Direction = direction;
			Move();
		}

		public void Grow()
		{
			Length++;
		}

		public void Wrap(Size size)
		{
			Position = new Point(Position.X % (size.Width - 1), Position.Y % (size.Height - 1));
		}
	}
}
