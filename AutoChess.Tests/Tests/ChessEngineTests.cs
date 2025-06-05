using System;
using System.IO;
using System.Threading.Tasks;
using AutoChess;
using Xunit;

namespace AutoChess.Tests
{
    public class ChessEngineTests
    {
        [Fact]
        public async Task GetBestMove_ReturnsValueFromBackend()
        {
            var script = Path.Combine(AppContext.BaseDirectory, "fake_stockfish.sh");
            var engine = new ChessEngine("/bin/bash", script);
            engine.LoadPosition("8/8/8/8/8/8/8/8 w - - 0 1");
            var bestMove = await engine.GetBestMove(1);
            engine.StopStockfish();

            Assert.Equal("a1a2", bestMove);
        }
    }
}
