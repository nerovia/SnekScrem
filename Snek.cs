using System.Drawing;

namespace SnekScrem
{
	struct OrientedPoint(Point position, Direction direction)
	{
		public readonly static OrientedPoint Empty = new(Point.Empty, Direction.None);
		public Point Position = position;
		public Direction Direction = direction;
	}

	internal class Snek
	{
		readonly Queue<OrientedPoint> nodes = new();
		
		public IEnumerable<OrientedPoint> Parts => nodes;
		public OrientedPoint Head;
		public Point Position { get => Head.Position; set => Head.Position = value; }
		public Direction Direction { get => Head.Direction; set => Head.Direction = value; }
		public int Length { get; private set; }

		public void Reset(Point position, int length)
		{
			nodes.Clear();
			Length = length;
			Head = new(position, Direction.None);
		}

		public void Move()
		{
			if (nodes.Count >= Length)
				nodes.Dequeue();
			nodes.Enqueue(Head);
			Head.Position += Head.Direction.Delta();
		}

		public void Grow()
		{
			Length++;
		}
	}
}
