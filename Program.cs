using System.Diagnostics;
using System.Drawing;

var size = new Size(-1, -1);

// the position that are occupied by the snake
Queue<Point> snek_bits = new Queue<Point>();

// the position of the snakes head
Point snek_head = Point.Empty;
Size snek_delta = Size.Empty;
Glyph head_glyph = new(':', '¨', ConsoleColor.Red, ConsoleColor.Green);
Glyph bits_glyph = new('~', '?', ConsoleColor.Yellow, ConsoleColor.Green);

char[] nav_chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
int nav_idx = int.MaxValue;
Size[] nav_deltas = [new(0, 1), new(0, -1), new(1, 0), new(-1, 0)];
Move[] nav_moves = [];

init_snek(5);
next_moves();

while (true)
{
	int width = Console.BufferWidth;
	int height = Console.BufferHeight;

	var moves = from m in nav_moves
				where snek_delta != m.delta && snek_delta != Size.Empty - m.delta
				let pos = snek_head + m.delta
				where pos.X < width && pos.Y < height && pos.X >= 0 && pos.Y >= 0
				select m;
				 
	foreach (var m in moves)
		Debug.WriteLine(m);
	
	draw(moves);

	var key = wait_input(TimeSpan.FromSeconds(0.8 / (snek_bits.Count + 1)));

	if (!key.HasValue)
	{
		move_snek(snek_delta);
	}
	else
	{
		var move = moves.FirstOrDefault(it => it.input == key.Value.KeyChar);
		if (move != null)
		{
			snek_delta = move.delta;
			move_snek(move.delta);
			next_moves();
		}
		else
		{
			move_snek(snek_delta);
			if (snek_bits.Count > 0)
				snek_bits.Dequeue();
		}
	}
}

char next_nav()
{
	if (nav_idx >= nav_chars.Length)
	{
		Random.Shared.Shuffle(nav_chars);
		nav_idx = 0;
	}
	return nav_chars[nav_idx++];
}

// moves the snake
void move_snek(Size delta, bool grow = false)
{
	if (!grow && snek_bits.Count > 0)
	{
		snek_bits.Dequeue();
		snek_bits.Enqueue(snek_head);
	}
	snek_head = snek_head + delta;
}

void next_moves()
{
	nav_moves = (from delta in nav_deltas
				select new Move(next_nav(), delta)).ToArray();
}

void init_snek(int length)
{
	snek_bits.Clear();
	snek_head = new(Console.BufferWidth / 2, Console.BufferHeight / 2);
	snek_delta = nav_deltas[0];
	for (int i = 1; i < length; i++)
		snek_bits.Enqueue(snek_head);
}

void draw(IEnumerable<Move> moves)
{
	set_color(ConsoleColor.Black, ConsoleColor.White);
	Console.Clear();

	// draw moves
	foreach (var move in moves)
		draw_at(snek_head + move.delta, move.input);

	// draw snek bits
	set_color(bits_glyph.foreground, bits_glyph.background);
	foreach (var bit in snek_bits)
		draw_at(bit, snek_delta.Width != 0 ? bits_glyph.symbol : bits_glyph.other);

	// draw snek head
	set_color(head_glyph.foreground, head_glyph.background);
	draw_at(snek_head, snek_delta.Width != 0 ? head_glyph.symbol : head_glyph.other);

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

record struct Glyph(char symbol, char other, ConsoleColor foreground, ConsoleColor background);

record Move(char input, Size delta);
