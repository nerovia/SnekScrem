using SnekScrem;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

Glyph glyph_head = new(':', '¨', ConsoleColor.Red, ConsoleColor.Green);
Glyph glyph_snek = new('~', '?', ConsoleColor.Yellow, ConsoleColor.Green);
Glyph glyph_goodies = new('&', ' ', ConsoleColor.Cyan, ConsoleColor.Magenta);

PlayerControls controls1 = new(ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow);
PlayerControls controls2 = new(ConsoleKey.W, ConsoleKey.S, ConsoleKey.A, ConsoleKey.D);


List<Point> goodies = new();

const double snek_speed = 0.6;

Move[] snek_moves = [
	new Move(ConsoleKey.UpArrow, Direction.Up),
	new Move(ConsoleKey.DownArrow, Direction.Down),
	new Move(ConsoleKey.RightArrow, Direction.Right),
	new Move(ConsoleKey.LeftArrow, Direction.Left),
];



var canvas = new ConsoleCanvas();
Player[] players = [
	new(new Snek(), new PlayerAgent(controls1), glyph_head, glyph_snek, it => Exit(it)),
	//new(new Snek(), new PlayerAgent(controls2), glyph_head, glyph_snek, () => Environment.Exit(0)),
	new(new Snek(), new NpcAgent(), new(':', '¨', ConsoleColor.Yellow, ConsoleColor.DarkRed), new('<', 'v', ConsoleColor.DarkMagenta, ConsoleColor.DarkRed), it => { it.Snek.Reset(Point.Empty, 1); }),
];
canvas.Begin();

players[0].Snek.Reset(new(canvas.Size.Width / 2, canvas.Size.Height / 2), 10);
players[1].Snek.Reset(Point.Empty, 1);

while (true)
{
	if (goodies.Count == 0)
	{
		var k = Random.Shared.Next(1, 5);
		for (int i = 0; i < k; i++)
			goodies.Add(new(Random.Shared.Next(0, Console.WindowWidth), Random.Shared.Next(0, Console.WindowHeight)));
	}

	var context = new Context(goodies, players.Select(it => it.Snek));
	foreach (var player in players)
	{
		Draw(player);
	}

	canvas.Refresh();
	ConsoleInput.Update();

	foreach (var player in players)
	{
		player.Agent.Act(player.Snek, context);
		player.Snek.Move();
		if (Test(player.Snek))
			player.Handle(player);
	}

	GetInput(TimeSpan.FromSeconds(snek_speed / players[0].Snek.Length));

}

bool Test(Snek snek)
{
	snek.Position = new Point(Mod(snek.Position.X, canvas.Size.Width), Mod(snek.Position.Y, canvas.Size.Height));
	
	if (goodies.RemoveAll(pos => pos == snek.Position) > 0)
	{
		snek.Grow();
		Task.Run(Console.Beep);
	}

	return snek.Length <= 0 || players.SelectMany(it => it.Snek.Nodes).Any(bit => bit == snek.Position) && snek.Direction != Direction.None;
}

void Draw(Player player)
{
	var snek = player.Snek;
	foreach (var goodie in goodies)
		canvas.Add(goodie, glyph_goodies.symbol, glyph_goodies.foreground, glyph_goodies.background);

	// draw snek bits
	foreach (var bit in snek.Nodes)
		canvas.Add(bit, snek.Direction.IsHorizontal() ? player.BodyGlyph.symbol : player.BodyGlyph.other, player.BodyGlyph.foreground, player.BodyGlyph.background);

	// draw snek head
	canvas.Add(snek.Position, snek.Direction.IsHorizontal() ? player.HeadGlyph.symbol : player.HeadGlyph.other, player.HeadGlyph.foreground, player.HeadGlyph.background);

}

void GetInput(TimeSpan timeout)
{
	var watch = new Stopwatch();
	watch.Start();
	SpinWait.SpinUntil(() => Console.KeyAvailable || watch.Elapsed > timeout);
	
}

void Exit(Player player)
{
	for (int i = 1000; i > 800; i -= 50)
		Console.Beep(i, 100);
	Thread.Sleep(1000);
	canvas.Clear();
	canvas.End();
	Console.WriteLine($"YOU ACHIEVED SNEK LENGTH OF {player.Snek.Length}!");
	Environment.Exit(0);
}

int Mod(int x, int m) => x < 0 ? ((x % m) + m) % m : x % m;

record struct Glyph(char symbol, char other, ConsoleColor foreground, ConsoleColor background);

record Move(ConsoleKey Key, Direction Direction);

record Player(Snek Snek, IAgent Agent, Glyph HeadGlyph, Glyph BodyGlyph, Action<Player> Handle);