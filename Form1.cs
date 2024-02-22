using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Core.ExcelPackage;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices.ComTypes;

namespace MFUAnalyzer
{
    public partial class Form1 : Form
    {
        string programDirectory;
        string temporaryDirectory;
        string templateDirectory;
        List<string> firstColumnData = new List<string>();
        List<string> secondColumnData = new List<string>();
        List<List<string>> allData = new List<List<string>>();
        private List<bool> checkboxStatusList = new List<bool>();
        int rowCount;
        private int currentRowIndex = 0;
        List<string> firstColumnDataTemporary = new List<string>();
 

        string excelFilePath;
       // string excelFilePath = MFUAnalyzer.Properties.Settings.Default.fisierOverview;
        string utilizatorMfu = MFUAnalyzer.Properties.Settings.Default.Utilizator;
        
        public Form1()
        {

            InitializeComponent();
            // Set the license context before creating any ExcelPackage instances
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            // Initialize programDirectory in the constructor
            programDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine the program directory with the "Temporary" folder
            temporaryDirectory = Path.Combine(programDirectory, "Temporar");
            templateDirectory = Path.Combine(programDirectory, "Template");
            // Create the directory if it doesn't exist
            if (!Directory.Exists(temporaryDirectory))
            {
                Directory.CreateDirectory(temporaryDirectory);
            }
            if (!Directory.Exists(templateDirectory))
            {
                Directory.CreateDirectory(templateDirectory);
            }
            //  LoadSettings();
            CreateExcelFile(programDirectory, templateDirectory, "OverviewTest.xlsx");
            string excelFilePath = Path.Combine(programDirectory, templateDirectory, "OverviewTest.xlsx");

            //  fisierOverview_txt.Text = MFUAnalyzer.Properties.Settings.Default.fisierOverview;
            //  rezumatCMK_txt.Text = MFUAnalyzer.Properties.Settings.Default.rezumatCMK;
            utilizator_txt.Text= MFUAnalyzer.Properties.Settings.Default.Utilizator;
            directorTemporar_txt.Text = temporaryDirectory;
            dataGridView1.Columns.Add("Column1", "Fisierele Template");
            dataGridView1.Columns.Add("Column2", "Denumire");



            // Inserarea unei noi coloane dupa "Column2"
            DataGridViewTextBoxColumn column3 = new DataGridViewTextBoxColumn
            {
                Name = "Column3",
                HeaderText = "Varianta",
                Width = 150
            };
            dataGridView1.Columns.Insert(2, column3);
            dataGridView1.Columns["Column3"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Column3"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            

            // Inserare checkbox dupa "Column3"
            DataGridViewCheckBoxColumn checkboxColumn = new DataGridViewCheckBoxColumn
            {
                Name = "CheckboxColumn",
                HeaderText = "#",
                Width = 133
            };
            dataGridView1.Columns.Insert(3, checkboxColumn);

            // Inserare coloana
            DataGridViewTextBoxColumn columnDert = new DataGridViewTextBoxColumn
            {
                Name = "ColumnCmk",
                HeaderText = "Cmk",
                Width = 150
            };
            dataGridView1.Columns.Add(columnDert);
            dataGridView1.Columns["ColumnCmk"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["ColumnCmk"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridView1.Columns.Add("DeleteButton", "Sterge");
            dataGridView1.Columns["DeleteButton"].DefaultCellStyle.NullValue = "Sterge";
            dataGridView1.Columns["DeleteButton"].DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);

            // Setare latime coloana 1 si 2
            dataGridView1.Columns["Column1"].Width = 450;
            dataGridView1.Columns["Column2"].Width = 200;
            dataGridView1.Columns["Column1"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["Column2"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["CheckboxColumn"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
           

            // Incarcare date din fisierul JSON
            LoadDataFromJson();
            CitireColoanaVariante();

            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;



        }



        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == dataGridView1.Columns["CheckboxColumn"].Index && e.RowIndex >= 0)
            {
                DataGridViewCheckBoxCell checkBoxCell = dataGridView1.Rows[e.RowIndex].Cells["CheckboxColumn"] as DataGridViewCheckBoxCell;

                if (checkBoxCell != null)
                {
                    bool isChecked = Convert.ToBoolean(checkBoxCell.Value);
                    bool wasChecked = Convert.ToBoolean(checkBoxCell.DefaultNewRowValue);

                    // verificare daca checkbox e checked sau nu
                    if (isChecked != wasChecked)
                    {
                        if (isChecked)
                        {
                            // Checkbox se adauga in lista
                            string firstColumnValue = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["Column1"].Value);
                            string secondColumnValue = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["Column2"].Value);

                            firstColumnData.Add(firstColumnValue);
                            secondColumnData.Add(secondColumnValue);
                        }
                        else
                        {
                            // Checkbox este unchecked si se elimina din lista
                            string firstColumnValue = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["Column1"].Value);
                            string secondColumnValue = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["Column2"].Value);

                            firstColumnData.Remove(firstColumnValue);
                            secondColumnData.Remove(secondColumnValue);
                        }

                        AddFirstColumnDataToList();
                        AddSecondColumnDataToList();
                    }
                }
            }


