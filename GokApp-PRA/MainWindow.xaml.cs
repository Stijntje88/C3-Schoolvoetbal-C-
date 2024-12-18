using GokApp_PRA.Data;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;
using GokApp_PRA.Model;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GokApp_PRA
{
    public sealed partial class MainWindow : Window
    {
        private readonly ApiManager apiManager;
        private int punten = 100; // Start met 100 punten
        private bool isBetPlaced = false; // Track whether a bet has been placed

        public MainWindow()
        {
            this.InitializeComponent();
            apiManager = new ApiManager();
            CleanupBets();  // Clean up bets when the application starts
            UpdatePointsDisplay(); // Zorg dat de punten bij het starten worden weergegeven
            CheckResultsButton.IsEnabled = false; // Disable the results check button initially
        }

        // Method to delete all bets from the database
        private async void CleanupBets()
        {
            try
            {
                using (var dbContext = new AppDbContext())
                {
                    var allBets = dbContext.Bets.ToList();  // Get all bets
                    dbContext.Bets.RemoveRange(allBets);  // Remove all bets from the database
                    await dbContext.SaveChangesAsync();    // Commit changes to the database
                }
            }
            catch (Exception ex)
            {
                resultText.Text = $"Error during cleanup: {ex.Message}";
            }
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
                await LoadMatchesAsync(); // Fetch the matches when the start button is clicked
            }
            catch (Exception ex)
            {
                resultText.Text = $"Error: {ex.Message}"; // Gebruik resultText voor de foutmelding
            }
        }

        private async Task LoadMatchesAsync()
        {
            // Clear existing items in the matches ComboBox
            matches.Items.Clear();

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
                resultText.Text = $"Error: {ex.Message}";
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

                // Validation checks
                if (selectedTeam == null)
                {
                    error.Text += "Kies een wedstrijd!\n";
                    return;
                }

                if (selectedBet == null)
                {
                    error.Text += "Kies een uitslag om te gokken!\n";
                    return;
                }

                if (!int.TryParse(amountToBet, out int bedrag) || bedrag <= 0)
                {
                    error.Text += "Voer een geldig bedrag in!\n";
                    return;
                }

                if (bedrag > punten)
                {
                    error.Text += "Je hebt niet genoeg punten!\n";
                    return;
                }

                // Subtract the bet amount from the current points
                punten -= bedrag;

                // Create the bet
                var newBet = new Bet
                {
                    ChosenMatch = selectedTeam.Content.ToString(),
                    ChosenTeam = selectedBet.Content.ToString(),
                    Amount = bedrag,
                    IsBetPlaced = true
                };

                // Save the bet to the database
                using (var dbContext = new AppDbContext())
                {
                    dbContext.Bets.Add(newBet);
                    await dbContext.SaveChangesAsync();
                }

                resultText.Text = "Bet is geplaatst!"; // Notify the user that the bet was placed
                UpdatePointsDisplay(); // Update the points display after placing the bet

                // Enable the results check button after a bet is placed
                CheckResultsButton.IsEnabled = true;

                // Disable the "Place Bet" button to avoid placing multiple bets until the results are checked
                myButton.IsEnabled = false;

                isBetPlaced = true; // Mark that a bet has been placed
            }
            catch (Exception ex)
            {
                resultText.Text = $"Error: {ex.Message}"; // Display error if any occurs
            }
        }

        private async Task CheckBetResults()
        {
            try
            {
                // Fetch the updated list of matches (reload every time)
                List<ApiManager.Match> matchesList = await apiManager.GetMatchAsync();

                // Fetch all bets from the database
                List<Bet> allBets;
                using (var dbContext = new AppDbContext())
                {
                    allBets = await dbContext.Bets.ToListAsync(); // Assuming 'Bet' is your bet model
                }

                bool resultChecked = false;

                foreach (var bet in allBets)
                {
                    foreach (var match in matchesList)
                    {
                        // Create the match string
                        string matchString = $"{match.team_1} vs {match.team_2}";

                        // Skip matches without valid scores
                        if (!int.TryParse(match.team_1_score, out int team1Score) ||
                            !int.TryParse(match.team_2_score, out int team2Score))
                        {
                            if (bet.ChosenMatch == matchString)
                            {
                                resultText.Text = "De scores voor deze wedstrijd zijn nog niet ingevuld!";
                                return;
                            }
                            continue;
                        }

                        // Compare the selected match and determine the outcome
                        if (bet.ChosenMatch == matchString)
                        {
                            if (team1Score > team2Score && bet.ChosenTeam == "Team 1" ||
                                team1Score < team2Score && bet.ChosenTeam == "Team 2" ||
                                team1Score == team2Score && bet.ChosenTeam == "Gelijk")
                            {
                                punten += bet.Amount * 2; // Win points (assuming `Amount` is a property of Bet)
                                resultText.Text = $"Gefeliciteerd, je hebt gewonnen! Je wint {bet.Amount * 2} punten.";
                            }
                            else
                            {
                                punten -= bet.Amount; // Lose points
                                resultText.Text = $"Helaas, je hebt verloren. Je verliest {bet.Amount} punten.";
                            }

                            UpdatePointsDisplay();
                            resultChecked = true;
                            break;
                        }
                    }

                    if (resultChecked)
                        break;
                }

                if (!resultChecked)
                {
                    resultText.Text = "Wedstrijd niet gevonden of scores niet ingevuld!";
                }

                // Disable the results check button and enable the "Place Bet" button
                CheckResultsButton.IsEnabled = false;
                myButton.IsEnabled = true;

                // Reset the bet flag
                isBetPlaced = false;
            }
            catch (Exception ex)
            {
                resultText.Text = $"Error: {ex.Message}";
            }
        }

        private async void CheckResultsButton_Click(object sender, RoutedEventArgs e)
        {
            // Reload the matches and check the results when the button is clicked
            await LoadMatchesAsync();  // Reload matches
            await CheckBetResults();   // Check bet results
        }
    }
}
