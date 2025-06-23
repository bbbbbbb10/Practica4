using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace Practica4
{
    public partial class MainWindow : Window
    {
        private int _secretNumber;
        private int _attempts;
        private readonly string _resultsFile = "results.json";
        private string _playerName = "Игрок";

        public MainWindow()
        {
            InitializeComponent();
            StartNewGame();
            LoadResults();
            PlayerNameInput.Text = _playerName;
        }

        private void StartNewGame()
        {
            Random rnd = new Random();
            _secretNumber = rnd.Next(1, 101);
            _attempts = 0;
            AttemptsText.Text = "0";
            HintText.Text = "Введите число от 1 до 100";
            NumberInput.Text = "";
            NumberInput.IsEnabled = true;
            CheckButton.IsEnabled = true;
            SaveButton.IsEnabled = false;
            NumberInput.Focus();
        }

        private void CheckNumber()
        {
            if (!int.TryParse(NumberInput.Text, out int guess) || guess < 1 || guess > 100)
            {
                HintText.Text = "Ошибка! Введите число от 1 до 100";
                return;
            }

            _attempts++;
            AttemptsText.Text = _attempts.ToString();

            if (guess < _secretNumber)
            {
                HintText.Text = $"Загаданное число БОЛЬШЕ чем {guess}";
            }
            else if (guess > _secretNumber)
            {
                HintText.Text = $"Загаданное число МЕНЬШЕ чем {guess}";
            }
            else
            {
                HintText.Text = $"ПОБЕДА! Вы угадали число за {_attempts} попыток";
                NumberInput.IsEnabled = false;
                CheckButton.IsEnabled = false;
                SaveButton.IsEnabled = true;
            }

            NumberInput.Text = "";
            NumberInput.Focus();
        }

        private List<GameResult> LoadAllResults()
        {
            if (File.Exists(_resultsFile))
            {
                try
                {
                    var json = File.ReadAllText(_resultsFile);
                    return JsonConvert.DeserializeObject<List<GameResult>>(json)
                        ?? new List<GameResult>();
                }
                catch
                {
                    return new List<GameResult>();
                }
            }
            return new List<GameResult>();
        }

        private void LoadResults()
        {
            var allResults = LoadAllResults();
            var topResults = allResults
                .OrderBy(r => r.Attempts)
                .ThenBy(r => r.Date)
                .Take(10)
                .ToList();

            ResultsGrid.ItemsSource = topResults;
        }

        private void SaveResult()
        {
            string playerName = PlayerNameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(playerName))
            {
                MessageBox.Show("Введите имя игрока перед сохранением", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Загружаем все существующие результаты
            var allResults = LoadAllResults();

            // Проверяем, существует ли уже игрок с таким именем
            if (allResults.Any(r =>
                r.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Игрок с таким именем уже существует!\nПожалуйста, выберите другое имя.",
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var result = new GameResult
            {
                PlayerName = playerName,
                Attempts = _attempts,
                Date = DateTime.Now
            };

            allResults.Add(result);

            try
            {
                File.WriteAllText(_resultsFile, JsonConvert.SerializeObject(allResults));
                LoadResults();
                SaveButton.IsEnabled = false;

                MessageBox.Show($"Результат сохранен для {playerName}!", "Сохранено",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчики событий
        private void CheckButton_Click(object sender, RoutedEventArgs e) => CheckNumber();

        private void NewGameButton_Click(object sender, RoutedEventArgs e) => StartNewGame();

        private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveResult();

        private void NumberInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) CheckNumber();
        }

        private void ChangeNameButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = PlayerNameInput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                _playerName = newName;
                MessageBox.Show($"Имя игрока изменено на: {_playerName}", "Имя изменено",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Введите корректное имя игрока", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}