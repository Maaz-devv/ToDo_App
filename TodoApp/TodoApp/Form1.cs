
using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Drawing;


namespace TodoApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadTasks(); // Load tasks when form opens
            taskGrid.CellEndEdit += TaskGrid_CellEndEdit;

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string connStr = ConfigurationManager.ConnectionStrings["TodoDb"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO tasks (description, is_done) VALUES (@description, 0)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@description", txtTaskDescription.Text);
                cmd.ExecuteNonQuery();
            }

            // Clear the textbox
            txtTaskDescription.Text = "";

            // Refresh the task list
            LoadTasks();
        }
        private void TaskGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int id = Convert.ToInt32(taskGrid.Rows[e.RowIndex].Cells["id"].Value);
            string newDescription = taskGrid.Rows[e.RowIndex].Cells["description"].Value.ToString();

            string connStr = ConfigurationManager.ConnectionStrings["TodoDb"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "UPDATE tasks SET description = @desc WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@desc", newDescription);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Task updated successfully.");
        }


        private void LoadTasks()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TodoDb"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT id, description, is_done, created_at FROM tasks";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable table = new DataTable();
                adapter.Fill(table);
                taskGrid.DataSource = table;

                taskGrid.Columns["id"].HeaderText = "ID";
                taskGrid.Columns["description"].HeaderText = "Task";
                taskGrid.Columns["is_done"].HeaderText = "Done?";
                taskGrid.Columns["created_at"].HeaderText = "Created";

                // Make editable
                taskGrid.ReadOnly = false;
                taskGrid.Columns["id"].ReadOnly = true;
                taskGrid.Columns["created_at"].ReadOnly = true;

                // Auto-size columns
                taskGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Color completed tasks
                foreach (DataGridViewRow row in taskGrid.Rows)
                {
                    bool isDone = Convert.ToBoolean(row.Cells["is_done"].Value);
                    if (isDone)
                    {
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                        row.DefaultCellStyle.Font = new Font(taskGrid.Font, FontStyle.Strikeout);
                    }
                }
            }
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnComplete_Click(object sender, EventArgs e)
        {
            if (taskGrid.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(taskGrid.SelectedRows[0].Cells["id"].Value);
                string connStr = ConfigurationManager.ConnectionStrings["TodoDb"].ConnectionString;

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "UPDATE tasks SET is_done = 1 WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                LoadTasks();
            }
            else
            {
                MessageBox.Show("Please select a task to mark as complete.");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (taskGrid.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(taskGrid.SelectedRows[0].Cells["id"].Value);
                string connStr = ConfigurationManager.ConnectionStrings["TodoDb"].ConnectionString;

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "DELETE FROM tasks WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                LoadTasks();
            }
            else
            {
                MessageBox.Show("Please select a task to delete.");
            }
        }

        private void btnUnmark_Click(object sender, EventArgs e)
        {
            if (taskGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a task to unmark.", "Info");
                return;
            }

            // get the ID of the selected task
            int id = Convert.ToInt32(taskGrid.SelectedRows[0].Cells["id"].Value);

            string connStr = ConfigurationManager
                .ConnectionStrings["TodoDb"]
                .ConnectionString;

            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connStr))
            {
                conn.Open();
                string q = "UPDATE tasks SET is_done = 0 WHERE id = @id";
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // reload so the green/strikeout style is removed
            LoadTasks();
        }

    }


}