            // Salvare date in fisierul Json 
            SaveDataToJson();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["DeleteButton"].Index && e.RowIndex >= 0)
            {
                if (e.RowIndex < dataGridView1.Rows.Count - 1)
                {
                   
                    dataGridView1.Rows.RemoveAt(e.RowIndex);

                    // Salvare date in fisierul Json 
                    SaveDataToJson();
                }
                else
                {

                    MessageBox.Show("Ultimul rand nu poate fi sters.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dataGridView1.Rows[e.RowIndex].Cells["DeleteButton"].ReadOnly = true;

                }
            }
        }

        private void SaveDataToJson()
        {

            List<RowData> rowDataList = new List<RowData>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow) 
                {
                    rowDataList.Add(new RowData
                    {
                        Column1 = Convert.ToString(row.Cells["Column1"].Value),
                        Column2 = Convert.ToString(row.Cells["Column2"].Value),
                        Column3 = Convert.ToString(row.Cells["Column3"].Value),
                        
                    });
                }
            }

            string jsonData = JsonConvert.SerializeObject(rowDataList, Formatting.Indented);
            File.WriteAllText("data.json", jsonData);
        }

        private RadioButton lastSelectedRadioButton = null;

        private void CitireColoanaVariante()
        {
            // Extragerea datelor unice din coloana a 3-a si anume varianta
            var uniqueValues = dataGridView1.Rows.Cast<DataGridViewRow>()
                .Take(dataGridView1.Rows.Count - 1) // nu se include si ultimul rand care e gol
                .Select(row => row.Cells["Column3"].Value?.ToString())
                .ToList();

            // excluderea celor null sau empty
            uniqueValues = uniqueValues.Where(value => !string.IsNullOrEmpty(value))
                .Distinct()
                .ToList();

            // crearea unui buton radio pt fiecare varianta
            foreach (var value in uniqueValues)
            {
                var radioButton = new RadioButton
                {
                    Text = value,
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10 + 25 * uniqueValues.IndexOf(value))
                };

                // labelul pt radio buton
                var label = new System.Windows.Forms.Label
                {
                    Text = value,
                    Location = new System.Drawing.Point(10, 10 + 25 * uniqueValues.IndexOf(value))
                };

               
                panel2.Controls.Add(radioButton);
                panel2.Controls.Add(label);

                radioButton.CheckedChanged += (sender, e) =>
                {
                    if (radioButton.Checked)
                    {
                        
                        UncheckCorrespondingCheckBox();

                      
                        string variantValue = radioButton.Text;
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow && row.Cells["Column3"].Value?.ToString() == variantValue)
                            {
                                DataGridViewCheckBoxCell checkBoxCell = row.Cells["CheckboxColumn"] as DataGridViewCheckBoxCell;
                                if (checkBoxCell != null)
                                {
                                    checkBoxCell.Value = true;
                                    lastSelectedRadioButton = radioButton;
                                }
                            }
                        }
                    }
                };
            }
        }

        private void UncheckCorrespondingCheckBox()
        {
           
            if (lastSelectedRadioButton != null)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow && row.Cells["Column3"].Value?.ToString() == lastSelectedRadioButton.Text)
                    {
                        DataGridViewCheckBoxCell checkBoxCell = row.Cells["CheckboxColumn"] as DataGridViewCheckBoxCell;
                        if (checkBoxCell != null)
                        {
                            checkBoxCell.Value = false;
                        }
                    }
                }
            }
        }



        private void LoadDataFromJson()
        {
            try
            {
                string jsonData = File.ReadAllText("data.json");
                List<RowData> rowDataList = JsonConvert.DeserializeObject<List<RowData>>(jsonData);

                if (rowDataList != null)
                {
                    foreach (var rowData in rowDataList)
                    {
                        dataGridView1.Rows.Add(rowData.Column1, rowData.Column2, rowData.Column3);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                
            }

        }

        public void AddFirstColumnDataToList()
        {
           
            firstColumnData.Clear();

            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                DataGridViewCheckBoxCell checkBoxCell = row.Cells["CheckboxColumn"] as DataGridViewCheckBoxCell;

                if (checkBoxCell != null && Convert.ToBoolean(checkBoxCell.Value))
                {
                    firstColumnData.Add(row.Cells[0].Value.ToString());
                }
            }
           
        }

        public void AddSecondColumnDataToList()
        {
            
            secondColumnData.Clear();

            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                DataGridViewCheckBoxCell checkBoxCell = row.Cells["CheckboxColumn"] as DataGridViewCheckBoxCell;

                if (checkBoxCell != null && Convert.ToBoolean(checkBoxCell.Value))
                {
                    secondColumnData.Add(row.Cells[1].Value.ToString());
                }
            }
           
        }
        private void SelectExcelFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx";
            openFileDialog.Title = "Select an Excel File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                fisierlogfile_txt.Text = filePath;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            SelectExcelFile();
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            

            if (!IsAnyRadioButtonChecked())
            {
                MessageBox.Show("Nu este selectata varianta!");
                return;
            }
            if (String.IsNullOrEmpty(fisierlogfile_txt.Text))
            {

                MessageBox.Show("Nu este selectat fisierul LogFile.");
                
                return;
            }

            AddFirstColumnDataToList();
            AddSecondColumnDataToList();
            CopyFilesToNewLocation(temporaryDirectory);
            ReadAllDataFromLogFile();

          

            for (int i = 0; i < firstColumnDataTemporary.Count; i++)
            {
                await Task.Run(() => WriteExcelFileAsync(firstColumnDataTemporary[i], 0, i));
            }
           
            MessageBox.Show("Final ");
            
        }

        public void ReadAllDataFromLogFile()
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            string filePath = fisierlogfile_txt.Text;

            FileInfo fileInfo = new FileInfo(filePath);
            // Stergere inainte de a efectua citirea
            allData.Clear();

            using (var package = new OfficeOpenXml.ExcelPackage(fileInfo))
            {
                OfficeOpenXml.ExcelWorksheet sheet = package.Workbook.Worksheets[0]; // citire sheet 1

                int lastRow = sheet.Dimension.End.Row; // numarul total de randuri



               


                foreach (string columnName in secondColumnData)
                {
                    
                    int col = sheet.Cells[1, 1, 1, sheet.Dimension.End.Column].FirstOrDefault(c => c.Text == columnName)?.Start.Column ?? -1;

                    if (col != -1)
                    {
                        List<string> columnData = new List<string>();

                        for (int row = 2; row <= lastRow; row++)
                        {
                            var cellValue = sheet.Cells[row, col].Value;
                            columnData.Add(cellValue?.ToString() ?? string.Empty);
                        }

                        allData.Add(columnData);
                    }
                    else
                    {
                        
                        Console.WriteLine($"Coloana '{columnName}' nu a fost gasita.");
                    }
                }

 
                int rowCount = lastRow;
            }
        }



        //private void DisplayFirstColumnData()
        //{
        //    label1.Text = "Prima coloanaa:\n" + string.Join("\n", firstColumnData);
        //}

        //private void DisplaySecondColumnData()
        //{
        //    label2.Text = "A doua coloana:\n" + string.Join("\n", secondColumnData);
        //}

        public async Task WriteExcelFileAsync(string deScris, int startRow, int coloana)
        {
            await Task.Run(() =>
            {
                Microsoft.Office.Interop.Excel.Application excel2 = new Microsoft.Office.Interop.Excel.Application();
                Workbook workbook = excel2.Workbooks.Open(deScris);
                Worksheet sheet = workbook.Worksheets[2];

                for (int i = 0; i < 50; i++)
                {
                    if (startRow + i < allData[coloana].Count)
                    {
                        sheet.Range[$"B{i + 5}"].Value = allData[coloana][i + startRow];
                    }
                    else
                    {
                        break; 
                    }
                }

                sheet.Range["O11"].Value = DateTime.Now.ToString("dd.MM.yyyy");
                sheet.Range["I11"].Value = utilizatorMfu;
                workbook.Save();

                Range cell = sheet.Cells[15, 14];
                double numarCelula = cell.Value;

                string formattedText = numarCelula.ToString("F2");

                workbook.Close();
                excel2.Application.Quit();
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(sheet);
                Marshal.ReleaseComObject(excel2);

                UpdateDataGridViewColumnValue(currentRowIndex, formattedText);

                //  currentRowIndex++;
            });
        }



        private void UpdateDataGridViewColumnValue(int rowIndex, string formattedText)
        {
            while (rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count)
            {

                DataGridViewCheckBoxCell checkBoxCell = dataGridView1.Rows[rowIndex].Cells["CheckboxColumn"] as DataGridViewCheckBoxCell;
                bool isChecked = (checkBoxCell != null) && Convert.ToBoolean(checkBoxCell.Value);


                if (isChecked)
                {
                    // Invoke pt update de UI pe thread principal
                    dataGridView1.Invoke((MethodInvoker)delegate
                    {
                        dataGridView1.Rows[rowIndex].Cells["ColumnCmk"].Value = formattedText;

                    
                        double cmkValue = Convert.ToDouble(formattedText);

                        if (cmkValue < 1.67)
                        {
                            dataGridView1.Rows[rowIndex].Cells["ColumnCmk"].Style.ForeColor = Color.Red;
                        }
                        else
                        {
                            dataGridView1.Rows[rowIndex].Cells["ColumnCmk"].Style.ForeColor = Color.Green;
                        }
                    });

                    currentRowIndex++;
                    break; 
                }
                else
                {
                    currentRowIndex++;
                    rowIndex++; 
                }
            }
        }


        private void CopyFilesToNewLocation(string destinationFolder)
        {
            foreach (var filePath in firstColumnData)
            {
                try
                {
                    string fileName = Path.GetFileName(filePath);
                    string destinationPath = Path.Combine(destinationFolder, fileName);

                    File.Copy(filePath, destinationPath, true);

                    
                    firstColumnDataTemporary.Add(destinationPath);

                  
                    Console.WriteLine($"File '{fileName}' copied to '{destinationFolder}'.");
                }
                catch (Exception ex)
                {
                  
                    Console.WriteLine($"Error copying file: {ex.Message}");
                }
            }
        }

        private void iesire_tbn_Click(object sender, EventArgs e)
        {
            InchidereProces();
            this.Close();
        }

        public void InchidereProces()
        {
            Process[] excelProcesses = Process.GetProcessesByName("excel");
            foreach (Process p in excelProcesses)
            {
                if (string.IsNullOrEmpty(p.MainWindowTitle)) // 
                {
                    p.Kill();
                }
            }
        }

        private void raport_btn_Click(object sender, EventArgs e)
        {
           
            ScriereFisierRaport(GetColumnCmkValues(), secondColumnData, excelFilePath);
            MutareFisiere(temporaryDirectory, temporaryDirectory);
           
        }

        // pentru mutarea fisierelor dupa ce au fost completate cu datele din logfile
        public void MutareFisiere(string sursaTemplate, string destinatieTemplate)
        {
            string dataDeAzi = DateTime.Now.ToString("ddMMyyyyHHmm");
            string newDirectoryPath = System.IO.Path.Combine(destinatieTemplate, dataDeAzi);

            if (!Directory.Exists(newDirectoryPath))
            {
                Directory.CreateDirectory(newDirectoryPath);
            }

            string[] filePaths = Directory.GetFiles(sursaTemplate);
            foreach (var filePath in filePaths)
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                string destinationPath = System.IO.Path.Combine(newDirectoryPath, fileName);

                if (!File.Exists(destinationPath))
                {
                    File.Move(filePath, destinationPath);
                }
            }

           // MessageBox.Show("Raportul a fost generat si poate fi gasit la adresa" + " " + newDirectoryPath);
            ShowCustomMessageBox("Raportul a fost generat cu succes.", newDirectoryPath);
        }

        private List<object> GetColumnCmkValues()
        {
            List<object> columnCmkValues = new List<object>();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                
                object cmkValue = dataGridView1.Rows[i].Cells["ColumnCmk"].Value;

               
                if (cmkValue != null)
                {
                    columnCmkValues.Add(cmkValue);
                }
            }

            return columnCmkValues;
        }

     

        void ScriereFisierRaport(List<object> columnCmkValues, List<string> secondColumnData, string filePath)
        {
            string directorTemporar1 = templateDirectory;
            string fisierTemporar1 = "OverviewTest.xlsx";
            string directorTemporar2 = temporaryDirectory;

            string sourcePathOverviewFile = Path.Combine(directorTemporar1, Path.GetFileName(fisierTemporar1));
            string destPathOverviewFile = Path.Combine(directorTemporar2, Path.GetFileName(fisierTemporar1));

            File.Copy(sourcePathOverviewFile, destPathOverviewFile, true);
            System.Threading.Thread.Sleep(1000);

            FileInfo excelFile = new FileInfo(destPathOverviewFile);
            

            using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(excelFile))
            {
                OfficeOpenXml.ExcelWorksheet worksheet;

                if (package.Workbook.Worksheets.Count == 0)
                {
                   
                    worksheet = package.Workbook.Worksheets.Add("Sheet1");
                }
                else
                {
                    
                    worksheet = package.Workbook.Worksheets[0];
                }

                // Writing data
                for (int i = 0; i < Math.Max(columnCmkValues.Count, secondColumnData.Count); i++)
                {
                    if (i < secondColumnData.Count)
                        worksheet.Cells[i + 3, 2].Value = secondColumnData[i];

                    if (i < columnCmkValues.Count)
                    {
                        if (Convert.ToDouble(columnCmkValues[i])>=1.67)
                        {
                            worksheet.Cells[i + 3, 3].Value = "OK";
                        }
                        else
                        {
                            worksheet.Cells[i + 3, 3].Value = "NOK";
                        }
                        worksheet.Cells[i + 3, 4].Value = columnCmkValues[i];
                    }
                        
                }
                string dataDeAzi = DateTime.Now.ToString("dd/MM/yyyy");
                worksheet.Cells[1, 3].Value = dataDeAzi.ToString();

             
                package.Save();
            }
        }

        private List<double> CalculateCmk(double[] values, double upperLimit, double lowerLimit)
        {

            List<double> cmkValues = new List<double>();
            int startIndex = 0;

            while (startIndex + 50 <= values.Length)
            {
                double[] subset = values.Skip(startIndex).Take(50).ToArray();
                double mean = subset.Average();
                double stdDev = Math.Sqrt(subset.Select(x => (x - mean) * (x - mean)).Sum() / (50 - 1));

                double cmk = Math.Min((upperLimit - mean) / (3 * stdDev), (mean - lowerLimit) / (3 * stdDev));

                cmkValues.Add(cmk);


                startIndex++;
            }

            return cmkValues;
        }

        public void CalculateCmkForCheckedColumns()
        {
            double upperLimit = Convert.ToDouble(usl_txt.Text);
            double lowerLimit = Convert.ToDouble(lsl_txt.Text);

            StringBuilder resultStringBuilder = new StringBuilder();
            List<double> cmkMax = new List<double>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
           
                if (row.Cells["CheckboxColumn"] is DataGridViewCheckBoxCell checkBoxCell &&
                    checkBoxCell.Value != null && (bool)checkBoxCell.Value)
                {
                    int rowIndex = row.Index;

                    if (rowIndex >= 0 )
                    {
                        var columnData = allData[0];

                        double[] values = columnData.Select(double.Parse).ToArray();

                       
                        List<double> cmkValues = CalculateCmk(values, upperLimit, lowerLimit);

                       
                        for (int k = 0; k < cmkValues.Count; k++)
                        {
                            resultStringBuilder.AppendLine($"Cmk for dataset {k + 1} in row {rowIndex + 1}: {cmkValues[k]}");
                            cmkMax.Add(cmkValues[k]);
                        }
                    }
                }
            }

           
            resultStringBuilder.AppendLine($"Max Cmk: {(cmkMax.Any() ? cmkMax.Max().ToString() : "N/A")}");
            string fisierStudiu = "StudiuCmk.txt";
            string filePath = Path.Combine(temporaryDirectory, fisierStudiu);
            // string filePath = @"G:\01_TestMFU\03_Test_Temporar\cmk.txt";
           // string filePath = MFUAnalyzer.Properties.Settings.Default.rezumatCMK;
             File.WriteAllText(filePath, resultStringBuilder.ToString());
            // Display the custom message box with the open folder button
            ShowCustomMessageBox($"Studiul Cmk a fost generat cu succes.", filePath);
        }

       

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fisierlogfile_txt.Text))
            {
                MessageBox.Show("Fisierul LogFile nu e selectat!");
                return; 
            }
            if (string.IsNullOrEmpty(lsl_txt.Text) || string.IsNullOrEmpty(usl_txt.Text))
            {
                MessageBox.Show("Valoarea USL și LSL nu pot fi goale!");
                return; 
            }

            if (Convert.ToDouble(lsl_txt.Text) >= Convert.ToDouble(usl_txt.Text))
            {
                MessageBox.Show("Valoarea USL trebuie să fie mai mare ca și LSL!");
                return; 
            }
            bool atLeastOneChecked = false;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                
                if (row.Cells.Count > 3 && row.Cells[3] is DataGridViewCheckBoxCell checkBoxCell &&
                    checkBoxCell.Value != null && (bool)checkBoxCell.Value)
                {
                    
                    atLeastOneChecked = true;

                    
                    int rowIndex = row.Index;

                }
            }

            if (!atLeastOneChecked)
            {

                MessageBox.Show("Nu este selectat fisierul la care se face studiul.");
                return;
            }

            ReadAllDataFromLogFile();

            CalculateCmkForCheckedColumns();

           
        }
        
        private bool IsAnyRadioButtonChecked()
        {
            foreach (Control control in panel2.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    return true;
                }
            }
            return false;
        }
        private void LoadSettings()
        {
            // Load settings from the configuration file
            utilizator_txt.Text = MFUAnalyzer.Properties.Settings.Default.Utilizator;
        }
        static void CreateExcelFile(string programDirectory, string templateDirectoryName, string excelFileName)
        {
            string templateDirectory = Path.Combine(programDirectory, templateDirectoryName);
            string excelFilePath = Path.Combine(templateDirectory, excelFileName);

            // Step 1: Check if the directory exists, and create it if it doesn't
            if (!Directory.Exists(templateDirectory))
            {
                Directory.CreateDirectory(templateDirectory);
            }

            // Step 2: Create the Excel file
            // Step 2: Create the Excel file
            if (!File.Exists(excelFilePath))
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    // Add a default worksheet to the Excel package
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    // Add code here to customize the Excel file if needed

                    // Save the Excel file
                    package.SaveAs(new FileInfo(excelFilePath));
                }

                MessageBox.Show($"Fisierul excell '{excelFileName}' a fost creat in directorul '{templateDirectory}'.");
            }
          
        }

        private static void ShowCustomMessageBox(string message, string linkPath)
        {
            Form customMessageBox = new Form();
            customMessageBox.Text = "Mesaj Raport";
            customMessageBox.Size = new System.Drawing.Size(300, 150);
            customMessageBox.StartPosition = FormStartPosition.CenterScreen; 

            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = message;
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(10, 10);

            System.Windows.Forms.Button openFolderButton = new System.Windows.Forms.Button();
            openFolderButton.Text = "Deschide raportul";
            openFolderButton.Font = new System.Drawing.Font(openFolderButton.Font.FontFamily, 10, System.Drawing.FontStyle.Bold);
            openFolderButton.Size = new System.Drawing.Size(180, 50); 
            openFolderButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter; 
            openFolderButton.Location = new System.Drawing.Point((customMessageBox.Width - openFolderButton.Width) / 2, (customMessageBox.Height - openFolderButton.Height) / 2);

            openFolderButton.Click += (sender, e) =>
            {
                OpenFolder(linkPath);
                customMessageBox.Close(); 
            };

            customMessageBox.Controls.Add(label);
            customMessageBox.Controls.Add(openFolderButton);

            customMessageBox.ShowDialog();
        }

        private static void OpenFolder(string folderPath)
        {
           
            Process.Start("explorer.exe", folderPath);
        }



    }



    public class RowData
    {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public bool IsChecked { get; set; } 
    }


}
