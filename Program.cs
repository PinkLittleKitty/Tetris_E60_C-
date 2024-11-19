using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class Program
{
    private static int width = 12;
    private static int height = 20;
    private static int[,] gameBoard = null!;
    private static int score = 0;
    private static int level = 0;
    private static bool isGameOver = false;
    private static Tetromino currentBlock = null!;
    private static Random random = new Random();

    static void Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
        }
        Console.WindowHeight = 25;
        Console.WindowWidth = 30;
        Console.CursorVisible = false;
        InitializeGame();
        RunGameLoop();
    }

    static void InitializeGame()
    {
        gameBoard = new int[height, width];
        // Initialize borders
        for (int i = 0; i < height; i++)
        {
            gameBoard[i, 0] = 1;
            gameBoard[i, width - 1] = 1;
        }
        for (int j = 0; j < width; j++)
        {
            gameBoard[height - 1, j] = 1;
        }
        SpawnNewBlock();
    }

    static void SpawnNewBlock()
    {
        currentBlock = new Tetromino(random.Next(7));
        if (!CanMove(currentBlock))
        {
            isGameOver = true;
        }
    }

    static void RunGameLoop()
    {
        DateTime lastFall = DateTime.Now;

        while (!isGameOver)
        {
            if (Console.KeyAvailable)
            {
                HandleInput();
            }

            if ((DateTime.Now - lastFall).TotalMilliseconds > 500)
            {
                UpdateGame();
                lastFall = DateTime.Now;
            }

            DrawGame();
            Thread.Sleep(30);
        }

        Console.Clear();
        Console.WriteLine($"Game Over! Final Score: {score}");
    }

    static void HandleInput()
    {
        var key = Console.ReadKey(true).Key;
        var tempBlock = currentBlock.Clone();

        switch (key)
        {
            case ConsoleKey.D7:  // Numpad 7
            case ConsoleKey.NumPad7:
                tempBlock.MoveLeft();
                if (CanMove(tempBlock)) currentBlock = tempBlock;
                break;
            case ConsoleKey.D9:  // Numpad 9
            case ConsoleKey.NumPad9:
                tempBlock.MoveRight();
                if (CanMove(tempBlock)) currentBlock = tempBlock;
                break;
            case ConsoleKey.D5:  // Numpad 5
            case ConsoleKey.NumPad5:
                tempBlock.MoveDown();
                if (CanMove(tempBlock))
                {
                    currentBlock = tempBlock;
                    score += 1;
                }
                break;
            case ConsoleKey.D8:  // Numpad 8
            case ConsoleKey.NumPad8:
                tempBlock.Rotate();
                if (CanMove(tempBlock)) currentBlock = tempBlock;
                break;
        }
    }

    static bool CanMove(Tetromino block)
    {
        foreach (var pos in block.GetCurrentPositions())
        {
            if (pos.Item1 < 0 || pos.Item1 >= height || pos.Item2 < 0 || pos.Item2 >= width)
                return false;
            if (gameBoard[pos.Item1, pos.Item2] != 0)
                return false;
        }
        return true;
    }

    static void UpdateGame()
    {
        var tempBlock = currentBlock.Clone();
        tempBlock.MoveDown();

        if (CanMove(tempBlock))
        {
            currentBlock = tempBlock;
        }
        else
        {
            // Lock the current block
            foreach (var pos in currentBlock.GetCurrentPositions())
            {
                gameBoard[pos.Item1, pos.Item2] = currentBlock.Color;
            }

            // Check for completed lines
            CheckLines();

            // Spawn new block
            SpawnNewBlock();
        }
    }

    static void CheckLines()
    {
        for (int row = height - 2; row >= 0; row--)
        {
            bool isComplete = true;
            for (int col = 1; col < width - 1; col++)
            {
                if (gameBoard[row, col] == 0)
                {
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
            {
                for (int r = row; r > 0; r--)
                {
                    for (int col = 1; col < width - 1; col++)
                    {
                        gameBoard[r, col] = gameBoard[r - 1, col];
                    }
                }
                score += 100;
                level = (score / 1000) + 1; // Level increases every 1000 points
                row++; // Check the same row again
            }
        }
    }

static void DrawGame()
{
    Console.SetCursorPosition(0, 0);
    var tempBoard = (int[,])gameBoard.Clone();
    string padding = new string(' ', Console.WindowWidth / 2 - width);

    // Draw current block
    foreach (var pos in currentBlock.GetCurrentPositions())
    {
        if (pos.Item1 >= 0 && pos.Item1 < height && pos.Item2 >= 0 && pos.Item2 < width)
        {
            tempBoard[pos.Item1, pos.Item2] = currentBlock.Color;
        }
    }

    // Draw the board with stats and controls
    for (int i = 0; i < height; i++)
    {
        // Left side stats
        string leftText = i switch
        {
            1 => $"Level: {level}".PadRight(12),
            2 => $"Score: {score}".PadRight(12),
            _ => new string(' ', 12)
        };
        Console.Write(leftText);
        Console.Write(padding.Length > 12 ? padding.Substring(12) : padding);
        
        // Draw board
        for (int j = 0; j < width; j++)
        {
            if (tempBoard[i, j] == 0)
                Console.Write("  ");
            else if (tempBoard[i, j] == 1)
            {
                if (j == 0)
                    Console.Write("<!"); 
                else if (j == width - 1)
                    Console.Write("!>"); 
                else if (i == height - 1)
                    Console.Write("=="); 
            }
            else
                Console.Write("[]");
        }
        
        // Right side controls
        switch(i)
        {
            case 1:
                Console.Write("  Controls:");
                break;
            case 2:
                Console.Write("  7 - Move Left");
                break;
            case 3:
                Console.Write("  9 - Move Right");
                break;
            case 4:
                Console.Write("  8 - Rotate");
                break;
            case 5:
                Console.Write("  5 - Drop");
                break;
        }
        Console.WriteLine();
    }
}
}

class Tetromino
{    private static readonly int[][,] Shapes = {
        // I
        new int[,] { {1,1,1,1} },
        // O
        new int[,] { {1,1}, {1,1} },
        // T
        new int[,] { {0,1,0}, {1,1,1} },
        // S
        new int[,] { {0,1,1}, {1,1,0} },
        // Z
        new int[,] { {1,1,0}, {0,1,1} },
        // J
        new int[,] { {1,0,0}, {1,1,1} },
        // L
        new int[,] { {0,0,1}, {1,1,1} }
    };

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Color { get; private set; }
    private int[,] shape;
    private int rotationState;

    public Tetromino(int type)
    {
        shape = Shapes[type];
        Color = type + 2;
        X = 5;
        Y = 0;
        rotationState = 0;
    }

    public void MoveLeft() => X--;
    public void MoveRight() => X++;
    public void MoveDown() => Y++;

    public void Rotate()
    {
        int[,] rotated = new int[shape.GetLength(1), shape.GetLength(0)];
        for (int i = 0; i < shape.GetLength(0); i++)
            for (int j = 0; j < shape.GetLength(1); j++)
                rotated[j, shape.GetLength(0) - 1 - i] = shape[i, j];
        shape = rotated;
    }

    public Tetromino Clone()
    {
        Tetromino clone = (Tetromino)MemberwiseClone();
        clone.shape = (int[,])shape.Clone();
        return clone;
    }

    public List<Tuple<int, int>> GetCurrentPositions()
    {
        var positions = new List<Tuple<int, int>>();
        for (int i = 0; i < shape.GetLength(0); i++)
        {
            for (int j = 0; j < shape.GetLength(1); j++)
            {
                if (shape[i, j] != 0)
                {
                    positions.Add(new Tuple<int, int>(Y + i, X + j));
                }
            }
        }
        return positions;
    }
}
