using ChessChallenge.API;
using System;
using System.Collections.Generic; 
using System.Linq;

public class MyBot : IChessBot
{
    public int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Dictionary<Move, int> moveScores = new Dictionary<Move, int>();

        foreach (Move move in moves)
        {
            // For each move, we see how it improves King Safety
            // and save the score to a dictionary
            moveScores.Add(move, kingSafety(board, move));

            // If the move is a capture, 
            // we also see how much material it gains
            // we save to the dictionary to compare with other types of moves
            if (move.IsCapture)
            {
                if (moveScores.ContainsKey(move))
                {
                    moveScores[move] += capture(board, move);
                }
                else
                {
                    moveScores.Add(move, capture(board, move));
                }
            }

            // We also check if the move helps in development
            // and add to the score of the move if it is already added
            if (moveScores.ContainsKey(move))
            {
                moveScores[move] += pieceDevelopment(board, move);
            }
            else
            {
                moveScores.Add(move, pieceDevelopment(board, move));
            }

            // We also check if the move helps in pawn structure
            if (move.MovePieceType == PieceType.Pawn)
            {
                if (moveScores.ContainsKey(move))
                {
                    moveScores[move] += pawnStructure(board, move);
                }
                else
                {
                    moveScores.Add(move, pawnStructure(board, move));
                }
            }


        }

        // Lastly, we choose the move with the highest score
        var bestMove = moveScores.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        Console.WriteLine(moveScores[bestMove]);
        return bestMove;
    }

    // Heuristcs:
    // 1. King Safety
    // 2. Material
    // 3. Piece Development & Center Control
    // 4. Pawn Structure
    // 5. Space
    // [OPTIONAL] 6. Tempo

    // 1. Ensure that King cannot be attacked or checkmated. 
    // Castle if possible
    // 2. Capture the most valuable piece without losing material
    // 3. Develop pieces to the center. Can be checked by
    // how many possible moves
    // 4. Pawn structure. Can be checked by how many pawns 
    // protected by other pawns
    // 5. Space. Can be checked by how many squares are controlled
    // 6. Tempo is just checking if can.


    // Each function should take in the board position,
    // evaluate based on the heuristic, and return a score

    // The function with the highest score will be chosen

    // Function to evaluate safety of the King
    public int kingSafety(Board board, Move move)
    {
        // King should not be in center
        // King should be castled
        // Should have pawns in front of King
        // Should have pieces protecting King
        // Shoould not move to a square where it can be checked

        if (move.IsCastles)
        {
            return 600;
        }
        return 0;

    }

    // Function to decide whether to capture 
    public int capture(Board board, Move move)
    {
        // Find capture value of piece
        Piece capturedPiece = board.GetPiece(move.TargetSquare);
        int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
        return capturedPieceValue;
    }

    // Function to evaluate piece development
    public int pieceDevelopment(Board board, Move move)
    {
        // We can evaluate this by moving the move then 
        // checking how many possible moves the piece has now

        // For piece development
        // 1. We want to develop our pieces to the center
        // 2. We want to develop our pieces to squares where they can attack
        if (board.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            return -100;
        }
        return 0;
    }

    // Function to evaluate pawn Structure
    public int pawnStructure(Board board, Move move)
    {
        // We can evaluate this by moving the move then 
        // checking how many pawns are protecting other pawns

        // For pawns, we want to achieve 2 things
        // 1. Have pawns controlling the center
        // 2. Have pawns protecting other pawns
        if (move.Equals(new Move("e2e4", board)) && (board.PlyCount == 0))
        {
            return 1000;
        }
        var pawnMoveScore = 0;
        board.MakeMove(move);
        pawnMoveScore = board.GetLegalMoves(true).Length;
        board.UndoMove(move);
        return pawnMoveScore;
    }

    public int tempoAttack(Board board, Move move)
    {
        board.MakeMove(move);
        if (board.IsInCheckmate())
        {
            board.UndoMove(move);
            return 10000;
        } else if (board.IsInCheck())
        {
            board.UndoMove(move);
            return 400;
        }
        return 0;
    }

}