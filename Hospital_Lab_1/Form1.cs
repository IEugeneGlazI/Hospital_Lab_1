using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hospital_Lab_1
{
    public partial class Form1 : Form
    {
        private string connectionString;
        private SqlDataAdapter doctorAdapter;
        private SqlDataAdapter patientAdapter;
        private SqlDataAdapter specAdapter;
        //private SqlDataAdapter visitAdapter;
        private SqlCommandBuilder doctorBuilder = new SqlCommandBuilder();
        private SqlCommandBuilder patientBuilder = new SqlCommandBuilder();
        private SqlCommandBuilder specBuilder = new SqlCommandBuilder();
        private DataSet dataSet = new DataSet();

        public Form1()
        {
            InitializeComponent();
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;

            // Создание объектов NpgsqlDataAdapter.
            doctorAdapter = new SqlDataAdapter("Select * from Doctor;", connectionString);
            specAdapter = new SqlDataAdapter("Select * from Specialization", connectionString);

            // Автоматическая генерация команд SQL.
            doctorBuilder = new SqlCommandBuilder(doctorAdapter);
            specBuilder = new SqlCommandBuilder(specAdapter);

            // Заполнение таблиц в DataSet.
            doctorAdapter.Fill(dataSet, "Doctor");
            specAdapter.Fill(dataSet, "Specialization");
            //visitAdapter.Fill(dataSet, "Visit");

            dataGridView1.DataSource = dataSet.Tables["Doctor"];
            dataGridView2.DataSource = dataSet.Tables["Specialization"];

            FillSpecCombobox();
        }

        private void FillSpecCombobox()
        {
            ((DataGridViewComboBoxColumn)dataGridView1.Columns["id"]).DataSource =
                dataSet.Tables["Specialization"];
            ((DataGridViewComboBoxColumn)dataGridView1.Columns["id"]).DisplayMember =
                "name_of_specialization";
            ((DataGridViewComboBoxColumn)dataGridView1.Columns["id"]).ValueMember =
                "id";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            doctorAdapter.Update(dataSet, "Doctor");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            specAdapter.Update(dataSet, "Specialization");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string startDate = textBoxStartDate.Text;
            string endDate = textBoxEndDate.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"USE Hospital " +
                       $"SELECT Diagnosis.name_of_diagnosis AS 'Диагноз', COUNT(*) AS 'Количество диагнозов' " +
                       $"FROM Diagnosis " +
                       $"INNER JOIN Visit ON Diagnosis.diagnosis_id = Visit.diagnosis_id " +
                       $"WHERE Visit.date > '{startDate}' AND Visit.date < '{endDate}' " +
                       $"GROUP BY Diagnosis.name_of_diagnosis " +
                       $"ORDER BY 'Количество диагнозов' DESC";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                dataGridView3.Columns.Clear();

                dataGridView3.Rows.Clear();

                dataGridView3.Columns.Add("Diagnosis", "Диагноз");
                dataGridView3.Columns.Add("Count", "Количество диагнозов");

                while (reader.Read())
                {
                    dataGridView3.Rows.Add(reader["Диагноз"], reader["Количество диагнозов"]);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlDataAdapter sqlAdapter = new SqlDataAdapter("GetPatientsByBirthDateAndGender", sqlConnection);
                sqlAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                sqlAdapter.SelectCommand.Parameters.Add(new SqlParameter("@DateOfBirth", SqlDbType.Date));
                sqlAdapter.SelectCommand.Parameters["@DateOfBirth"].Value = dateTimePicker1.Value;

                // Добавляем параметры для пола
                sqlAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Gender", SqlDbType.VarChar));
                sqlAdapter.SelectCommand.Parameters["@Gender"].Value = comboBox1.SelectedItem.ToString();

                // Создаем DataSet для хранения результата
                DataSet dataSet = new DataSet();

                // Заполняем DataSet данными из хранимой процедуры
                sqlAdapter.Fill(dataSet, "PatientsReport");

                // Привязываем результат к DataGridView
                dataGridView4.DataSource = dataSet.Tables["PatientsReport"];
            }
        }
    }
}