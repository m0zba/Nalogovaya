using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace Nalogovaya
{
    public partial class LoginWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordInput.Password;
            string loggedInRole = "";

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageText.Text = "Введите логин и пароль.";
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        SELECT PasswordHash, FullName, Role
                        FROM dbo.Users
                        WHERE Login = @Login AND IsActive = 1";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@Login", SqlDbType.NVarChar, 100).Value = login;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageText.Text = "Неверный логин или пароль.";
                                return;
                            }

                            string storedHash = reader["PasswordHash"]?.ToString() ?? "";
                            string fullName = reader["FullName"]?.ToString() ?? "";
                            loggedInRole = reader["Role"]?.ToString() ?? "User";

                            if (!PasswordHelper.VerifyPassword(password, storedHash))
                            {
                                MessageText.Text = "Неверный логин или пароль.";
                                return;
                            }

                            reader.Close();

                            MessageBox.Show(
                                $"Добро пожаловать, {fullName}!\\nРоль: {loggedInRole}",
                                "Вход выполнен",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }

                MainWindow mainWindow = new MainWindow(loggedInRole);
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при входе:\\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow
            {
                Owner = this
            };

            registerWindow.ShowDialog();
        }
    }
}
