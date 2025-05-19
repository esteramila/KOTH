using Logic;
using System.Windows;
using System.Windows.Controls;

namespace UI
{
    /// <summary>
    /// Interaction logic for GameOverMenu.xaml
    /// </summary>
    public partial class GameOverMenu : UserControl
    {
        public event Action<Option> OptionSelected;
        public GameOverMenu(GameState gameState)
        {
            InitializeComponent();

            Result result = gameState.Result;
            WinnerText.Text = GetWinnderText(result.Winner);
            ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer);
        }

        private static string GetWinnderText(Player winner)
        {
            switch (winner)
            {
                case Player.White:
                    return "white wins";
                case Player.Black:
                    return "black wins";
                default: // Player.None
                    return "it's a draw";
            }
        }

        private static string PlayerString(Player player)
        {
            switch (player)
            {
                case Player.White:
                    return "white";
                case Player.Black:
                    return "black";
                default:
                    return "";
            }
        }

        private static string GetReasonText(EndReason reason, Player currentPlayer)
        {
            switch (reason)
            {
                case EndReason.Stalemate:
                    return $"stalemate - {PlayerString(currentPlayer)} can't move";
                case EndReason.Checkmate:
                    return $"checkmate - {PlayerString(currentPlayer)} can't move";
                // InsufficientMaterial, ThreefoldRepetition, FiftyMoveRule
                default:
                    return "";
            }
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }
    }
}
