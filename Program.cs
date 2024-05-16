using SnekScrem;
using System.Diagnostics;
using System.Drawing;

var size = new Size(-1, -1);

List<Point> goodies = new();
Glyph goodie_glyph = new('&', ' ', ConsoleColor.DarkYellow, ConsoleColor.Magenta);

// the position that are occupied by the snake
Queue<Point> snek_bits = new Queue<Point>();

// the position of the snakes head
Point snek_head = Point.Empty;
Size snek_delta = Size.Empty;
Glyph head_glyph = new(':', '¨', ConsoleColor.Red, ConsoleColor.Green);
Glyph bits_glyph = new('~', '?', ConsoleColor.Yellow, ConsoleColor.Green);
int snek_length = 2;
const double snek_speed = 0.6;

ConsoleKey[] nav_keys = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(c => (ConsoleKey)c).ToArray();
int nav_idx = int.MaxValue;
Size[] nav_deltas = [new(0, 1), new(0, -1), new(1, 0), new(-1, 0)];
Move[] nav_moves = [];

init_snek(40);
next_moves();

var canvas = new ConsoleCanvas() { Foreground = ConsoleColor.Black, Background = ConsoleColor.White };
canvas.Begin();

while (true)
{
	
	var moves = from m in nav_moves
		where snek_delta != m.delta && snek_delta != Size.Empty - m.delta
		where screen_rect().Contains(snek_head + m.delta)
		select m;
	
	draw(moves);

	var key = wait_input(TimeSpan.FromSeconds(snek_speed / snek_length));

	if (!key.HasValue)
	{
		move_snek(snek_delta);
	}
	else
	{
		var move = moves.FirstOrDefault(it => it.key == key.Value.Key);
		if (move != null)
		{
			snek_delta = move.delta;
			move_snek(move.delta);
			next_moves();
		}
		else
		{
			move_snek(snek_delta);
			snek_length--;
			if (snek_bits.Count > 0)
				snek_bits.Dequeue();
		}
	}

	test_snek();
}

ConsoleKey next_key()
{
	if (nav_idx >= nav_keys.Length)
	{
		Random.Shared.Shuffle(nav_keys);
		nav_idx = 0;
	}
	return nav_keys[nav_idx++];
}

// moves the snake
void move_snek(Size delta)
{
	if (snek_bits.Count >= snek_length)
		snek_bits.Dequeue();
	snek_bits.Enqueue(snek_head);
	snek_head += delta;
}

void next_moves()
{
	nav_moves = [
		new Move(ConsoleKey.UpArrow, new(0, -1)),
		new Move(ConsoleKey.DownArrow, new(0, 1)),
		new Move(ConsoleKey.RightArrow, new(1, 0)),
		new Move(ConsoleKey.LeftArrow, new(-1, 0)),
	];
	/*
	nav_moves = (from delta in nav_deltas
				select new Move(next_key(), delta)).ToArray();
	*/
}

void init_snek(int length)
{
	snek_bits.Clear();
	snek_head = new(screen_rect().Width / 2, screen_rect().Height / 2);
	snek_delta = Size.Empty;
	snek_length = length;
}

void test_snek()
{
	if (goodies.Count == 0)
	{
		var k = Random.Shared.Next(1, 5);
		for (int i = 0; i < k; i++)
			goodies.Add(new(Random.Shared.Next(0, Console.WindowWidth), Random.Shared.Next(0, Console.WindowHeight)));
	}
	int n = goodies.RemoveAll(pos => pos == snek_head);
	snek_length += n;


	var screen = screen_rect();
	if (snek_head.X < 0)
		snek_head.X = screen.Width - 1;
	else if (snek_head.X >= screen.Width)
		snek_head.X = 0;
	if (snek_head.Y < 0)
		snek_head.Y = screen.Height - 1;
	else if (snek_head.Y >= screen.Height)
		snek_head.Y = 0;

}

void draw(IEnumerable<Move> moves)
{
	
	// draw moves
	foreach (var move in moves)
		canvas.Draw(snek_head + move.delta, (char)move.key);

	// draw snek bits
	foreach (var bit in snek_bits)
		canvas.Draw(bit, snek_delta.Width != 0 ? bits_glyph.symbol : bits_glyph.other, bits_glyph.foreground, bits_glyph.background);

	// draw snek head
	canvas.Draw(snek_head, snek_delta.Width != 0 ? head_glyph.symbol : head_glyph.other, head_glyph.foreground, head_glyph.background);

	foreach (var goodie in goodies)
		canvas.Draw(goodie, goodie_glyph.symbol);

	canvas.Apply();
}

ConsoleKeyInfo? wait_input(TimeSpan timeout)
{
	var watch = new Stopwatch();
	watch.Start();
	SpinWait.SpinUntil(() => Console.KeyAvailable || watch.Elapsed > timeout);
	return Console.KeyAvailable ? Console.ReadKey(true) : null;
}

void draw_at(Point pos, char c)
{
	Console.SetCursorPosition(pos.X, pos.Y);
	Console.Write(c);
}

void set_color(ConsoleColor? foreground, ConsoleColor? background)
{
	Console.ForegroundColor = foreground ?? Console.ForegroundColor;
	Console.BackgroundColor = background ?? Console.BackgroundColor;
}

Rectangle screen_rect() => new Rectangle(0, 0, Console.WindowWidth, Console.WindowHeight);


record struct Glyph(char symbol, char other, ConsoleColor foreground, ConsoleColor background);

record Move(ConsoleKey key, Size delta);
