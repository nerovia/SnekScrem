using SnekScrem;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

var controls = new PlayerControls(ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.LeftArrow, ConsoleKey.RightArrow);
var green = new SnekSkin()
{
	BaseColor = ConsoleColor.Green,
	BodyColor = ConsoleColor.Yellow,
	HeadColor = ConsoleColor.Red,
	HeadGlyph = direction => direction switch
	{
		Direction.Up or Direction.Down => '¨',
		_ => ':',
	},
	BodyGlyph = direction => direction switch 
	{
		Direction.Up or Direction.Down => '|',
		_ => '-',
	}
};
var red = new SnekSkin()
{ 
	BaseColor = ConsoleColor.DarkRed,
	BodyColor = ConsoleColor.DarkMagenta,
	HeadColor = ConsoleColor.Yellow,
	HeadGlyph = direction => direction switch
	{
		Direction.Up or Direction.Down => '¨',
		_ => ':'
	},
	BodyGlyph = direction => direction switch
	{
		Direction.Up => '^',
		Direction.Down => 'v',
		Direction.Left => '<',
		_ => '>'
	}
};
var treatGlyph = new ColoredGlyph('&', ConsoleColor.Magenta, ConsoleColor.Cyan);


const double snek_speed = 0.6;

List<Point> treats = new();
SnekEntity[] entities = [
	new SnekEntity()
	{
		Agent = new PlayerAgent(controls),
		Skin = green,
		ResetHandle = self => self.Snek.Reset(new(Console.WindowWidth / 2, Console.WindowHeight / 2), 10),
		LooseHandle = Exit
	},
	new SnekEntity()
	{
		Agent = new NpcAgent(),
		Skin = red,
		ResetHandle = self => self.Snek.Reset(Point.Empty, 2),
		LooseHandle = self => self.InvokeReset()
	},
	new SnekEntity()
	{
		Agent = new NpcAgent(),
		Skin = red,
		ResetHandle = self => self.Snek.Reset(new Point(-2, -2), 2),
		LooseHandle = self => self.InvokeReset()
	},
];

ConsoleCanvas.Begin();

foreach (var entity in entities)
	entity.InvokeReset();

while (true)
{
	if (treats.Count == 0)
	{
		var k = Random.Shared.Next(1, 5);
		for (int i = 0; i < k; i++)
			treats.Add(new(Random.Shared.Next(0, Console.WindowWidth), Random.Shared.Next(0, Console.WindowHeight)));
	}


	var context = new Context(treats, entities.Select(it => it.Snek));
	foreach (var entity in entities)
	{
		var snek = entity.Snek;
		snek.Position = new Point(Mod(snek.Position.X, ConsoleCanvas.Size.Width), Mod(snek.Position.Y, ConsoleCanvas.Size.Height));
		Draw(entity);
	}

	ConsoleCanvas.Clean();
	ConsoleInput.Update();

	foreach (var entity in entities)
	{
		entity.Agent.Act(entity.Snek, context);
		entity.Snek.Move();
		if (Test(entity.Snek))
			entity.InvokeLoose();
	}

	GetInput(TimeSpan.FromSeconds(snek_speed / entities[0].Snek.Length));

}

bool Test(Snek snek)
{
	
	if (treats.RemoveAll(pos => pos == snek.Position) > 0)
	{
		snek.Grow();
		Task.Run(Console.Beep);
	}

	return snek.Length <= 0 || entities.SelectMany(it => it.Snek.Parts).Any(it => it.Position == snek.Position) && snek.Direction != Direction.None;
}

void Draw(SnekEntity player)
{
	var snek = player.Snek;
	var skin = player.Skin;

	foreach (var goodie in treats)
		ConsoleCanvas.Draw(goodie, treatGlyph.Glyph, treatGlyph.Foreground, treatGlyph.Background);

	foreach (var part in snek.Parts)
		ConsoleCanvas.Draw(part.Position, skin.BodyGlyph(part.Direction), skin.BodyColor, skin.BaseColor);

	ConsoleCanvas.Draw(snek.Position, skin.HeadGlyph(snek.Direction), skin.HeadColor, skin.BaseColor);
}

void GetInput(TimeSpan timeout)
{
	var watch = new Stopwatch();
	watch.Start();
	SpinWait.SpinUntil(() => Console.KeyAvailable || watch.Elapsed > timeout);
}

void Exit(SnekEntity player)
{
	for (int i = 1000; i > 800; i -= 50)
		Console.Beep(i, 100);
	Thread.Sleep(1000);
	ConsoleCanvas.Clean();
	ConsoleCanvas.End();
	Console.WriteLine($"YOU ACHIEVED SNEK LENGTH OF {player.Snek.Length}!");
	Environment.Exit(0);
}

int Mod(int x, int m) => x < 0 ? ((x % m) + m) % m : x % m;

record Move(ConsoleKey Key, Direction Direction);

delegate char GlyphSelector(Direction direction);

record ColoredGlyph(char Glyph, ConsoleColor Foreground, ConsoleColor Background);

record SnekSkin
{
	public required ConsoleColor BaseColor { get; set; }
	public required ConsoleColor BodyColor { get; set; }
	public required ConsoleColor HeadColor { get; set; }
	public required GlyphSelector HeadGlyph { get; set; }
	public required GlyphSelector BodyGlyph { get; set; }
}

delegate void PlayerBehaviour(SnekEntity self);

record SnekEntity
{
	public Snek Snek { get; } = new();
	public required IAgent Agent { get; init; }
	public required SnekSkin Skin { get; init; }
	public required PlayerBehaviour ResetHandle { get; init; }
	public required PlayerBehaviour LooseHandle { get; init; }

	public void InvokeReset() => ResetHandle(this);
	public void InvokeLoose() => LooseHandle(this);
}