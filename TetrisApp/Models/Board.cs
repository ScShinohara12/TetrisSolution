public class Board
{
    public const int Width = 10;
    public const int Height = 20;
    // 0: 空, 1~7: 各テトリミノの色番号
    public int[,] Grid = new int[Height, Width];
}