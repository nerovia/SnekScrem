using System.Drawing;

namespace SnekScrem
{
	internal class Snek
	{
		readonly Queue<Point> nodes = new();
		
		public IEnumerable<Point> Nodes => nodes;
		public Point Position;
		public Direction Direction;
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

		public void Grow()
		{
			Length++;
		}
	}
}
