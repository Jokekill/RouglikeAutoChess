using AutoChess;
using Xunit;

namespace AutoChess.Tests
{
    public class BoardTests
    {
        [Fact]
        public void LoadFEN_ShouldSetPiecesAndTurn()
        {
            var board = new Board();
            const string fen = "8/8/8/3p4/8/8/8/8 b - - 0 1";
            board.LoadFEN(fen);

            Assert.Equal('p', board.GetPieceAt(3, 3));
            Assert.False(board.IsWhiteTurn);
        }

        [Fact]
        public void GetFEN_ShouldReturnCurrentPosition()
        {
            var board = new Board();
            const string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1";
            board.LoadFEN(fen);

            var result = board.GetFEN();

            Assert.StartsWith("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w", result);
        }

        [Fact]
        public void Clone_ShouldReturnDeepCopy()
        {
            var board = new Board();
            board.LoadFEN("8/8/8/8/8/8/8/R3K2R w - - 0 1");

            var clone = board.Clone();
            board.SetPieceAt(7, 0, '\0');

            Assert.Equal('R', clone.GetPieceAt(7, 0));
            Assert.NotEqual(board.GetPieceAt(7, 0), clone.GetPieceAt(7, 0));
        }
    }
}
