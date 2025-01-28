using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoChess
{
    public partial class MainWindow : Window
    {
        private string selectedPiece;
        private Button selectedButton;
        private int budget = 10;
        private int engineDepth = 2;
        private int oponentDepth = 12;

        private ChessEngine chessEngine;
        private Board board;



        public MainWindow()
        {
            InitializeComponent();
            board = new Board();
            CreateChessBoard();
            chessEngine = new ChessEngine();
            chessEngine.RefreshBoardCallback = RefreshChessBoard;
            chessEngine.HighlightMoveCallback = HighlightLastMove;
            chessEngine.RoundOverCallback = RoundOver;

        }

        private void CreateChessBoard()
        {
            ChessBoard.Children.Clear();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var button = new Button
                    {
                        Tag = new Position(row, col),
                        Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.Gray,
                        BorderBrush = Brushes.Black
                    };

                    button.Click += OnCellClick;

                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);
                    ChessBoard.Children.Add(button);
                }
            }
        }

        private void OnCellClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Position position)
            {
                if (!string.IsNullOrEmpty(selectedPiece))
                {
                    button.Content = new Image
                    {
                        Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{selectedPiece}.png")),
                        Stretch = Stretch.Uniform
                    };
                    board.SetPieceAt(position.Row, position.Col, selectedPiece[1]);
                    UpdateBudget();

                }
                else if (button.Content != null)
                {
                    button.Content = null;
                    board.SetPieceAt(position.Row, position.Col, '\0');
                }
            }

            DeselectPiece();
        }

        private void OnStoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                SelectPiece(button);
            }
        }

        private void SelectPiece(Button button)
        {
            DeselectPiece();
            selectedPiece = ((Image)button.Content).Source.ToString().Split('/').Last().Split('.').First();
            selectedButton = button;
            button.Background = Brushes.LightGreen;
        }

        private void DeselectPiece()
        {
            if (selectedButton != null)
            {
                selectedButton.Background = Brushes.White;
                selectedButton = null;
            }
            selectedPiece = null;
        }

        private void UpdateBudget()
        {
            switch (selectedPiece)
            {
                case "WPawn":
                    budget -= 1;
                    break;
                case "WRook":
                    budget -= 5;
                    break;
                case "WNight":
                    budget -= 3;
                    break;
                case "WBishop":
                    budget -= 3;
                    break;
                case "WQueen":
                    budget -= 9;
                    break;
                case "WKing":
                    budget -= 0;
                    break;
            }
            BudgetTextBlock.Text = $"Budget: ${budget}";
            UpdatePieceButtonStates();
        }

        private void UpdatePieceButtonStates()
        {
            foreach (var button in StorePanel.Children.OfType<Button>())
            {
                string piece = ((Image)button.Content).Source.ToString().Split('/').Last().Split('.').First();
                int pieceCost = GetPieceCost(piece);
                button.IsEnabled = budget >= pieceCost;
                button.Background = button.IsEnabled ? Brushes.White : Brushes.DarkRed;
            }
        }

        private int GetPieceCost(string piece)
        {
            return piece switch
            {
                "WPawn" => 1,
                "WRook" => 5,
                "WNight" => 3,
                "WBishop" => 3,
                "WQueen" => 9,
                "WKing" => 0,
                _ => int.MaxValue
            };
        }

        private void UpdateStats()
        {
            BudgetTextBlock.Text = $"Budget: ${budget}";
   
            EngineDepthTextBlock.Text = $"Engine Depth: {engineDepth} and Oponents engine will have depth: {oponentDepth}";
            TurnTextBlock.Text = board.IsWhiteTurn ? "Turn: White" : "Turn: Black";
            LastMoveTextBlock.Text = $"Last Move: {board.LastMove}";
        }

        private void LoadOpponentPosition(string filePath)
        {
            string position = File.ReadAllText(filePath);
            board.LoadFEN(position);
            PlacePiecesFromBoard();
        }

        private void PlacePiecesFromBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var button = (Button)ChessBoard.Children
                        .Cast<UIElement>()
                        .First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);

                    char piece = board.GetPieceAt(row, col);
                    if (piece != '\0')
                    {
                        button.Content = new Image
                        {
                            Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{board.GetPieceImage(piece)}.png")),
                            Stretch = Stretch.Uniform
                        };
                    }
                    else
                    {
                        button.Content = null;
                    }
                }
            }

            UpdateStats();
        }

        private void RefreshChessBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var button = (Button)ChessBoard.Children
                        .Cast<UIElement>()
                        .First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);

                    char piece = board.GetPieceAt(row, col);
                    if (piece != '\0')
                    {
                        button.Content = new Image
                        {
                            Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{board.GetPieceImage(piece)}.png")),
                            Stretch = Stretch.Uniform
                        };
                    }
                    else
                    {
                        button.Content = null;
                    }
                }
            }
            UpdateStats();
        }

        private void OnClearBoardButtonClick(object sender, RoutedEventArgs e)
        {
            board = new Board();

            RefreshChessBoard();
            ResetBoardColors();
            UpdateStats();
        }


        private void HighlightLastMove(int startRow, int startCol, int endRow, int endCol)
        {
            ResetBoardColors();
            var startButton = (Button)ChessBoard.Children
                .Cast<UIElement>()
                .First(e => Grid.GetRow(e) == startRow && Grid.GetColumn(e) == startCol);
            var endButton = (Button)ChessBoard.Children
                .Cast<UIElement>()
                .First(e => Grid.GetRow(e) == endRow && Grid.GetColumn(e) == endCol);

            startButton.Background = Brushes.Yellow;
            endButton.Background = Brushes.Yellow;
        }

        private void ResetBoardColors()
        {
            foreach (var button in ChessBoard.Children.Cast<Button>())
            {
                button.Background = (Grid.GetRow(button) + Grid.GetColumn(button)) % 2 == 0 ? Brushes.White : Brushes.Gray;
            }
        }


        private void OnFightButtonClick(object sender, RoutedEventArgs e)
        {
            string opponentPositionFilePath = "../../../Oponents/Oponent1.txt";
            LoadOpponentPosition(opponentPositionFilePath);

            board.PlayerDepth = engineDepth;
            board.OpponentDepth = oponentDepth;

            chessEngine.LoadPosition(board.GetFEN());
            string position = board.GetFEN();
            chessEngine.LoadPosition(position);
            chessEngine.PlayAsync(board);

        }

        private void OnLoadOpponentButtonClick(object sender, RoutedEventArgs e)
        {
            string opponentPositionFilePath = "../../../Oponents/Oponent1.txt";
            LoadOpponentPosition(opponentPositionFilePath);
        }


        private void RoundOver(string message)
        {
            MessageBox.Show(message, "Round Over", MessageBoxButton.OK, MessageBoxImage.Information);
            board = new Board();
            RefreshChessBoard();
            ResetBoardColors();
            budget += 15; 
            UpdateStats();
            UpdatePieceButtonStates();
        }

    }

}