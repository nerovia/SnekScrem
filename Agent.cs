using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnekScrem
{
	record Context(IEnumerable<Point> Goodies, IEnumerable<Snek> Sneks);

	interface IAgent
	{
		void Act(Snek subject, Context context);
	}

	class PlayerAgent(PlayerControls controls) : IAgent
	{
		public void Act(Snek subject, Context context)
		{
			(ConsoleKey key, Direction dir)[] moves = [
				(controls.Up, Direction.Up),
				(controls.Down, Direction.Down),
				(controls.Left, Direction.Left),
				(controls.Right, Direction.Right),
			];

			subject.Direction = moves
				.Where(it => it.dir != subject.Direction.Opposite())
				.FirstOrDefault(it => ConsoleInput.HasKeyDown(it.key), (key: ConsoleKey.None, dir: subject.Direction)).dir;

		}
	}

	static class ConsoleInput
	{
		static readonly HashSet<ConsoleKey> KeysDown = new();

		public static bool HasKeyDown(ConsoleKey key) => KeysDown.Contains(key);

		public static void Update()
		{
			KeysDown.Clear();
			while (Console.KeyAvailable)
				KeysDown.Add(Console.ReadKey(true).Key);
		}
	}

	record PlayerControls(
		ConsoleKey Up,
		ConsoleKey Down,
		ConsoleKey Left,
		ConsoleKey Right
	);

	internal class NpcAgent : IAgent
	{
		public static readonly Direction[] Moves = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
	
		

		public void Act(Snek subject, Context context)
		{
			if (context.Goodies.Count() == 0)
				return;

			var target = context.Goodies.MinBy(pos => Extensions.Distance(subject.Position, pos));

			subject.Direction = Moves
				.Select(it => (dir: it, pos: subject.Position + it.Delta()))
				.Where(it => !context.Sneks.SelectMany(s => s.Parts).Any(p => p.Position == it.pos))
				.DefaultIfEmpty((dir: subject.Direction, pos: Point.Empty))
				.MinBy(it => Extensions.Distance(it.pos, target)).dir;
		}
	}

	
}
