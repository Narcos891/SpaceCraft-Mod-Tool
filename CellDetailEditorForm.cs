#nullable disable
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace SpaceCraft_Mod_Tool
{
    public class CellDetailEditorForm : Form
    {
        private DataGridView dgvMiniGrid;
        private TextBox txtSimpleContent;
        private Button btnSave;
        private Button btnCancel;
        private Label lblInfo;
        public string ModifiedValue { get; private set; }
        private bool isJsonArray = false;
        private bool darkMode = false;

        public CellDetailEditorForm(string columnName, string rawValue, bool isDarkMode)
        {
            darkMode = isDarkMode;
            InitializeComponent(columnName, rawValue, isDarkMode);
        }

        private void InitializeComponent(string columnName, string rawValue, bool isDarkMode)
        {
            this.Text = $"Editor Strutturato: {columnName}";
            this.Size = new Size(620, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White;

            lblInfo = new Label
            {
                Text = $"Modifica guidata stile Excel per la colonna: [{columnName}]",
                Left = 20,
                Top = 15,
                Width = 560,
                Height = 25,
                ForeColor = isDarkMode ? Color.Gainsboro : Color.Black,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            string trimmed = rawValue?.Trim() ?? string.Empty;
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                isJsonArray = true;
                SetupMiniGrid(trimmed, isDarkMode);
            }
            else
            {
                SetupSimpleTextBox(trimmed, isDarkMode);
            }

            btnSave = new Button
            {
                Text = "Salva",
                Left = 390,
                Top = 360,
                Width = 100,
                Height = 35,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Annulla",
                Left = 495,
                Top = 360,
                Width = 95,
                Height = 35,
                BackColor = isDarkMode ? Color.FromArgb(60, 60, 65) : Color.FromArgb(220, 220, 220),
                ForeColor = isDarkMode ? Color.White : Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblInfo);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void SetupMiniGrid(string jsonVal, bool isDarkMode)
        {
            dgvMiniGrid = new DataGridView
            {
                Left = 20,
                Top = 45,
                Width = 570,
                Height = 300,
                BackgroundColor = isDarkMode ? Color.FromArgb(37, 37, 38) : Color.White,
                ForeColor = isDarkMode ? Color.White : Color.Black,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                EnableHeadersVisualStyles = false
            };

            // Stile colori celle e intestazioni per evitare testi invisibili
            dgvMiniGrid.DefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            dgvMiniGrid.DefaultCellStyle.ForeColor = isDarkMode ? Color.White : Color.Black;
            dgvMiniGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvMiniGrid.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvMiniGrid.ColumnHeadersDefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(60, 60, 65) : Color.FromArgb(230, 230, 230);
            dgvMiniGrid.ColumnHeadersDefaultCellStyle.ForeColor = isDarkMode ? Color.White : Color.Black;
            dgvMiniGrid.GridColor = isDarkMode ? Color.FromArgb(80, 80, 80) : Color.LightGray;

            DataTable dt = new DataTable();
            dt.Columns.Add("item", typeof(string));
            dt.Columns.Add("qty", typeof(int));

            try
            {
                JArray arr = JArray.Parse(jsonVal);
                foreach (var obj in arr)
                {
                    DataRow row = dt.NewRow();
                    // Gestione flessibile delle maiuscole/minuscole nel JSON
                    row["item"] = obj["item"]?.ToString() ?? obj["Item"]?.ToString() ?? string.Empty;

                    int qty = 1;
                    if (obj["qty"] != null) int.TryParse(obj["qty"].ToString(), out qty);
                    else if (obj["Qty"] != null) int.TryParse(obj["Qty"].ToString(), out qty);
                    row["qty"] = qty;

                    dt.Rows.Add(row);
                }
            }
            catch
            {
                DataRow row = dt.NewRow();
                row["item"] = jsonVal;
                row["qty"] = 1;
                dt.Rows.Add(row);
            }

            dgvMiniGrid.DataSource = dt;
            this.Controls.Add(dgvMiniGrid);
        }

        private void SetupSimpleTextBox(string val, bool isDarkMode)
        {
            txtSimpleContent = new TextBox
            {
                Left = 20,
                Top = 45,
                Width = 570,
                Height = 300,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = val,
                Font = new Font("Consolas", 10f),
                BackColor = isDarkMode ? Color.FromArgb(37, 37, 38) : Color.White,
                ForeColor = isDarkMode ? Color.White : Color.Black
            };
            this.Controls.Add(txtSimpleContent);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (isJsonArray && dgvMiniGrid.DataSource is DataTable dt)
            {
                JArray newArray = new JArray();
                foreach (DataRow row in dt.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    string itemName = row["item"]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(itemName)) continue;

                    int qtyVal = 1;
                    if (row["qty"] != DBNull.Value)
                    {
                        int.TryParse(row["qty"].ToString(), out qtyVal);
                    }

                    JObject itemObj = new JObject
                    {
                        ["item"] = itemName,
                        ["qty"] = qtyVal
                    };
                    newArray.Add(itemObj);
                }

                ModifiedValue = newArray.ToString(Newtonsoft.Json.Formatting.None);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (txtSimpleContent != null)
            {
                ModifiedValue = txtSimpleContent.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}