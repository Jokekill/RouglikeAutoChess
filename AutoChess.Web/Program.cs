using AutoChess;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var board = new Board();
var engine = new ChessEngine();

app.MapGet("/", () => "AutoChess API running");

app.MapGet("/board", () => board.GetFEN());

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
