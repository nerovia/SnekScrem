using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnekScrem
{
	internal class Agent()
	{
		public static readonly Direction[] Moves = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
	
		

		public void Act(Snek subject, IEnumerable<Point> targets)
		{
			if (targets.Count() == 0)
				return;

			var target = targets.MinBy(pos => Extensions.Distance(subject.Position, pos));

			subject.Direction = Moves
				.Select(it => (dir: it, pos: subject.Position + it.Delta()))
				.Where(it => !subject.Nodes.Contains(it.pos))
				.DefaultIfEmpty((dir: subject.Direction, pos: Point.Empty))
				.MinBy(it => Extensions.Distance(it.pos, target)).dir;
		}
	}

	
}
