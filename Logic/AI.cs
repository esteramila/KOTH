namespace Logic
{
    public class AI
    {
        // temporary simple AI implementation
        public static Move GetBestMove(GameState gameState, int depth = 3)
        {
            Move bestMove = null;
            int bestEval = int.MinValue;

            foreach (var move in gameState.AllLegalMovesFor(Player.Black))
            {
                GameState nextState = SimulateMove(gameState, move);
                int eval = Minimax(nextState, depth - 1, int.MinValue, int.MaxValue, false);
                if (eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private static int Minimax(GameState state, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0 || state.IsGameOver())
            {
                return EvaluateBoard(state.Board);
            }

            Player player = maximizingPlayer ? Player.Black : Player.White;

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in state.AllLegalMovesFor(player))
                {
                    GameState nextState = SimulateMove(state, move);
                    int eval = Minimax(nextState, depth - 1, alpha, beta, false);
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
                    int eval = Minimax(nextState, depth - 1, alpha, beta, true);
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
                { PieceType.Knight, 320 },
                { PieceType.Bishop, 330 },
                { PieceType.Rook, 500 },
                { PieceType.Queen, 900 },
                { PieceType.King, 20000 }
            };

            int score = 0;

            foreach (var pos in board.PiecePositions())
            {
                Piece piece = board[pos];
                int value = pieceValues[piece.Type];
                score += (piece.Color == Player.White) ? value : -value;
            }

            return score;
        }
    }
}
