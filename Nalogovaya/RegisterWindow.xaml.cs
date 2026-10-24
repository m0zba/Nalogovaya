
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace Nalogovaya
{
    public partial class RegisterWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordInput.Password;
            string confirmPassword = ConfirmPasswordInput.Password;

            // Проверка заполнения всех полей
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageText.Text = "Заполните все поля.";
                return;
            }

            // Проверка длины ФИО
            if (fullName.Length < 5)
            {
                MessageText.Text =
                    "ФИО должно содержать минимум 5 символов.";
                return;
            }

            if (fullName.Length > 200)
            {
                MessageText.Text =
                    "ФИО не должно превышать 200 символов.";
                return;
            }

            // Проверка длины логина
            if (login.Length < 4)
            {
                MessageText.Text =
                    "Логин должен содержать минимум 4 символа.";
                return;
            }

            if (login.Length > 100)
            {
                MessageText.Text =
                    "Логин не должен превышать 100 символов.";
                return;
            }

            // Проверка длины пароля
            if (password.Length < 8)
            {
                MessageText.Text =
                    "Пароль должен содержать минимум 8 символов.";
                return;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                MessageText.Text = "Пароли не совпадают.";
                return;
            }

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем, существует ли такой логин
                    string checkSql = @"
                        SELECT COUNT(*)
                        FROM dbo.Users
                        WHERE Login = @Login";

                    using (SqlCommand checkCommand =
                           new SqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.Add(
                            "@Login", SqlDbType.NVarChar, 100).Value = login;

                        int existingUsers =
                            Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (existingUsers > 0)
                        {
                            MessageText.Text =
                                "Этот логин уже занят. Выберите другой.";
                            return;
                        }
                    }

                    // Хешируем пароль перед сохранением
                    string passwordHash =
                        PasswordHelper.HashPassword(password);

                    // Создаём обычного пользователя, не администратора
                    string insertSql = @"
                        INSERT INTO dbo.Users
                            (Login, PasswordHash, FullName, Role, IsActive)
                        VALUES
                            (@Login, @PasswordHash, @FullName, @Role, @IsActive)";

                    using (SqlCommand insertCommand =
                           new SqlCommand(insertSql, connection))
                    {
                        insertCommand.Parameters.Add(
                            "@Login", SqlDbType.NVarChar, 100).Value = login;

                        insertCommand.Parameters.Add(
                            "@PasswordHash", SqlDbType.NVarChar, 500)
                            .Value = passwordHash;

                        insertCommand.Parameters.Add(
                            "@FullName", SqlDbType.NVarChar, 200)
                            .Value = fullName;

                        insertCommand.Parameters.Add(
                            "@Role", SqlDbType.NVarChar, 50)
                            .Value = "User";

                        insertCommand.Parameters.Add(
                            "@IsActive", SqlDbType.Bit)
                            .Value = true;

                        insertCommand.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Регистрация прошла успешно!\n" +
                    "Теперь вы можете войти под своим логином.",
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
