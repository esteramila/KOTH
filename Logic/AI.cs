namespace Logic
{
    public class AI
    {
        public static Move GetBestMove(GameState gameState, int depth = 3)
        {
            Move bestMove = null;
            int bestEval = int.MinValue;

            foreach (Move move in gameState.AllLegalMovesFor(Player.Black))
            {
                GameState nextState = SimulateMove(gameState, move);
                int eval = MinMax(nextState, depth - 1, int.MinValue, int.MaxValue, false);
                if (eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private static int MinMax(GameState state, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0 || state.IsGameOver())
            {
                if (state.IsGameOver())
                {
                    Result result = state.Result;
                    if (result.Winner == Player.Black)
                        return int.MaxValue; // AI (Black) wins
                    else if (result.Winner == Player.White)
                        return int.MinValue; // AI (Black) loses
                    else
                        return 0; // Draw (stalemate)
                }

                return EvaluateBoard(state.Board);
            }

            Player player = maximizingPlayer ? Player.Black : Player.White;

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in state.AllLegalMovesFor(player))
                {
                    GameState nextState = SimulateMove(state, move);
                    int eval = MinMax(nextState, depth - 1, alpha, beta, false);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break; // Beta cutoff
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in state.AllLegalMovesFor(player))
                {
                    GameState nextState = SimulateMove(state, move);
                    int eval = MinMax(nextState, depth - 1, alpha, beta, true);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break; // Alpha cutoff
                }
                return minEval;
            }
        }

        private static GameState SimulateMove(GameState state, Move move)
        {
            Board boardCopy = state.Board.Copy();
            GameState newState = new GameState(state.CurrentPlayer, boardCopy);
            newState.MakeMove(move);
            return newState;
        }

        private static int EvaluateBoard(Board board)
        {
            Dictionary<PieceType, int> pieceValues = new()
            {
                { PieceType.Pawn, 100 },
                { PieceType.Knight, 300 },
                { PieceType.Bishop, 320 },
                { PieceType.Rook, 600 },
                { PieceType.Queen, 1200 },
                { PieceType.King, 90000 }
            };

            int score = 0;

            foreach (Position pos in board.PiecePositions())
            {
                Piece piece = board[pos];
                int value = pieceValues[piece.Type];

                // Material score
                score += (piece.Color == Player.White) ? value : -value;

                // KOTH bonus
                if (piece.Type == PieceType.King)
                {
                    int distanceToCenter = Math.Abs(pos.Row - 3) + Math.Abs(pos.Column - 3); // Manhattan distance to center
                    int centerProximityBonus = 40000 - (distanceToCenter * 50); // closer = higher bonus

                    score += centerProximityBonus;
                }
            }
            return score;
        }
    }
}
