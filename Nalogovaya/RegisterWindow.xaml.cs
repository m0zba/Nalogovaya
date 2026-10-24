
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Media;

namespace Nalogovaya
{
    public partial class RegisterWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        private string selectedRole = "User";

        public RegisterWindow()
        {
            InitializeComponent();
            UpdateRoleButtons();
        }

        private void AdminRoleButton_Click(object sender, RoutedEventArgs e)
        {
            selectedRole = "Admin";
            UpdateRoleButtons();
            MessageText.Text = "";
        }

        private void EditorRoleButton_Click(object sender, RoutedEventArgs e)
        {
            selectedRole = "Editor";
            UpdateRoleButtons();
            MessageText.Text = "";
        }

        private void UserRoleButton_Click(object sender, RoutedEventArgs e)
        {
            selectedRole = "User";
            UpdateRoleButtons();
            MessageText.Text = "";
        }

        private void UpdateRoleButtons()
        {
            AdminRoleButton.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(
                    selectedRole == "Admin" ? "#4F3021" : "#79553D"));

            EditorRoleButton.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(
                    selectedRole == "Editor" ? "#4F3021" : "#79553D"));

            UserRoleButton.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(
                    selectedRole == "User" ? "#4F3021" : "#79553D"));

            SelectedRoleText.Text = "Выбрана роль: " + selectedRole;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordInput.Password;
            string confirmPassword = ConfirmPasswordInput.Password;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageText.Text = "Заполните все поля.";
                return;
            }

            if (fullName.Length < 5 || fullName.Length > 150)
            {
                MessageText.Text =
                    "ФИО должно содержать от 5 до 150 символов.";
                return;
            }

            if (login.Length < 4 || login.Length > 50)
            {
                MessageText.Text =
                    "Логин должен содержать от 4 до 50 символов.";
                return;
            }

            if (password.Length < 8)
            {
                MessageText.Text =
                    "Пароль должен содержать минимум 8 символов.";
                return;
            }

            if (password != confirmPassword)
            {
                MessageText.Text = "Пароли не совпадают.";
                return;
            }

            string passwordHash = PasswordHelper.HashPassword(password);

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction =
                           connection.BeginTransaction(IsolationLevel.Serializable))
                    {
                        try
                        {
                            // Проверяем, занят ли логин.
                            string checkLoginSql = @"
                                SELECT COUNT(*)
                                FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
                                WHERE Login = @Login";

                            using (SqlCommand command =
                                   new SqlCommand(checkLoginSql, connection, transaction))
                            {
                                command.Parameters.Add(
                                    "@Login", SqlDbType.NVarChar, 50).Value = login;

                                int existingUsers = Convert.ToInt32(
                                    command.ExecuteScalar());

                                if (existingUsers > 0)
                                {
                                    transaction.Rollback();
                                    MessageText.Text =
                                        "Этот логин уже занят. Выберите другой.";
                                    return;
                                }
                            }

                            // Ограничиваем количество активных аккаунтов.
                            if (selectedRole == "Admin" ||
                                selectedRole == "Editor")
                            {
                                string countRoleSql = @"
                                    SELECT COUNT(*)
                                    FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
                                    WHERE Role = @Role AND IsActive = 1";

                                using (SqlCommand command =
                                       new SqlCommand(countRoleSql, connection, transaction))
                                {
                                    command.Parameters.Add(
                                        "@Role", SqlDbType.NVarChar, 30)
                                        .Value = selectedRole;

                                    int roleCount = Convert.ToInt32(
                                        command.ExecuteScalar());

                                    int roleLimit =
                                        selectedRole == "Admin" ? 1 : 3;

                                    if (roleCount >= roleLimit)
                                    {
                                        transaction.Rollback();

                                        MessageText.Text =
                                            selectedRole == "Admin"
                                            ? "Администратор уже существует. Разрешён только 1 активный Admin."
                                            : "Достигнут лимит: разрешено максимум 3 активных Editor.";

                                        return;
                                    }
                                }
                            }

                            // Создаём пользователя с выбранной ролью.
                            string insertSql = @"
                                INSERT INTO dbo.Users
                                    (Login, PasswordHash, FullName, Role, IsActive)
                                VALUES
                                    (@Login, @PasswordHash, @FullName, @Role, @IsActive)";

                            using (SqlCommand command =
                                   new SqlCommand(insertSql, connection, transaction))
                            {
                                command.Parameters.Add(
                                    "@Login", SqlDbType.NVarChar, 50).Value = login;

                                command.Parameters.Add(
                                    "@PasswordHash", SqlDbType.NVarChar, 500)
                                    .Value = passwordHash;

                                command.Parameters.Add(
                                    "@FullName", SqlDbType.NVarChar, 150)
                                    .Value = fullName;

                                command.Parameters.Add(
                                    "@Role", SqlDbType.NVarChar, 30)
                                    .Value = selectedRole;

                                command.Parameters.Add(
                                    "@IsActive", SqlDbType.Bit).Value = true;

                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch
                            {
                                // Транзакция могла уже завершиться.
                            }

                            throw;
                        }
                    }
                }

                MessageBox.Show(
                    "Регистрация прошла успешно!\n" +
                    "Логин: " + login + "\n" +
                    "Роль: " + selectedRole,
                    "Регистрация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (SqlException ex) when (
                ex.Number == 2601 || ex.Number == 2627)
            {
                MessageText.Text =
                    "Этот логин уже занят. Выберите другой.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось зарегистрировать пользователя:\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
