using GokApp_PRA.Data;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;

namespace GokApp_PRA
{
    public sealed partial class MainWindow : Window
    {
        private readonly ApiManager apiManager;
        private int punten = 100; // Start met 100 punten

        public MainWindow()
        {
            this.InitializeComponent();
            apiManager = new ApiManager();
            UpdatePointsDisplay(); // Zorg dat de punten bij het starten worden weergegeven
        }

        // Update de puntenweergave in de UI
        private void UpdatePointsDisplay()
        {
            puntenText.Text = $"Je huidige punten: {punten}";
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ContentSection.Visibility = Visibility.Visible;
            StartContent.Visibility = Visibility.Collapsed;

            try
            {
                List<ApiManager.Match> matchesList = await apiManager.GetMatchAsync();

                foreach (var match in matchesList)
                {
                    if (!int.TryParse(match.team_1_score, out int team1Score) || !int.TryParse(match.team_2_score, out int team2Score))
                    {
                        string content = $"{match.team_1} vs {match.team_2}";
                        matches.Items.Add(new ComboBoxItem { Content = content });
                    }
                }

                if (matches.Items.Count == 0)
                {
                    resultText.Text = "Geen wedstrijden beschikbaar om te selecteren."; // Gebruik resultText voor de bericht
                }
            }
            catch (Exception ex)
            {
                resultText.Text = $"Error: {ex.Message}"; // Gebruik resultText voor de foutmelding
            }
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedTeam = matches.SelectedItem as ComboBoxItem;
                var selectedBet = bet.SelectedItem as ComboBoxItem;
                var amountToBet = amount.Text;
                error.Text = "";
                resultText.Text = ""; // Reset de winst/verliesboodschap

                if (selectedTeam == null)
                {
                    error.Text += "Kies een wedstrijd!\n";
                    return;
                }

                if (selectedBet == null)
                {
                    error.Text += "Kies een uitslag om te gok\n";
                    return;
                }

                if (!int.TryParse(amountToBet, out int bedrag))
                {
                    error.Text += "Voer een geldig getal in\n";
                    return;
                }

                if (bedrag > punten) // Controleer of de inzet niet hoger is dan de huidige punten
                {
                    error.Text += "Je hebt niet genoeg punten om dit bedrag in te zetten.\n";
                    return;
                }

                List<ApiManager.Match> matchesList = await apiManager.GetMatchAsync();

                foreach (var match in matchesList)
                {
                    string matchString = $"{match.team_1} vs {match.team_2}";

                    if (!int.TryParse(match.team_1_score, out int team1Score) || !int.TryParse(match.team_2_score, out int team2Score))
                    {
                        continue;
                    }

                    // Check if selected match matches the current match
                    if (selectedTeam.Content.ToString() == matchString)
                    {
                        if (team1Score > team2Score && selectedBet.Content.ToString() == "Team 1" ||
                            team1Score < team2Score && selectedBet.Content.ToString() == "Team 2" ||
                            team1Score == team2Score && selectedBet.Content.ToString() == "Gelijk")
                        {
                            resultText.Text = "Gefeliciteerd, je hebt gewonnen!"; // Gebruik resultText voor de bericht
                            punten += 2 * bedrag; // Win 2 keer de inzet
                        }
                        else
                        {
                            resultText.Text = "Helaas, je hebt verloren."; // Gebruik resultText voor de bericht
                            punten -= bedrag; // Verlies de inzet
                        }
                        UpdatePointsDisplay(); // Werk de punten bij na de gok
                        return;
                    }
                }

                resultText.Text = "Wedstrijd niet gevonden of scores niet ingevuld!"; // Gebruik resultText voor de bericht
            }
            catch (Exception ex)
            {
                resultText.Text = $"Error: {ex.Message}"; // Gebruik resultText voor de foutmelding
            }
        }
    }
}
