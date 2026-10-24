
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Nalogovaya
{
    public partial class AddProjectWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        public AddProjectWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string projectName = ProjectNameTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                MessageBox.Show(
                    "Введите название проекта.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ProjectNameTextBox.Focus();
                return;
            }

            if (EndDatePicker.SelectedDate.HasValue &&
                StartDatePicker.SelectedDate.HasValue &&
                EndDatePicker.SelectedDate.Value < StartDatePicker.SelectedDate.Value)
            {
                MessageBox.Show(
                    "Дата окончания не может быть раньше даты начала.",
                    "Проверка данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            ComboBoxItem selectedStatus =
                StatusComboBox.SelectedItem as ComboBoxItem;

            string status = selectedStatus?.Content?.ToString() ?? "Активен";

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        INSERT INTO dbo.Projects
                            (ProjectName, Description, StartDate, EndDate, ProjectStatus)
                        VALUES
                            (@ProjectName, @Description, @StartDate, @EndDate, @ProjectStatus)";

                    using (SqlCommand command =
                           new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@ProjectName",
                            SqlDbType.NVarChar,
                            200).Value = projectName;

                        command.Parameters.Add(
                            "@Description",
                            SqlDbType.NVarChar,
                            1000).Value =
                            string.IsNullOrWhiteSpace(description)
                                ? DBNull.Value
                                : description;

                        command.Parameters.Add(
                            "@StartDate",
                            SqlDbType.Date).Value =
                            StartDatePicker.SelectedDate.HasValue
                                ? (object)StartDatePicker.SelectedDate.Value.Date
                                : DBNull.Value;

                        command.Parameters.Add(
                            "@EndDate",
                            SqlDbType.Date).Value =
                            EndDatePicker.SelectedDate.HasValue
                                ? (object)EndDatePicker.SelectedDate.Value.Date
                                : DBNull.Value;

                        command.Parameters.Add(
                            "@ProjectStatus",
                            SqlDbType.NVarChar,
                            30).Value = status;

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Проект успешно сохранён!",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить проект:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
