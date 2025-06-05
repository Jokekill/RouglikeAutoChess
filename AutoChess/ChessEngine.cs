using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AutoChess
{

    public class ChessEngine
    {
        private Process stockfishProcess;
        private StreamWriter stockfishInput;
        private StreamReader stockfishOutput;

        private readonly string enginePath;
        private readonly string? engineArguments;

        public Action RefreshBoardCallback { get; set; }
        public Action<string> RoundOverCallback { get; set; }

        public Action<int, int, int, int> HighlightMoveCallback { get; set; }




        public ChessEngine() : this("../../../stockfish/stockfish.exe")
        {
        }

        public ChessEngine(string enginePath, string? arguments = null)
        {
            this.enginePath = enginePath;
            this.engineArguments = arguments;
            StartStockfish();
        }

        public void StartStockfish()
        {
            stockfishProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = enginePath,
                    Arguments = engineArguments ?? string.Empty,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            stockfishProcess.Start();
            stockfishInput = stockfishProcess.StandardInput;
            stockfishOutput = stockfishProcess.StandardOutput;
        }

        public void LoadPosition(string position)
        {
            // Send the position to Stockfish
            stockfishInput.WriteLine($"position fen {position}");
            stockfishInput.Flush();
        }

        public async Task<string> GetBestMove(int depth)
        {
            // Send the command to get the best move
            stockfishInput.WriteLine($"go depth {depth}");
            stockfishInput.Flush();

            // Read the output from Stockfish
            string output;
            while ((output = await stockfishOutput.ReadLineAsync()) != null)
            {
                if (output.StartsWith("bestmove"))
                {
                    return output.Split(' ')[1];
                }
                else if (output.StartsWith("info") && output.Contains("score"))
                {
                    //return output;
                    // Process the score information
                }
            }

            return null;
        }

        public async Task PlayAsync(Board board)
        {
            int moveCount = 0;
            const int maxMoves = 90;
            const int delay = 2000; // 2 seconds
            int depth = 1;

            while (moveCount < maxMoves)
            {
                LoadPosition(board.GetFEN());
                depth = board.IsWhiteTurn ? board.PlayerDepth : board.OpponentDepth;
                var bestMove = await GetBestMove(depth);
                Console.WriteLine($"Best move: {bestMove}");

                // Apply the best move to the board
                // Assuming you have a method to apply the move to the board
                ApplyMove(board, bestMove);



                // Check for mate
                /*
                if (IsMate(board))
                {
                    Console.WriteLine("Mate detected. Game over.");
                    // Notify player of the outcome
                    NotifyPlayerOutcome("You won!");
                    // Increase player's budget
                    IncreasePlayerBudget();
                    break;
                }
                */



                moveCount++;
                await Task.Delay(delay);
            }
        }

        private void ApplyMove(Board board, string move)
        {
            if (move == null)
            {
                return;
            }

            if (move == "(none)")
            {
                StopStockfish();

                if (board.IsWhiteTurn)
                {
                    NotifyPlayerOutcome("Mate on board! Black Won");
                }
                else
                {
                    NotifyPlayerOutcome("Congratulations You have won");
                }

                return;
            }

            // Convert the move from algebraic notation to board coordinates
            int startCol = move[0] - 'a';
            int startRow = 8 - (move[1] - '0');
            int endCol = move[2] - 'a';
            int endRow = 8 - (move[3] - '0');

            // Get the piece at the starting position
            char piece = board.GetPieceAt(startRow, startCol);

            // Set the piece at the ending position
            board.SetPieceAt(endRow, endCol, piece);

            // Clear the starting position
            board.SetPieceAt(startRow, startCol, '\0');

            // Check for promotion
            if ((piece == 'P' && endRow == 0) || (piece == 'p' && endRow == 7))
            {
                // Promote to a queen for simplicity
                char promotedPiece = piece == 'P' ? 'Q' : 'q';
                board.SetPieceAt(endRow, endCol, promotedPiece);
            }

            // Update the last move
            board.SetLastMove(move);

            // Highlight the last move
            HighlightMoveCallback?.Invoke(startRow, startCol, endRow, endCol);

            RefreshBoardCallback?.Invoke();
        }


        



        public void StopStockfish()
        {
            stockfishInput.WriteLine("quit");
            stockfishProcess.WaitForExit();
        }

        private void NotifyPlayerOutcome(string message)
        {
            // Implement the logic to notify the player of the outcome

            RoundOverCallback?.Invoke(message);

        }

    }
}
