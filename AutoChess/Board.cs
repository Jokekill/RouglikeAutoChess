using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoChess
{
    public class Board
    {
        private readonly char[,] board;
        private readonly Dictionary<char, string> pieceMap;

        public bool IsWhiteTurn { get; private set; }
        public string LastMove { get; private set; }
        public int PlayerDepth { get; set; }
        public int OpponentDepth { get; set; }

        public Board()
        {
            board = new char[8, 8];
            pieceMap = new Dictionary<char, string>
                {
                    { 'p', "BPawn" },
                    { 'r', "BRook" },
                    { 'n', "BNight" },
                    { 'b', "BBishop" },
                    { 'q', "BQueen" },
                    { 'k', "BKing" },
                    { 'P', "WPawn" },
                    { 'R', "WRook" },
                    { 'N', "WNight" },
                    { 'B', "WBishop" },
                    { 'Q', "WQueen" },
                    { 'K', "WKing" }
                };
            IsWhiteTurn = true;
            LastMove = string.Empty;
        }

        public void LoadFEN(string fen)
        {
            // Clear the current board before loading a new position
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    board[r, c] = '\0';
                }
            }

            string[] parts = fen.Split(' ');
            string[] rows = parts[0].Split('/');

            for (int row = 0; row < 8; row++)
            {
                int col = 0;
                foreach (char c in rows[row])
                {
                    if (char.IsDigit(c))
                    {
                        col += (int)char.GetNumericValue(c);
                    }
                    else
                    {
                        board[row, col] = c;
                        col++;
                    }
                }
            }

            IsWhiteTurn = parts[1] == "w";
        }

        public string GetFEN()
        {
            StringBuilder fen = new StringBuilder();

            for (int row = 0; row < 8; row++)
            {
                int emptyCount = 0;
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] == '\0')
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            fen.Append(emptyCount);
                            emptyCount = 0;
                        }
                        fen.Append(board[row, col]);
                    }
                }
                if (emptyCount > 0)
                {
                    fen.Append(emptyCount);
                }
                if (row < 7)
                {
                    fen.Append('/');
                }
            }

            fen.Append(IsWhiteTurn ? " w " : " b ");
            fen.Append("- - 0 1");

            return fen.ToString();
        }

        public char GetPieceAt(int row, int col)
        {
            return board[row, col];
        }

        public void SetPieceAt(int row, int col, char piece)
        {
            board[row, col] = piece;
        }

        public void SetLastMove(string move)
        {
            LastMove = move;
            IsWhiteTurn = !IsWhiteTurn;
        }

        public string GetPieceImage(char piece)
        {
            return pieceMap.ContainsKey(piece) ? pieceMap[piece] : null;
        }

        public Board Clone()
        {
            var newBoard = new Board();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    newBoard.SetPieceAt(row, col, this.GetPieceAt(row, col));
                }
            }
            newBoard.IsWhiteTurn = this.IsWhiteTurn;
            newBoard.LastMove = this.LastMove;
            return newBoard;
        }
    }
}
