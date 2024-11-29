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
            _ = LoadMatchesAsync();
        }

        private async Task LoadMatchesAsync()
        {
            try
            {
                // Haal de wedstrijden op via API
                matches = await FetchDataAsync<List<Match>>("https://jouw-api-url/matches");

                // Update UI met wedstrijden
                matchComboBox.ItemsSource = matches;
            }
            catch (Exception ex)
            {
                // Foutmelding weergeven
                resultLabel.Text = $"Fout bij het laden van gegevens: {ex.Message}";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
            }
        }

        private async Task<T> FetchDataAsync<T>(string url)
        {
            var response = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(response);
        }

        private async void MatchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Haal de geselecteerde wedstrijd op
            Match selectedMatch = matchComboBox.SelectedItem as Match;

            if (selectedMatch != null)
            {
                // Werk de betComboBox bij met de teamnamen van de geselecteerde wedstrijd
                betComboBox.Items.Clear();
                betComboBox.Items.Add(selectedMatch.Team1);
                betComboBox.Items.Add(selectedMatch.Team2);

                // Zet de geselecteerde waarde naar null om te voorkomen dat er per ongeluk een gok wordt gedaan zonder keuze
                betComboBox.SelectedItem = null;
            }
        }

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
                resultLabel.Text = "Selecteer een gokoptie (Team 1, Gelijk, Team 2).";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            // Haal de uitslag van de wedstrijd op via een andere API
            var result = await FetchDataAsync<Result>($"https://jouw-api-url/results/{selectedMatch.Id}");

            // Vergelijk de gok met de werkelijke uitslag
            string resultText = "";
            SolidColorBrush resultColor = new SolidColorBrush(Windows.UI.Colors.Gray);

            if (selectedBet == result.Outcome)
            {
                resultText = $"Je hebt gewonnen! Het team heeft {result.Outcome.ToLower()}!";
                resultColor = new SolidColorBrush(Windows.UI.Colors.Green);
                balance += betAmount; // Voeg het gewonnen bedrag toe aan het saldo
            }
            else
            {
                resultText = $"Je hebt verloren! Het team heeft {result.Outcome.ToLower()}!";
                resultColor = new SolidColorBrush(Windows.UI.Colors.Red);
                balance -= betAmount; // Verlies het ingelegde bedrag
            }

            resultLabel.Text = $"{resultText} Je nieuwe saldo is: {balance:C}";
            resultLabel.Foreground = resultColor;
        }
    }

    public class Match
    {
        public string Id { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public DateTime MatchDate { get; set; }
    }

    public class Result
    {
        public string MatchId { get; set; }
        public string Outcome { get; set; } // Team1, Gelijk, Team2
    }
}

