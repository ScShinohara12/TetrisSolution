using System;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisApp
{
    public partial class Form1 : Form
    {
        const int BoardWidth = 10;
        const int BoardHeight = 20;
        const int TileSize = 30;

        int[,] field = new int[BoardHeight, BoardWidth];
        int currentX, currentY;

        // NULL許容型として宣言 (CS8618対策)
        int[,]? currentPiece;

        // 完全修飾名で宣言 (CS0104対策)
        System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer();

        Random rand = new Random();

        readonly int[][,] shapes = new int[][,] {
            new int[,] { {1,1,1,1} },
            new int[,] { {1,1}, {1,1} },
            new int[,] { {0,1,0}, {1,1,1} },
            new int[,] { {0,1,1}, {1,1,0} },
            new int[,] { {1,1,0}, {0,1,1} },
            new int[,] { {1,0,0}, {1,1,1} },
            new int[,] { {0,0,1}, {1,1,1} }
        };

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.ClientSize = new Size(BoardWidth * TileSize, BoardHeight * TileSize);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // イベントの登録
            this.KeyDown += Form1_KeyDown;
            this.Paint += Form1_Paint;

            StartGame();
        }

        private void StartGame()
        {
            field = new int[BoardHeight, BoardWidth];
            SpawnPiece();
            gameTimer.Interval = 500;
            // ハンドラの重複登録を防ぐため、一度クリアしてから登録
            gameTimer.Tick -= GameTimer_Tick;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            MovePiece(0, 1);
        }

        private void SpawnPiece()
        {
            currentPiece = shapes[rand.Next(shapes.Length)];
            currentX = BoardWidth / 2 - currentPiece.GetLength(1) / 2;
            currentY = 0;

            if (!CanMove(currentX, currentY, currentPiece))
            {
                gameTimer.Stop();
                MessageBox.Show("Game Over!");
                StartGame();
            }
        }

        private bool CanMove(int newX, int newY, int[,] shape)
        {
            for (int y = 0; y < shape.GetLength(0); y++)
            {
                for (int x = 0; x < shape.GetLength(1); x++)
                {
                    if (shape[y, x] == 0) continue;
                    int tx = newX + x;
                    int ty = newY + y;
                    if (tx < 0 || tx >= BoardWidth || ty >= BoardHeight) return false;
                    if (ty >= 0 && field[ty, tx] != 0) return false;
                }
            }
            return true;
        }

        private void MovePiece(int dx, int dy)
        {
            if (currentPiece == null) return;

            if (CanMove(currentX + dx, currentY + dy, currentPiece))
            {
                currentX += dx;
                currentY += dy;
            }
            else if (dy > 0)
            {
                LockPiece();
                ClearLines();
                SpawnPiece();
            }
            Invalidate();
        }

        private void LockPiece()
        {
            if (currentPiece == null) return;
            for (int y = 0; y < currentPiece.GetLength(0); y++)
                for (int x = 0; x < currentPiece.GetLength(1); x++)
                    if (currentPiece[y, x] != 0)
                        field[currentY + y, currentX + x] = 1;
        }

        private void ClearLines()
        {
            for (int y = BoardHeight - 1; y >= 0; y--)
            {
                bool isFull = true;
                for (int x = 0; x < BoardWidth; x++) if (field[y, x] == 0) isFull = false;

                if (isFull)
                {
                    for (int moveY = y; moveY > 0; moveY--)
                        for (int x = 0; x < BoardWidth; x++) field[moveY, x] = field[moveY - 1, x];
                    for (int x = 0; x < BoardWidth; x++) field[0, x] = 0;
                    y++;
                }
            }
        }

        private void RotatePiece()
        {
            if (currentPiece == null) return;
            int r = currentPiece.GetLength(0);
            int c = currentPiece.GetLength(1);
            int[,] rotated = new int[c, r];
            for (int y = 0; y < r; y++)
                for (int x = 0; x < c; x++)
                    rotated[x, r - 1 - y] = currentPiece[y, x];

            if (CanMove(currentX, currentY, rotated)) currentPiece = rotated;
            Invalidate();
        }

        // 引数に ? を追加して NULL 許容の型一致をさせる (CS8622対策)
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (currentPiece == null) return;
            switch (e.KeyCode)
            {
                case Keys.Left: MovePiece(-1, 0); break;
                case Keys.Right: MovePiece(1, 0); break;
                case Keys.Down: MovePiece(0, 1); break;
                case Keys.Up: RotatePiece(); break;
                case Keys.Space: while (CanMove(currentX, currentY + 1, currentPiece)) MovePiece(0, 1); break;
            }
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            for (int y = 0; y < BoardHeight; y++)
                for (int x = 0; x < BoardWidth; x++)
                    if (field[y, x] != 0)
                        g.FillRectangle(Brushes.DodgerBlue, x * TileSize, y * TileSize, TileSize - 1, TileSize - 1);

            if (currentPiece != null)
            {
                for (int y = 0; y < currentPiece.GetLength(0); y++)
                    for (int x = 0; x < currentPiece.GetLength(1); x++)
                        if (currentPiece[y, x] != 0)
                            g.FillRectangle(Brushes.OrangeRed, (currentX + x) * TileSize, (currentY + y) * TileSize, TileSize - 1, TileSize - 1);
            }

            for (int x = 0; x <= BoardWidth; x++) g.DrawLine(Pens.DimGray, x * TileSize, 0, x * TileSize, BoardHeight * TileSize);
            for (int y = 0; y <= BoardHeight; y++) g.DrawLine(Pens.DimGray, 0, y * TileSize, BoardWidth * TileSize, y * TileSize);
        }
    }
}