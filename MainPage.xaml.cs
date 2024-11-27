using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GokApp
{
    public sealed partial class MainPage : Page
    {
        // Lijsten voor teams en resultaten
        private List<Team> teams;
        private List<Result> results;

        public MainPage()
        {
            this.InitializeComponent();

            // Laad de gegevens uit de JSON-bestanden
            LoadTeams();
            LoadResults();

            // Zet de ComboBox items (teams) als de geladen lijst van teams
            teamComboBox.ItemsSource = teams;
        }

        // Methode om teams uit een JSON-bestand te laden
        private void LoadTeams()
        {
            string teamsJson = File.ReadAllText(@"recources\teams.json");
            teams = JsonConvert.DeserializeObject<List<Team>>(teamsJson);
        }

        // Methode om resultaten uit een JSON-bestand te laden
        private void LoadResults()
        {
            string resultsJson = File.ReadAllText(@"recources\results.json");
            results = JsonConvert.DeserializeObject<List<Result>>(resultsJson);
        }

        // Event handler voor de gok knop
        private void GokButton_Click(object sender, RoutedEventArgs e)
        {
            // Haal het geselecteerde team op
            Team selectedTeam = teamComboBox.SelectedItem as Team;
            if (selectedTeam == null)
            {
                resultLabel.Text = "Selecteer eerst een team!";
                resultLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            // Zoek het resultaat voor het geselecteerde team in de resultatenlijs
            Result selectedResult = results.Find(r => r.TeamName == selectedTeam.Name);
            if (selectedResult != null)
            {
                // Toon het resultaat uit het JSON-bestand
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

    // Teamklasse om de gegevens uit teams.json te representeren
    public class Team
    {
        public string Name { get; set; }
        public double Probability { get; set; } // Kans op winnen
    }

    // Resultaatklasse om de gegevens uit results.json te representeren
    public class Result
    {
        public string TeamName { get; set; }
        public string Outcome { get; set; } // "Winnend" of "Verliezend"
    }
}
