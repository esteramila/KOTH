﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Logic;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();

        private GameState gameState;
        private Position selectedPos = null;

        private GameMode currentGameMode = GameMode.LocalMultiplayer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();

            currentGameMode = GameMode.LocalMultiplayer;
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle highlight = new Rectangle();
                    highlights[r, c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen())
            {
                return;
            }

            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }


        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);
            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }
        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();
            if (moveCache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                    HandleMove(move);
            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = promMenu;

            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };
        }

        private void HandleMove(Move move)
        {
            gameState.MakeMove(move);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            if(gameState.IsGameOver())
            {
                ShowGameOver();
                return; // I need this, without it, the AI would try to move, but it can't so I get an exception
            }

            // If it's AI's turn in PvAI mode
            if (currentGameMode == GameMode.PlayerVsAI && gameState.CurrentPlayer == Player.Black)
            {
                MakeAIMove();
            }
        }

        private void HandleNetworkMove(Move move)
        {
            // Send move over the network
        }

        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 129, 221, 252); // green: 125, 255, 125
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }

        }
        private void HideHighlights()
        {
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            if (player == Player.White)
            {
                Cursor = Cursors.WhiteCursor;
            }
            else
            {
                Cursor = Cursors.BlackCursor;
            }
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(gameState);
            MenuContainer.Content = gameOverMenu;

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }

        private void RestartGame()
        {
            selectedPos = null;
            HideHighlights();
            moveCache.Clear();
            gameState = new GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMenuOnScreen() && e.Key == Key.P)
            {
                ShowPauseMenu();
            }
        }

        private void ShowPauseMenu()
        {
            PauseMenu pauseMenu = new PauseMenu();
            MenuContainer.Content = pauseMenu;

            pauseMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;
                if (option == Option.Restart)
                {
                    RestartGame();
                }
            };
        }

        private void P1vP2_Click(object sender, RoutedEventArgs e)
        {
            currentGameMode = GameMode.LocalMultiplayer;
            RestartGame();
        }

        private void PvPC_Click(object sender, RoutedEventArgs e)
        {
            currentGameMode = GameMode.PlayerVsAI;
            RestartGame();

            // If AI is black and it's its turn, let it move immediately
            if (gameState.CurrentPlayer == Player.Black)
            {
                MakeAIMove();
            }
        }

        private void PC1vPC2_Click(object sender, RoutedEventArgs e)
        {
            currentGameMode = GameMode.Networked;
            RestartGame();

            // Add logic to initialize network handling
        }

        // temporary simple AI method implementation
        private void MakeAIMove()
        {
            Move aiMove = AI.GetBestMove(gameState);
            HandleMove(aiMove);
        }
    }
}