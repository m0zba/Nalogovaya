
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Nalogovaya
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString =
            @"Server=localhost\SQLEXPRESS;Database=Nalogovaya;Trusted_Connection=True;TrustServerCertificate=True;";

        private readonly string userRole;

        public MainWindow(string role)
        {
            InitializeComponent();

            userRole = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "User";

            ApplyRolePermissions();
        }

        private void ApplyRolePermissions()
        {
            bool isAdmin = userRole == "Admin";

            AddProjectButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditProjectButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteProjectButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            AddTaskButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            EditTaskButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            DeleteTaskButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            PageDescription.Text = isAdmin
                ? "Вы вошли как администратор. Доступны все действия."
                : "Вы вошли как сотрудник. Доступен просмотр данных и отчётов.";
        }

        private bool RequireAdmin()
        {
            if (userRole == "Admin")
                return true;

            MessageBox.Show(
                "У вас недостаточно прав для выполнения этого действия.",
                "Доступ запрещён",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }

        // =========================
        // НАВИГАЦИЯ
        // =========================

        private void HideAllPanels()
        {
            HomePanel.Visibility = Visibility.Collapsed;
            ContentPanel.Visibility = Visibility.Visible;

            ProjectsPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            ReportsPanel.Visibility = Visibility.Collapsed;

            SectionMessage.Visibility = Visibility.Collapsed;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Главная";
            PageDescription.Text = "Добро пожаловать в систему управления.";

            HomePanel.Visibility = Visibility.Visible;
            ContentPanel.Visibility = Visibility.Collapsed;

            ProjectsPanel.Visibility = Visibility.Collapsed;
            TasksPanel.Visibility = Visibility.Collapsed;
            ReportsPanel.Visibility = Visibility.Collapsed;
        }

        private void ProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();

            PageTitle.Text = "Проекты";
            PageDescription.Text = "Управление проектами.";

            ProjectsPanel.Visibility = Visibility.Visible;
            LoadProjects();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();

            PageTitle.Text = "Задачи";
            PageDescription.Text = "Управление задачами сотрудников.";

            TasksPanel.Visibility = Visibility.Visible;
            LoadTasks();
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();

            PageTitle.Text = "Отчёты";
            PageDescription.Text = "Просмотр и формирование отчётов.";

            ReportsPanel.Visibility = Visibility.Visible;
        }

        // =========================
        // ЗАГРУЗКА ПРОЕКТОВ
        // =========================

        private void LoadProjects()
        {
            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string sql = @"
                        SELECT ProjectId,
                               ProjectName,
                               Description,
                               StartDate,
                               EndDate,
                               ProjectStatus
                        FROM dbo.Projects
                        ORDER BY ProjectId DESC";

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(sql, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        ProjectsDataGrid.ItemsSource = table.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить проекты:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================
        // ДОБАВЛЕНИЕ ПРОЕКТА
        // =========================

        private void AddProjectButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (!RequireAdmin())
                return;
            AddProjectWindow window = new AddProjectWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                LoadProjects();
        }

        // =========================
        // РЕДАКТИРОВАНИЕ ПРОЕКТА
        // =========================

        private void EditProjectButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (!RequireAdmin())
                return;
            if (ProjectsDataGrid.SelectedItem is not DataRowView row)
            {
                MessageBox.Show(
                    "Сначала выберите проект в таблице.",
                    "Проекты",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int id = Convert.ToInt32(row["ProjectId"]);
            string name = row["ProjectName"].ToString();

            string description = row["Description"] == DBNull.Value
                ? ""
                : row["Description"].ToString();

            DateTime? startDate = row["StartDate"] == DBNull.Value
                ? null
                : Convert.ToDateTime(row["StartDate"]);

            DateTime? endDate = row["EndDate"] == DBNull.Value
                ? null
                : Convert.ToDateTime(row["EndDate"]);

            string status = row["ProjectStatus"].ToString();

            EditProjectWindow window = new EditProjectWindow(
                id, name, description, startDate, endDate, status)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                LoadProjects();
        }

        // =========================
        // УДАЛЕНИЕ ПРОЕКТА
        // =========================

        private void DeleteProjectButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (!RequireAdmin())
                return;
            if (ProjectsDataGrid.SelectedItem is not DataRowView row)
            {
                MessageBox.Show(
                    "Сначала выберите проект в таблице.",
                    "Удаление проекта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int projectId = Convert.ToInt32(row["ProjectId"]);
            string projectName = row["ProjectName"].ToString();

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить проект «{projectName}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string sql = @"
                        DELETE FROM dbo.Projects
                        WHERE ProjectId = @ProjectId";

                    using (SqlCommand command =
                           new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@ProjectId", SqlDbType.Int).Value = projectId;

                        connection.Open();
                        int rows = command.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            MessageBox.Show(
                                "Проект не найден. Обновите список.");
                            LoadProjects();
                            return;
                        }
                    }
                }

                MessageBox.Show(
                    "Проект успешно удалён.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadProjects();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Этот проект нельзя удалить, поскольку с ним связаны задачи.\n" +
                    "Сначала удалите или перенесите связанные задачи.",
                    "Удаление невозможно",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при удалении проекта:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshProjectsButton_Click(
            object sender, RoutedEventArgs e)
        {
            LoadProjects();
        }

        // =========================
        // ЗАГРУЗКА ЗАДАЧ
        // =========================

        private void LoadTasks()
        {
            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string sql = @"
                        SELECT
                            t.TaskId,
                            p.ProjectName,
                            t.Title,
                            u.FullName AS AssignedToName,
                            t.DueDate,
                            t.Status,
                            t.Priority
                        FROM dbo.Tasks AS t
                        INNER JOIN dbo.Projects AS p
                            ON t.ProjectId = p.ProjectId
                        LEFT JOIN dbo.Users AS u
                            ON t.AssignedTo = u.UserId
                        ORDER BY t.TaskId DESC";

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(sql, connection))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        TasksDataGrid.ItemsSource = table.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить задачи:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================
        // ДОБАВЛЕНИЕ ЗАДАЧИ
        // =========================

        private void AddTaskButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (!RequireAdmin())
                return;
            AddTaskWindow window = new AddTaskWindow
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                LoadTasks();
        }

        // =========================
        // РЕДАКТИРОВАНИЕ ЗАДАЧИ
        // =========================

        private void EditTaskButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (!RequireAdmin())
                return;
            if (TasksDataGrid.SelectedItem is not DataRowView row)
            {
                MessageBox.Show(
                    "Сначала выберите задачу в таблице.",
                    "Редактирование задачи",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int id = Convert.ToInt32(row["TaskId"]);

            try
            {
                int projectId;
                string title;
                string description;
                int? assignedTo;
                DateTime? dueDate;
                string status;
                string priority;

                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string sql = @"
                        SELECT ProjectId,
                               Title,
                               TaskDescription,
                               AssignedTo,
                               DueDate,
                               Status,
                               Priority
                        FROM dbo.Tasks
                        WHERE TaskId = @TaskId";

                    using (SqlCommand command =
                           new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@TaskId", SqlDbType.Int).Value = id;

                        connection.Open();

                        using (SqlDataReader reader =
                               command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Задача не найдена.");
                                LoadTasks();
                                return;
                            }

                            projectId = Convert.ToInt32(reader["ProjectId"]);
                            title = reader["Title"].ToString();

                            description =
                                reader["TaskDescription"] == DBNull.Value
                                    ? ""
                                    : reader["TaskDescription"].ToString();

                            assignedTo =
                                reader["AssignedTo"] == DBNull.Value
                                    ? null
                                    : Convert.ToInt32(reader["AssignedTo"]);

                            dueDate =
                                reader["DueDate"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(reader["DueDate"]);

                            status = reader["Status"].ToString();
                            priority = reader["Priority"].ToString();
                        }
                    }
                }

                EditTaskWindow window = new EditTaskWindow(
                    id,
                    projectId,
                    title,
                    description,
                    assignedTo,
                    dueDate,
                    status,
                    priority)
                {
                    Owner = this
                };

                if (window.ShowDialog() == true)
                    LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось открыть задачу для редактирования:\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================
        // УДАЛЕНИЕ ЗАДАЧИ
        // =========================

        private void DeleteTaskButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (!RequireAdmin())
                return;
            if (TasksDataGrid.SelectedItem is not DataRowView row)
            {
                MessageBox.Show(
                    "Сначала выберите задачу в таблице.",
                    "Удаление задачи",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int taskId = Convert.ToInt32(row["TaskId"]);
            string title = row["Title"].ToString();

            MessageBoxResult result = MessageBox.Show(
                $"Вы действительно хотите удалить задачу «{title}»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string sql = @"
                        DELETE FROM dbo.Tasks
                        WHERE TaskId = @TaskId";

                    using (SqlCommand command =
                           new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@TaskId", SqlDbType.Int).Value = taskId;

                        connection.Open();
                        int rows = command.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            MessageBox.Show(
                                "Задача не найдена. Обновите список.");
                            LoadTasks();
                            return;
                        }
                    }
                }

                MessageBox.Show(
                    "Задача успешно удалена.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при удалении задачи:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshTasksButton_Click(
            object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        // =========================
        // ФОРМИРОВАНИЕ PDF-ОТЧЁТА
        // =========================

        private void CreateTasksReportButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable tasks = new DataTable();

                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    string sql = @"
                        SELECT
                            t.TaskId,
                            p.ProjectName,
                            t.Title,
                            u.FullName AS AssignedToName,
                            t.DueDate,
                            t.Status,
                            t.Priority
                        FROM dbo.Tasks AS t
                        INNER JOIN dbo.Projects AS p
                            ON t.ProjectId = p.ProjectId
                        LEFT JOIN dbo.Users AS u
                            ON t.AssignedTo = u.UserId
                        ORDER BY t.TaskId DESC";

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(sql, connection))
                    {
                        adapter.Fill(tasks);
                    }
                }

                SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Сохранить PDF-отчёт",
                    Filter = "PDF-файлы (*.pdf)|*.pdf",
                    FileName = "Отчёт_по_задачам.pdf",
                    DefaultExt = ".pdf",
                    AddExtension = true
                };

                if (dialog.ShowDialog() != true)
                    return;

                PdfReportService.CreateTasksReport(
                    tasks, dialog.FileName);

                MessageBox.Show(
                    "PDF-отчёт успешно создан!\n\n" + dialog.FileName,
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сформировать PDF-отчёт:\n\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
