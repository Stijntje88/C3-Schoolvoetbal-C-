using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GokApp
{
    public sealed partial class MainPage : Page
    {
        private readonly HttpClient client = new HttpClient();
        private List<Match> matches;
        private double balance = 100.0; // Start met 100 nep geld

        public MainPage()
        {
            this.InitializeComponent();
            _ = LoadMatchesAsync(); // Asynchronously load matches when the page is loaded
        }

        // Methode om wedstrijden van de API op te halen
        private async Task LoadMatchesAsync()
        {
            try
            {
                // Haal de wedstrijden op via API
                matches = await FetchDataAsync<List<Match>>("http://localhost:8000/matches/scores");

                // Filter alleen niet-gespeelde wedstrijden
                var unplayedMatches = matches.FindAll(m => m.Team1Score == null && m.Team2Score == null);

                // Update de ComboBox met de lijst van niet-gespeelde wedstrijden
                matchComboBox.ItemsSource = unplayedMatches;
            }
            catch (Exception ex)
            {
                // Foutmelding weergeven bij mislukte API-aanroep
                resultLabel.Text = $"Fout bij het laden van gegevens: {ex.Message}";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            }
        }

        // Methode om gegevens van de API op te halen
        private async Task<T> FetchDataAsync<T>(string url)
        {
            var response = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(response);
        }

        // Event handler voor het wijzigen van de geselecteerde wedstrijd in de ComboBox
        private void MatchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Haal de geselecteerde wedstrijd op
            Match selectedMatch = matchComboBox.SelectedItem as Match;

            if (selectedMatch != null)
            {
                // Werk de betComboBox bij met de teamnamen van de geselecteerde wedstrijd
                betComboBox.Items.Clear();
                betComboBox.Items.Add(selectedMatch.Team1);
                betComboBox.Items.Add(selectedMatch.Team2);
                betComboBox.SelectedItem = null; // Reset selectie
            }
        }

        // Event handler voor de gokknop
        private async void GokButton_Click(object sender, RoutedEventArgs e)
        {
            Match selectedMatch = matchComboBox.SelectedItem as Match;
            if (selectedMatch == null)
            {
                resultLabel.Text = "Selecteer eerst een wedstrijd!";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            double betAmount;
            if (!double.TryParse(betAmountTextBox.Text, out betAmount) || betAmount <= 0 || betAmount > balance)
            {
                resultLabel.Text = "Ongeldig bedrag. Zorg ervoor dat het bedrag binnen je saldo valt.";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            string selectedBet = betComboBox.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedBet))
            {
                resultLabel.Text = "Selecteer een gokoptie.";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            // Controleer of de wedstrijd is gespeeld
            if (selectedMatch.Team1Score.HasValue && selectedMatch.Team2Score.HasValue)
            {
                resultLabel.Text = "Je kunt niet wedden op een al gespeelde wedstrijd!";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            // Simuleer het resultaat
            await Task.Delay(1000); // Tijd voor simulatie

            // Haal opnieuw gegevens op om te zien of de wedstrijd is gespeeld
            var updatedMatches = await FetchDataAsync<List<Match>>("http://localhost:8000/matches/scores");
            var updatedMatch = updatedMatches.Find(m => m.Team1 == selectedMatch.Team1 && m.Team2 == selectedMatch.Team2);

            if (updatedMatch == null || !updatedMatch.Team1Score.HasValue || !updatedMatch.Team2Score.HasValue)
            {
                resultLabel.Text = "De wedstrijd is nog niet gespeeld. Probeer het later opnieuw.";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
                return;
            }

            // Bepaal de winnaar
            string winner;
            if (updatedMatch.Team1Score > updatedMatch.Team2Score)
                winner = updatedMatch.Team1;
            else if (updatedMatch.Team1Score < updatedMatch.Team2Score)
                winner = updatedMatch.Team2;
            else
                winner = "Gelijkspel";

            // Vergelijk de gok met de werkelijke uitslag
            string resultText;
            SolidColorBrush resultColor;

            if (selectedBet == winner)
            {
                resultText = $"Je hebt gewonnen! {winner} heeft gewonnen.";
                resultColor = new SolidColorBrush(Windows.UI.Colors.Green);
                balance += betAmount;
            }
            else
            {
                resultText = $"Je hebt verloren. {winner} heeft gewonnen.";
                resultColor = new SolidColorBrush(Windows.UI.Colors.Red);
                balance -= betAmount;
            }

            resultLabel.Text = $"{resultText} Je nieuwe saldo is: {balance:C}";
            resultLabel.Foreground = resultColor;
        }
    }

    // Modelklasse voor de wedstrijdgegevens
    public class Match
    {
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public int? Team1Score { get; set; } // Kan null zijn als de score nog niet bekend is
        public int? Team2Score { get; set; } // Kan null zijn als de score nog niet bekend is
        public int TournamentId { get; set; }
    }
}
