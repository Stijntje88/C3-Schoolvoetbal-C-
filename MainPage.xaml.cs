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
        private List<Team> teams;
        private List<Result> results;

        public MainPage()
        {
            this.InitializeComponent();
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // API-aanroepen
                teams = await FetchDataAsync<List<Team>>("https://jouw-api-url/teams");
                results = await FetchDataAsync<List<Result>>("https://jouw-api-url/results");

                // Update UI
                teamComboBox.ItemsSource = teams;
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

        private void GokButton_Click(object sender, RoutedEventArgs e)
        {
            Team selectedTeam = teamComboBox.SelectedItem as Team;
            if (selectedTeam == null)
            {
                resultLabel.Text = "Selecteer eerst een team!";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            Result selectedResult = results.Find(r => r.TeamName == selectedTeam.Name);
            if (selectedResult != null)
            {
                resultLabel.Text = $"{selectedResult.TeamName} heeft {selectedResult.Outcome.ToLower()}!";
                resultLabel.Foreground = selectedResult.Outcome == "Winnend"
                    ? new SolidColorBrush(Windows.UI.Colors.Green)
                    : new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else
            {
                resultLabel.Text = $"Geen resultaat gevonden voor {selectedTeam.Name}.";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
            }
        }
    }

    public class Team
    {
        public string Name { get; set; }
        public double Probability { get; set; }
    }

    public class Result
    {
        public string TeamName { get; set; }
        public string Outcome { get; set; }
    }
}
