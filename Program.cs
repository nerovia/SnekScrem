using SnekScrem;
using System.Diagnostics;
using System.Drawing;

var size = new Size(-1, -1);

List<Point> goodies = new();

Queue<Point> snek_bits = new Queue<Point>();
Point snek_head = Point.Empty;
Size snek_delta = Size.Empty;
int snek_length = 2;
const double snek_speed = 0.6;

Move[] snek_moves = [
	new Move(ConsoleKey.UpArrow, new(0, -1)),
	new Move(ConsoleKey.DownArrow, new(0, 1)),
	new Move(ConsoleKey.RightArrow, new(1, 0)),
	new Move(ConsoleKey.LeftArrow, new(-1, 0)),
];

Glyph glyph_head = new(':', '¨', ConsoleColor.Red, ConsoleColor.Green);
Glyph glyph_snek = new('~', '?', ConsoleColor.Yellow, ConsoleColor.Green);
Glyph glyph_goodies = new('&', ' ', ConsoleColor.DarkYellow, ConsoleColor.Magenta);

var canvas = new ConsoleCanvas() { Foreground = ConsoleColor.Black, Background = ConsoleColor.White };
canvas.Begin();

init_snek(10);

while (true)
{
	
	var moves = from m in snek_moves
		where snek_delta != m.delta && snek_delta != Size.Empty - m.delta
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
		}
		else
		{
			move_snek(snek_delta);
		}
	}

	test_snek();
}

// moves the snake
void move_snek(Size delta)
{
	if (snek_bits.Count >= snek_length)
		snek_bits.Dequeue();
	snek_bits.Enqueue(snek_head);
	snek_head += delta;
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

	if (snek_length <= 0 || snek_bits.Any(bit => bit == snek_head) && snek_delta != Size.Empty)
		Environment.Exit(0);

}

void draw(IEnumerable<Move> moves)
{
	// draw snek bits
	foreach (var bit in snek_bits)
		canvas.Draw(bit, snek_delta.Width != 0 ? glyph_snek.symbol : glyph_snek.other, glyph_snek.foreground, glyph_snek.background);

	// draw snek head
	canvas.Draw(snek_head, snek_delta.Width != 0 ? glyph_head.symbol : glyph_head.other, glyph_head.foreground, glyph_head.background);

	foreach (var goodie in goodies)
		canvas.Draw(goodie, glyph_goodies.symbol);

	canvas.Apply();
}

ConsoleKeyInfo? wait_input(TimeSpan timeout)
{
	var watch = new Stopwatch();
	watch.Start();
	SpinWait.SpinUntil(() => Console.KeyAvailable || watch.Elapsed > timeout);
	return Console.KeyAvailable ? Console.ReadKey(true) : null;
}

Rectangle screen_rect() => new Rectangle(0, 0, Console.WindowWidth, Console.WindowHeight);

record struct Glyph(char symbol, char other, ConsoleColor foreground, ConsoleColor background);

record Move(ConsoleKey key, Size delta);
