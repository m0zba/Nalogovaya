
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Nalogovaya
{
    public partial class EditTaskWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly int taskId;

        public EditTaskWindow(
            int id,
            int projectId,
            string title,
            string description,
            int? assignedTo,
            DateTime? dueDate,
            string status,
            string priority)
        {
            InitializeComponent();

            taskId = id;

            LoadProjects();
            LoadEmployees();

            ProjectComboBox.SelectedValue = projectId;
            TitleTextBox.Text = title;
            DescriptionTextBox.Text = description;
            EmployeeComboBox.SelectedValue = assignedTo;
            DueDatePicker.SelectedDate = dueDate;

            SelectComboItem(StatusComboBox, status);
            SelectComboItem(PriorityComboBox, priority);
        }

        private void LoadProjects()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT ProjectId, ProjectName FROM dbo.Projects ORDER BY ProjectName";

                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connection))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    ProjectComboBox.ItemsSource = table.DefaultView;
                }
            }
        }

        private void LoadEmployees()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT UserId, FullName
                    FROM dbo.Users
                    WHERE IsActive = 1
                    ORDER BY FullName";

                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connection))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    EmployeeComboBox.ItemsSource = table.DefaultView;
                }
            }
        }

        private void SelectComboItem(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
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
                return;
            }

            if (StatusComboBox.SelectedItem == null ||
                PriorityComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус и приоритет.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = @"
                        UPDATE dbo.Tasks
                        SET ProjectId = @ProjectId,
                            Title = @Title,
                            TaskDescription = @Description,
                            AssignedTo = @AssignedTo,
                            DueDate = @DueDate,
                            Status = @Status,
                            Priority = @Priority
                        WHERE TaskId = @TaskId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@ProjectId", SqlDbType.Int)
                            .Value = Convert.ToInt32(ProjectComboBox.SelectedValue);

                        command.Parameters.Add("@Title", SqlDbType.NVarChar, 200)
                            .Value = TitleTextBox.Text.Trim();

                        command.Parameters.Add("@Description", SqlDbType.NVarChar, -1)
                            .Value = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                                ? DBNull.Value
                                : DescriptionTextBox.Text.Trim();

                        command.Parameters.Add("@AssignedTo", SqlDbType.Int)
                            .Value = EmployeeComboBox.SelectedValue ?? DBNull.Value;

                        command.Parameters.Add("@DueDate", SqlDbType.Date)
                            .Value = DueDatePicker.SelectedDate.HasValue
                                ? DueDatePicker.SelectedDate.Value.Date
                                : DBNull.Value;

                        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50)
                            .Value = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();

                        command.Parameters.Add("@Priority", SqlDbType.NVarChar, 50)
                            .Value = ((ComboBoxItem)PriorityComboBox.SelectedItem).Content.ToString();

                        command.Parameters.Add("@TaskId", SqlDbType.Int).Value = taskId;

                        connection.Open();
                        int rows = command.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            MessageBox.Show("Задача не найдена. Обновите список.");
                            return;
                        }
                    }
                }

                MessageBox.Show("Задача успешно изменена!",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при изменении задачи:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
