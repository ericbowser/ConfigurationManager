using System.Windows;
using System.Text.Json;
using ConfigurationManager.Services;
using ConfigurationManager.Models;

namespace ConfigurationManager
{
    public partial class MainWindow : Window
    {
        private DatabaseService? _databaseService;

        public MainWindow()
        {
            InitializeComponent();
            LoadConnectionStringFromEnvironment();
        }

        private void LoadConnectionStringFromEnvironment()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "yourdb";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "youruser";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "yourpass";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

            ConnectionStringTextBox.Text = $"Host={host};Database={database};Username={user};Password={password};Port={port}";
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var connectionString = ConnectionStringTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Please enter a connection string.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ConnectButton.IsEnabled = false;
            ConnectButton.Content = "Connecting...";

            try
            {
                _databaseService = new DatabaseService(connectionString);
                var connected = await _databaseService.TestConnectionAsync();

                if (connected)
                {
                    RefreshButton.IsEnabled = true;
                    AddConfigExpander.IsEnabled = true;
                    ConnectButton.Content = "✓ Connected";
                    ConnectButton.Background = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(40, 167, 69));
                    
                    MessageBox.Show("Connected successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    await LoadConfigurationsAsync();
                }
                else
                {
                    MessageBox.Show("Failed to connect to the database.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Content = "Connect";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ConnectButton.IsEnabled = true;
                ConnectButton.Content = "Connect";
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadConfigurationsAsync();
        }

        private async Task LoadConfigurationsAsync()
        {
            if (_databaseService == null)
                return;

            try
            {
                RefreshButton.IsEnabled = false;
                RefreshButton.Content = "Loading...";

                var configurations = await _databaseService.GetAllConfigurationsAsync();
                
                // Update UI on the UI thread
                Dispatcher.Invoke(() =>
                {
                    ConfigurationsListBox.ItemsSource = null;
                    ConfigurationsListBox.ItemsSource = configurations;
                });

                RefreshButton.Content = "🔄 Refresh";
                RefreshButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configurations: {ex.Message}\n\nStack: {ex.StackTrace}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                RefreshButton.Content = "🔄 Refresh";
                RefreshButton.IsEnabled = true;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseService == null)
                return;

            var projectName = ProjectNameTextBox.Text.Trim();
            var url = UrlTextBox.Text.Trim();
            var configJson = ConfigJsonTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                MessageBox.Show("Project name is required.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Dictionary<string, string>? config = null;

            if (!string.IsNullOrWhiteSpace(configJson))
            {
                try
                {
                    config = JsonSerializer.Deserialize<Dictionary<string, string>>(configJson);
                }
                catch (JsonException)
                {
                    MessageBox.Show("Invalid JSON format in Config field.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                AddButton.IsEnabled = false;
                AddButton.Content = "Adding...";

                var newId = await _databaseService.AddConfigurationAsync(
                    projectName, 
                    string.IsNullOrWhiteSpace(url) ? null : url, 
                    config);

                MessageBox.Show($"Configuration added successfully with ID: {newId}", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                ProjectNameTextBox.Clear();
                UrlTextBox.Clear();
                ConfigJsonTextBox.Text = "{}";

                // Refresh list
                await LoadConfigurationsAsync();

                AddButton.Content = "Add Configuration";
                AddButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AddButton.Content = "Add Configuration";
                AddButton.IsEnabled = true;
            }
        }
    }
}
