
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Nalogovaya
{
    public partial class EditProjectWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int projectId;

        public EditProjectWindow(
            int id,
            string name,
            string description,
            DateTime? startDate,
            DateTime? endDate,
            string status)
        {
            InitializeComponent();

            projectId = id;

            ProjectNameTextBox.Text = name;
            DescriptionTextBox.Text = description;

            StartDatePicker.SelectedDate = startDate;
            EndDatePicker.SelectedDate = endDate;

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = ProjectNameTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название проекта.");
                return;
            }

            if (StartDatePicker.SelectedDate.HasValue &&
                EndDatePicker.SelectedDate.HasValue &&
                EndDatePicker.SelectedDate.Value <
                StartDatePicker.SelectedDate.Value)
            {
                MessageBox.Show(
                    "Дата окончания не может быть раньше даты начала.");
                return;
            }

            ComboBoxItem selectedStatus =
                StatusComboBox.SelectedItem as ComboBoxItem;

            string status =
                selectedStatus?.Content?.ToString() ?? "Активен";

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        UPDATE dbo.Projects
                        SET ProjectName = @Name,
                            Description = @Description,
                            StartDate = @StartDate,
                            EndDate = @EndDate,
                            ProjectStatus = @Status
                        WHERE ProjectId = @ProjectId";

                    using (SqlCommand command =
                           new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@Name", SqlDbType.NVarChar, 200).Value = name;

                        command.Parameters.Add(
                            "@Description", SqlDbType.NVarChar, 1000).Value =
                            string.IsNullOrWhiteSpace(description)
                                ? DBNull.Value
                                : description;

                        command.Parameters.Add(
                            "@StartDate", SqlDbType.Date).Value =
                            StartDatePicker.SelectedDate.HasValue
                                ? (object)StartDatePicker.SelectedDate.Value.Date
                                : DBNull.Value;

                        command.Parameters.Add(
                            "@EndDate", SqlDbType.Date).Value =
                            EndDatePicker.SelectedDate.HasValue
                                ? (object)EndDatePicker.SelectedDate.Value.Date
                                : DBNull.Value;

                        command.Parameters.Add(
                            "@Status", SqlDbType.NVarChar, 30).Value = status;

                        command.Parameters.Add(
                            "@ProjectId", SqlDbType.Int).Value = projectId;

                        int rows = command.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            MessageBox.Show(
                                "Проект не найден. Возможно, он был удалён.");
                            return;
                        }
                    }
                }

                MessageBox.Show(
                    "Изменения сохранены!",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при изменении проекта:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
