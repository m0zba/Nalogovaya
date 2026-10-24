
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Nalogovaya
{
    public partial class AddTaskWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        public AddTaskWindow()
        {
            InitializeComponent();

            LoadProjects();
            LoadEmployees();
        }

        private void LoadProjects()
        {
            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT ProjectId, ProjectName
                        FROM dbo.Projects
                        ORDER BY ProjectName";

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        ProjectComboBox.ItemsSource = table.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить проекты.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT UserId, FullName
                        FROM dbo.Users
                        WHERE IsActive = 1
                        ORDER BY FullName";

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(query, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        EmployeeComboBox.ItemsSource = table.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить сотрудников.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите проект.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название задачи.");
                TitleTextBox.Focus();
                return;
            }

            if (PriorityComboBox.SelectedItem == null ||
                StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите приоритет и статус.");
                return;
            }

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO dbo.Tasks
                        (
                            ProjectId,
                            Title,
                            TaskDescription,
                            AssignedTo,
                            CreatedBy,
                            DueDate,
                            Status,
                            Priority
                        )
                        VALUES
                        (
                            @ProjectId,
                            @Title,
                            @TaskDescription,
                            @AssignedTo,
                            NULL,
                            @DueDate,
                            @Status,
                            @Priority
                        )";

                    using (SqlCommand command =
                           new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(
                            "@ProjectId", SqlDbType.Int).Value =
                            Convert.ToInt32(ProjectComboBox.SelectedValue);

                        command.Parameters.Add(
                            "@Title", SqlDbType.NVarChar, 200).Value =
                            TitleTextBox.Text.Trim();

                        command.Parameters.Add(
                            "@TaskDescription", SqlDbType.NVarChar).Value =
                            string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                                ? DBNull.Value
                                : DescriptionTextBox.Text.Trim();

                        command.Parameters.Add(
                            "@AssignedTo", SqlDbType.Int).Value =
                            EmployeeComboBox.SelectedValue == null
                                ? DBNull.Value
                                : EmployeeComboBox.SelectedValue;

                        command.Parameters.Add(
                            "@DueDate", SqlDbType.Date).Value =
                            DueDatePicker.SelectedDate.HasValue
                                ? DueDatePicker.SelectedDate.Value.Date
                                : DBNull.Value;

                        command.Parameters.Add(
                            "@Status", SqlDbType.NVarChar, 50).Value =
                            ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();

                        command.Parameters.Add(
                            "@Priority", SqlDbType.NVarChar, 50).Value =
                            ((ComboBoxItem)PriorityComboBox.SelectedItem).Content.ToString();

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Задача успешно сохранена!",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить задачу.\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
