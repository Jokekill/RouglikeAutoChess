using AutoChess;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

var board = new Board();
var engine = new ChessEngine();

string GenerateRandomPosition()
{
    var pieces = new[] { 'p','r','n','b','q','k','P','R','N','B','Q','K' };
    var rnd = new Random();
    var sb = new StringBuilder();
    for (int r = 0; r < 8; r++)
    {
        int empty = 0;
        for (int c = 0; c < 8; c++)
        {
            if (rnd.NextDouble() < 0.5)
            {
                empty++;
            }
            else
            {
                if (empty > 0)
                {
                    sb.Append(empty);
                    empty = 0;
                }
                sb.Append(pieces[rnd.Next(pieces.Length)]);
            }
        }
        if (empty > 0) sb.Append(empty);
        if (r < 7) sb.Append('/');
    }
    sb.Append(rnd.Next(2) == 0 ? " w " : " b ");
    sb.Append("- - 0 1");
    return sb.ToString();
}

app.MapGet("/", () => "AutoChess API running");

app.MapGet("/board", () => board.GetFEN());

app.MapGet("/board/random", () =>
{
    var fen = GenerateRandomPosition();
    board.LoadFEN(fen);
    return Results.Ok(board.GetFEN());
});

app.MapPost("/board/load", (string fen) =>
{
    board.LoadFEN(fen);
    return Results.Ok(board.GetFEN());
});

app.MapPost("/board/clear", () =>
{
    board = new Board();
    return Results.Ok(board.GetFEN());
});

app.MapPost("/board/piece", (PieceRequest req) =>
{
    board.SetPieceAt(req.Row, req.Col, req.Piece);
    return Results.Ok(board.GetFEN());
});

app.MapPost("/move", async () =>
{
    engine.LoadPosition(board.GetFEN());
    var depth = board.IsWhiteTurn ? board.PlayerDepth : board.OpponentDepth;
    var bestMove = await engine.GetBestMove(depth);
    if (bestMove != null)
    {
        var sCol = bestMove[0] - 'a';
        var sRow = 8 - (bestMove[1] - '0');
        var eCol = bestMove[2] - 'a';
        var eRow = 8 - (bestMove[3] - '0');

        var piece = board.GetPieceAt(sRow, sCol);
        board.SetPieceAt(eRow, eCol, piece);
        board.SetPieceAt(sRow, sCol, '\0');
        board.SetLastMove(bestMove);
    }

    return Results.Ok(board.GetFEN());
});

app.Run();

public record PieceRequest(int Row, int Col, char Piece);
