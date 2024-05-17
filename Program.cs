using SnekScrem;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

var snek = new Snek();

List<Point> goodies = new();

var agent = new Agent();

const double snek_speed = 0.6;

Move[] snek_moves = [
	new Move(ConsoleKey.UpArrow, Direction.Up),
	new Move(ConsoleKey.DownArrow, Direction.Down),
	new Move(ConsoleKey.RightArrow, Direction.Right),
	new Move(ConsoleKey.LeftArrow, Direction.Left),
];

Glyph glyph_head = new(':', '¨', ConsoleColor.Red, ConsoleColor.Green);
Glyph glyph_snek = new('~', '?', ConsoleColor.Yellow, ConsoleColor.Green);
Glyph glyph_goodies = new('&', ' ', ConsoleColor.DarkYellow, ConsoleColor.Magenta);

var canvas = new ConsoleCanvas();
canvas.Begin();

snek.Reset(new(canvas.Size.Width / 2, canvas.Size.Height / 2), 10);

while (true)
{
	Test();

	var moves = from m in snek_moves
		where snek.Direction != m.Direction && snek.Direction != m.Direction.Inverse()
		select m;
	
	Draw(moves);

	var key = GetInput(TimeSpan.FromSeconds(snek_speed / snek.Length));

	if (key.HasValue)
	{
		var move = moves.FirstOrDefault(it => it.Key == key.Value.Key);
		if (move != null)
			snek.Direction = move.Direction;
	}
	else
	{
		agent.Act(snek, goodies);
	}

	snek.Move();
	
	
}

void Test()
{
	if (goodies.Count == 0)
	{
		var k = Random.Shared.Next(1, 5);
		for (int i = 0; i < k; i++)
			goodies.Add(new(Random.Shared.Next(0, Console.WindowWidth), Random.Shared.Next(0, Console.WindowHeight)));
	}
	if (goodies.RemoveAll(pos => pos == snek.Position) > 0)
		snek.Grow();

	snek.Position = new Point(Mod(snek.Position.X, canvas.Size.Width), Mod(snek.Position.Y, canvas.Size.Height));

	if (snek.Length <= 0 || snek.Nodes.Any(bit => bit == snek.Position) && snek.Direction != Direction.None)
		Exit();
}

void Draw(IEnumerable<Move> moves)
{
	foreach (var goodie in goodies)
		canvas.Add(goodie, glyph_goodies.symbol);

	// draw snek bits
	foreach (var bit in snek.Nodes)
		canvas.Add(bit, snek.Direction.IsHorizontal() ? glyph_snek.symbol : glyph_snek.other, glyph_snek.foreground, glyph_snek.background);

	// draw snek head
	canvas.Add(snek.Position, snek.Direction.IsHorizontal() ? glyph_head.symbol : glyph_head.other, glyph_head.foreground, glyph_head.background);

	canvas.Refresh();
}

ConsoleKeyInfo? GetInput(TimeSpan timeout)
{
	var watch = new Stopwatch();
	watch.Start();
	SpinWait.SpinUntil(() => Console.KeyAvailable || watch.Elapsed > timeout);
	return Console.KeyAvailable ? Console.ReadKey(true) : null;
}

void Exit()
{
	canvas.Clear();
	canvas.End();
	Console.WriteLine($"YOU ACHIEVED SNEK LENGTH OF {snek.Length}!");
	Environment.Exit(0);
}

int Mod(int x, int m) => x < 0 ? ((x % m) + m) % m : x % m;

record struct Glyph(char symbol, char other, ConsoleColor foreground, ConsoleColor background);

record Move(ConsoleKey Key, Direction Direction);
