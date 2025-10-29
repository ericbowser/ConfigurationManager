using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using ConfigurationManager.Services;
using ConfigurationManager.Models;

namespace ConfigurationManager
{
    public partial class MainWindow : Window
    {
        private DatabaseService? _databaseService;
        private EnvConfiguration? _editingConfig;

        public MainWindow()
        {
            InitializeComponent();
            LoadConnectionStringFromEnvironment();
            ConfigJsonTextBox.Text = "{}"; // Set initial valid JSON
        }

        private void LoadConnectionStringFromEnvironment()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "yourdb";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "youruser";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "yourpass";
            var portStr = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            
            // Validate port is a valid integer
            var port = int.TryParse(portStr, out var parsedPort) ? parsedPort : 5432;

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
                catch (JsonException ex)
                {
                    MessageBox.Show($"Invalid JSON format in Config field.\n\nError: {ex.Message}", "Validation Error", 
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

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is EnvConfiguration config)
            {
                _editingConfig = config;
                AddConfigExpander.IsExpanded = true;
                AddConfigExpander.Header = $"✏️ Edit Configuration (ID: {config.Id})";
                
                ProjectNameTextBox.Text = config.Project;
                UrlTextBox.Text = config.Url ?? string.Empty;
                ConfigJsonTextBox.Text = config.Config != null 
                    ? JsonSerializer.Serialize(config.Config, new JsonSerializerOptions { WriteIndented = true })
                    : "{}";
                
                AddButton.Content = "Update Configuration";
                AddButton.Click -= AddButton_Click;
                AddButton.Click += UpdateButton_Click;
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseService == null || _editingConfig == null)
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
                catch (JsonException ex)
                {
                    MessageBox.Show($"Invalid JSON format in Config field.\n\nError: {ex.Message}", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                AddButton.IsEnabled = false;
                AddButton.Content = "Updating...";

                await _databaseService.UpdateConfigurationAsync(
                    _editingConfig.Id,
                    projectName, 
                    string.IsNullOrWhiteSpace(url) ? null : url, 
                    config);

                MessageBox.Show("Configuration updated successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form
                ResetForm();

                // Refresh list
                await LoadConfigurationsAsync();

                AddButton.Content = "Add Configuration";
                AddButton.IsEnabled = true;
                AddButton.Click -= UpdateButton_Click;
                AddButton.Click += AddButton_Click;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating configuration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AddButton.Content = "Update Configuration";
                AddButton.IsEnabled = true;
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is EnvConfiguration config)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete configuration '{config.Project}' (ID: {config.Id})?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes && _databaseService != null)
                {
                    try
                    {
                        await _databaseService.DeleteConfigurationAsync(config.Id);
                        MessageBox.Show("Configuration deleted successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadConfigurationsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting configuration: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ResetForm()
        {
            _editingConfig = null;
            AddConfigExpander.IsExpanded = false;
            AddConfigExpander.Header = "➕ Add New Configuration";
            ProjectNameTextBox.Clear();
            UrlTextBox.Clear();
            ConfigJsonTextBox.Text = "{}";
        }

        private void CopyConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is EnvConfiguration config)
            {
                try
                {
                    Clipboard.SetText(config.ConfigDisplay ?? string.Empty);
                    MessageBox.Show("Config copied to clipboard!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
