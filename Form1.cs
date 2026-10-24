#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace SpaceCraft_Mod_Tool
{
    public partial class Form1 : Form
    {
        private Button btnOpenCdb;
        private Button btnSaveCdb;
        private Button btnToggleTheme;
        private Button btnSideEditorToggle;
        private TextBox txtSearch;
        private Panel topPanel;
        private FlowLayoutPanel panelTabs;
        private DataGridView dgvData;

        // Smart Editor Controls
        private Panel sideGuidedPanel;
        private ComboBox cbSideCategory;
        private ComboBox cbSideItem;
        private ComboBox cbSideProperty;
        private TextBox txtSmartValue; // Per valori singoli o stringhe
        private DataGridView dgvSmartGrid; // Griglia tabellare dinamica per array/oggetti (stile HIDE)
        private Button btnAddGridRow;
        private Button btnRemoveGridRow;
        private Button btnApplySmart;
        private Label lblSideWarning;
        private bool isSideEditorOpen = false;

        // Constant Controls
        private Panel panelConstantContainer;
        private ComboBox cmbConstantCategory;
        private ComboBox cmbConstantSubCategory;
        private DataGridView dgvConstantItems;

        private JObject currentCdbJson;
        private string loadedFilePath = string.Empty;
        private Button activeTabButton = null;
        private bool isDarkMode = true;

        private ConstantManager _constantManager = new ConstantManager();

        public Form1()
        {
            InitializeEditorUI();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();

            ApplyTheme();
            LoadDefaultDatabaseIfExists();
        }

        private void InitializeEditorUI()
        {
            this.Text = "SpaceCraft CastleDB Editor (Stile HIDE Avanzato + Smart Editor Tabellare + Ricerca)";
            this.Size = new Size(1400, 850);
            this.StartPosition = FormStartPosition.CenterScreen;

            topPanel = new Panel { Dock = DockStyle.Top, Height = 50 };

            btnOpenCdb = new Button { Text = "📂 Apri data.cdb / JSON", Left = 15, Top = 10, Width = 160, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnOpenCdb.Click += BtnOpenCdb_Click;

            btnSaveCdb = new Button { Text = "💾 Salva Database", Left = 185, Top = 10, Width = 140, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSaveCdb.Click += BtnSaveCdb_Click;

            btnToggleTheme = new Button { Left = 340, Top = 10, Width = 150, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnToggleTheme.Click += BtnToggleTheme_Click;

            btnSideEditorToggle = new Button { Text = "🪟 Pannello Guidato", Left = 505, Top = 10, Width = 150, Height = 30, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSideEditorToggle.Click += BtnSideEditorToggle_Click;

            Label lblSearch = new Label { Text = "🔍 Cerca:", Left = 675, Top = 15, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtSearch = new TextBox { Left = 730, Top = 12, Width = 250, Height = 25 };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            topPanel.Controls.Add(btnOpenCdb);
            topPanel.Controls.Add(btnSaveCdb);
            topPanel.Controls.Add(btnToggleTheme);
            topPanel.Controls.Add(btnSideEditorToggle);
            topPanel.Controls.Add(lblSearch);
            topPanel.Controls.Add(txtSearch);

            panelTabs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(5)
            };

            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvData.CellDoubleClick += DgvData_CellDoubleClick;

            this.Controls.Add(dgvData);
            this.Controls.Add(panelTabs);
            this.Controls.Add(topPanel);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvData.DataSource is DataTable dt)
            {
                string filterText = txtSearch.Text.Trim().Replace("'", "''");
                if (string.IsNullOrEmpty(filterText))
                {
                    dt.DefaultView.RowFilter = string.Empty;
                }
                else
                {
                    List<string> conditions = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        conditions.Add($"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{filterText}%'");
                    }
                    dt.DefaultView.RowFilter = string.Join(" OR ", conditions);
                }
            }
        }

        private void DgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string columnName = dgvData.Columns[e.ColumnIndex].Name;
            string cellValue = dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? string.Empty;

            using (var detailForm = new CellDetailEditorForm(columnName, cellValue, isDarkMode))
            {
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = detailForm.ModifiedValue;

                    if (dgvData.Tag is JObject sheet && sheet["lines"] is JArray lines)
                    {
                        if (e.RowIndex < lines.Count && lines[e.RowIndex] is JObject lineObj)
                        {
                            string updatedVal = detailForm.ModifiedValue;
                            try
                            {
                                lineObj[columnName] = JToken.Parse(updatedVal);
                            }
                            catch
                            {
                                lineObj[columnName] = updatedVal;
                            }
                        }
                    }
                }
            }
        }

        private void BtnToggleTheme_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();

            if (activeTabButton != null)
            {
                activeTabButton.BackColor = isDarkMode ? Color.FromArgb(0, 122, 204) : Color.FromArgb(0, 120, 215);
                activeTabButton.ForeColor = Color.White;
            }
        }

        private void ApplyTheme()
        {
            if (isDarkMode)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                topPanel.BackColor = Color.FromArgb(45, 45, 48);
                btnOpenCdb.BackColor = Color.FromArgb(60, 60, 65);
                btnOpenCdb.ForeColor = Color.White;
                btnSaveCdb.BackColor = Color.FromArgb(35, 100, 45);
                btnSaveCdb.ForeColor = Color.White;
                btnToggleTheme.Text = "☀️ Modalità Chiara";
                btnToggleTheme.BackColor = Color.FromArgb(70, 70, 75);
                btnToggleTheme.ForeColor = Color.Gold;

                txtSearch.BackColor = Color.FromArgb(50, 50, 52);
                txtSearch.ForeColor = Color.White;
                panelTabs.BackColor = Color.FromArgb(25, 25, 28);

                dgvData.BackgroundColor = Color.FromArgb(30, 30, 30);
                dgvData.DefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
                dgvData.DefaultCellStyle.ForeColor = Color.Gainsboro;
                dgvData.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
                dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                dgvData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                dgvData.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvData.GridColor = Color.FromArgb(60, 60, 60);

                if (panelConstantContainer != null) panelConstantContainer.BackColor = Color.FromArgb(30, 30, 30);
                if (dgvConstantItems != null)
                {
                    dgvConstantItems.BackgroundColor = Color.FromArgb(30, 30, 30);
                    dgvConstantItems.DefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
                    dgvConstantItems.DefaultCellStyle.ForeColor = Color.Gainsboro;
                    dgvConstantItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
                    dgvConstantItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                    dgvConstantItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                    dgvConstantItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgvConstantItems.GridColor = Color.FromArgb(60, 60, 60);
                }
            }
            else
            {
                this.BackColor = Color.White;
                topPanel.BackColor = Color.FromArgb(240, 240, 240);
                btnOpenCdb.BackColor = Color.FromArgb(225, 225, 225);
                btnOpenCdb.ForeColor = Color.Black;
                btnSaveCdb.BackColor = Color.FromArgb(220, 245, 220);
                btnSaveCdb.ForeColor = Color.DarkGreen;
                btnToggleTheme.Text = "🌙 Modalità Scura";
                btnToggleTheme.BackColor = Color.FromArgb(230, 230, 230);
                btnToggleTheme.ForeColor = Color.FromArgb(40, 40, 40);

                txtSearch.BackColor = Color.White;
                txtSearch.ForeColor = Color.Black;
                panelTabs.BackColor = Color.FromArgb(50, 55, 65);

                dgvData.BackgroundColor = Color.White;
                dgvData.DefaultCellStyle.BackColor = Color.White;
                dgvData.DefaultCellStyle.ForeColor = Color.Black;
                dgvData.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
                dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
                dgvData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
                dgvData.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dgvData.GridColor = Color.LightGray;

                if (panelConstantContainer != null) panelConstantContainer.BackColor = Color.White;
                if (dgvConstantItems != null)
                {
                    dgvConstantItems.BackgroundColor = Color.White;
                    dgvConstantItems.DefaultCellStyle.BackColor = Color.White;
                    dgvConstantItems.DefaultCellStyle.ForeColor = Color.Black;
                    dgvConstantItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
                    dgvConstantItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
                    dgvConstantItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
                    dgvConstantItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                    dgvConstantItems.GridColor = Color.LightGray;
                }
            }

            foreach (Control ctrl in panelTabs.Controls)
            {
                if (ctrl is Button btn && btn != activeTabButton)
                {
                    btn.BackColor = isDarkMode ? Color.FromArgb(55, 65, 80) : Color.FromArgb(70, 90, 120);
                    btn.ForeColor = Color.White;
                }
            }

            UpdateSidePanelTheme();
        }

        private void LoadDefaultDatabaseIfExists()
        {
            string defaultPath = Path.Combine(Environment.CurrentDirectory, "ModdingData", "data.cdb");
            if (!File.Exists(defaultPath)) defaultPath = "data.cdb";
            if (File.Exists(defaultPath)) LoadCdbFile(defaultPath);
        }

        private void BtnOpenCdb_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CastleDB / JSON (*.cdb;*.json)|*.cdb;*.json|Tutti i file (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK) LoadCdbFile(ofd.FileName);
            }
        }

        private void LoadCdbFile(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                currentCdbJson = JObject.Parse(jsonContent);
                loadedFilePath = filePath;

                List<ConstantItem> constantList = new List<ConstantItem>();
                if (currentCdbJson["sheets"] is JArray sheetsArr)
                {
                    foreach (var sheet in sheetsArr)
                    {
                        if (sheet["name"]?.ToString() == "constant")
                        {
                            JArray lines = sheet["lines"] as JArray;
                            if (lines != null)
                            {
                                foreach (var line in lines)
                                {
                                    string lineId = line["id"]?.ToString();
                                    string lineVal = line["val"]?.ToString();

                                    if (!string.IsNullOrEmpty(lineId))
                                    {
                                        constantList.Add(new ConstantItem
                                        {
                                            Id = lineId,
                                            Value = lineVal ?? ""
                                        });
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                _constantManager.LoadFromCdb(constantList);

                PopulateTabs();
                if (sideGuidedPanel != null && sideGuidedPanel.Visible) PopulateSideCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la lettura del file CDB:\n{ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateTabs()
        {
            panelTabs.Controls.Clear();
            activeTabButton = null;

            if (currentCdbJson == null || !currentCdbJson.ContainsKey("sheets")) return;

            JArray sheets = (JArray)currentCdbJson["sheets"];
            foreach (JObject sheet in sheets)
            {
                string sheetName = sheet["name"]?.ToString();
                if (string.IsNullOrEmpty(sheetName) || sheetName.Contains("@")) continue;

                Button btnTab = new Button
                {
                    Text = sheetName,
                    AutoSize = true,
                    MinimumSize = new Size(100, 34),
                    Margin = new Padding(4),
                    BackColor = isDarkMode ? Color.FromArgb(55, 65, 80) : Color.FromArgb(70, 90, 120),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = sheet
                };
                btnTab.FlatAppearance.BorderColor = isDarkMode ? Color.FromArgb(80, 95, 115) : Color.FromArgb(100, 130, 170);

                btnTab.Click += (s, e) =>
                {
                    if (activeTabButton != null)
                    {
                        activeTabButton.BackColor = isDarkMode ? Color.FromArgb(55, 65, 80) : Color.FromArgb(70, 90, 120);
                        activeTabButton.ForeColor = Color.White;
                    }
                    activeTabButton = btnTab;
                    activeTabButton.BackColor = isDarkMode ? Color.FromArgb(0, 122, 204) : Color.FromArgb(0, 120, 215);
                    activeTabButton.ForeColor = Color.White;

                    if (sheetName == "constant")
                    {
                        DisplayConstantInterface(sheet);
                    }
                    else
                    {
                        if (panelConstantContainer != null) panelConstantContainer.Visible = false;
                        dgvData.Visible = true;
                        DisplaySheetData(sheet);
                    }
                };

                panelTabs.Controls.Add(btnTab);
            }

            if (panelTabs.Controls.Count > 0 && panelTabs.Controls[0] is Button firstBtn)
            {
                firstBtn.PerformClick();
            }
        }

        private void DisplaySheetData(JObject sheet)
        {
            DataTable dt = new DataTable();

            JArray columns = sheet["columns"] as JArray;
            if (columns != null)
            {
                foreach (JObject col in columns)
                {
                    string colName = col["name"]?.ToString();
                    if (!string.IsNullOrEmpty(colName) && !dt.Columns.Contains(colName))
                    {
                        dt.Columns.Add(colName, typeof(string));
                    }
                }
            }

            if (!dt.Columns.Contains("id")) dt.Columns.Add("id", typeof(string));

            JArray lines = sheet["lines"] as JArray;
            if (lines != null)
            {
                foreach (JObject line in lines)
                {
                    DataRow row = dt.NewRow();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        if (line[dc.ColumnName] != null)
                        {
                            var val = line[dc.ColumnName];
                            row[dc.ColumnName] = val.Type == JTokenType.Object || val.Type == JTokenType.Array
                                ? val.ToString(Newtonsoft.Json.Formatting.None)
                                : val.ToString();
                        }
                        else
                        {
                            row[dc.ColumnName] = string.Empty;
                        }
                    }
                    dt.Rows.Add(row);
                }
            }

            txtSearch.Clear();
            dgvData.DataSource = dt;
            dgvData.Tag = sheet;
        }

        private void DisplayConstantInterface(JObject sheet)
        {
            dgvData.Visible = false;

            if (panelConstantContainer == null)
            {
                panelConstantContainer = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(15)
                };

                Panel topFilterPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 70
                };

                Label lblCat = new Label { Text = "Macro-Categoria:", Top = 5, Left = 0, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
                cmbConstantCategory = new ComboBox
                {
                    Top = 30,
                    Left = 0,
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDown,
                    AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                    AutoCompleteSource = AutoCompleteSource.ListItems
                };
                cmbConstantCategory.SelectedIndexChanged += CmbConstantCategory_SelectedIndexChanged;
                cmbConstantCategory.TextChanged += CmbConstantCategory_TextChanged;

                Label lblSubCat = new Label { Text = "Sotto-Categoria:", Top = 5, Left = 330, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
                cmbConstantSubCategory = new ComboBox
                {
                    Top = 30,
                    Left = 330,
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDown,
                    AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                    AutoCompleteSource = AutoCompleteSource.ListItems
                };
                cmbConstantSubCategory.SelectedIndexChanged += CmbConstantSubCategory_SelectedIndexChanged;
                cmbConstantSubCategory.TextChanged += CmbConstantSubCategory_TextChanged;

                topFilterPanel.Controls.Add(lblCat);
                topFilterPanel.Controls.Add(cmbConstantCategory);
                topFilterPanel.Controls.Add(lblSubCat);
                topFilterPanel.Controls.Add(cmbConstantSubCategory);

                dgvConstantItems = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BorderStyle = BorderStyle.None,
                    ReadOnly = true,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    EnableHeadersVisualStyles = false
                };
                dgvConstantItems.CellDoubleClick += DgvConstantItems_CellDoubleClick;

                panelConstantContainer.Controls.Add(dgvConstantItems);
                panelConstantContainer.Controls.Add(topFilterPanel);

                this.Controls.Add(panelConstantContainer);
            }

            panelConstantContainer.Visible = true;
            panelConstantContainer.BringToFront();

            cmbConstantCategory.DataSource = _constantManager.GetCategories();
        }

        private void CmbConstantCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbConstantCategory.SelectedItem != null)
            {
                string selectedCat = cmbConstantCategory.SelectedItem.ToString();
                cmbConstantSubCategory.DataSource = _constantManager.GetSubCategories(selectedCat);
            }
        }

        private void CmbConstantCategory_TextChanged(object sender, EventArgs e)
        {
            string typedCat = cmbConstantCategory.Text;
            if (cmbConstantCategory.Items.Contains(typedCat))
            {
                cmbConstantSubCategory.DataSource = _constantManager.GetSubCategories(typedCat);
            }
        }

        private void CmbConstantSubCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadConstantGridData();
        }

        private void CmbConstantSubCategory_TextChanged(object sender, EventArgs e)
        {
            LoadConstantGridData();
        }

        private void LoadConstantGridData()
        {
            if (cmbConstantCategory.SelectedItem != null)
            {
                string selectedCat = cmbConstantCategory.SelectedItem.ToString();
                string typedSubCat = cmbConstantSubCategory.Text;

                var items = _constantManager.GetFilteredItems(selectedCat, typedSubCat);

                DataTable dt = new DataTable();
                dt.Columns.Add("Id", typeof(string));
                dt.Columns.Add("DataType", typeof(string));
                dt.Columns.Add("Value", typeof(string));
                dt.Columns.Add("DescriptionIt", typeof(string));

                foreach (var item in items)
                {
                    dt.Rows.Add(item.Id, item.DataType, item.Value, item.DescriptionIt);
                }

                dgvConstantItems.DataSource = dt;

                if (dgvConstantItems.Columns["Id"] != null) dgvConstantItems.Columns["Id"].HeaderText = "Parametro (ID)";
                if (dgvConstantItems.Columns["DataType"] != null) dgvConstantItems.Columns["DataType"].HeaderText = "Tipo di Dato";
                if (dgvConstantItems.Columns["Value"] != null) dgvConstantItems.Columns["Value"].HeaderText = "Valore";
                if (dgvConstantItems.Columns["DescriptionIt"] != null) dgvConstantItems.Columns["DescriptionIt"].HeaderText = "Descrizione (IT)";
            }
        }

        private void DgvConstantItems_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string id = dgvConstantItems.Rows[e.RowIndex].Cells["Id"]?.Value?.ToString() ?? "";
            string columnName = dgvConstantItems.Columns[e.ColumnIndex].Name;

            if (columnName == "Id")
            {
                MessageBox.Show("L'ID del parametro non può essere modificato per preservare i riferimenti di gioco.", "Azione non consentita", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (columnName == "DescriptionIt")
            {
                string currentDesc = dgvConstantItems.Rows[e.RowIndex].Cells["DescriptionIt"].Value?.ToString() ?? "";

                using (var detailForm = new CellDetailEditorForm("Modifica Descrizione (IT)", currentDesc, isDarkMode))
                {
                    if (detailForm.ShowDialog() == DialogResult.OK)
                    {
                        dgvConstantItems.Rows[e.RowIndex].Cells["DescriptionIt"].Value = detailForm.ModifiedValue;
                        LoadConstantGridData();
                    }
                }
                return;
            }

            if (columnName == "Value" || columnName == "DataType")
            {
                string dataType = dgvConstantItems.Rows[e.RowIndex].Cells["DataType"].Value?.ToString() ?? "float";
                string currentVal = dgvConstantItems.Rows[e.RowIndex].Cells["Value"].Value?.ToString() ?? "";

                using (var detailForm = new CellDetailEditorForm("Modifica Valore Numerico", currentVal, isDarkMode))
                {
                    if (detailForm.ShowDialog() == DialogResult.OK)
                    {
                        string newNumericVal = detailForm.ModifiedValue;
                        _constantManager.UpdateValue(id, newNumericVal);

                        string wrappedJsonValue = newNumericVal;
                        if (dataType == "float" || dataType == "integer")
                        {
                            if (double.TryParse(newNumericVal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dVal))
                            {
                                wrappedJsonValue = $"{{\"{dataType}\": {newNumericVal.Replace(',', '.')}}}";
                            }
                            else
                            {
                                wrappedJsonValue = $"{{\"{dataType}\": {newNumericVal}}}";
                            }
                        }

                        if (currentCdbJson["sheets"] is JArray sheetsArr)
                        {
                            foreach (var sheet in sheetsArr)
                            {
                                if (sheet["name"]?.ToString() == "constant" && sheet["lines"] is JArray lines)
                                {
                                    foreach (var line in lines)
                                    {
                                        if (line["id"]?.ToString() == id)
                                        {
                                            try
                                            {
                                                line["val"] = JToken.Parse(wrappedJsonValue);
                                            }
                                            catch
                                            {
                                                line["val"] = wrappedJsonValue;
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        LoadConstantGridData();
                    }
                }
            }
        }

        private void BtnSaveCdb_Click(object sender, EventArgs e)
        {
            if (currentCdbJson == null)
            {
                MessageBox.Show("Nessun database aperto da salvare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string savePath = string.IsNullOrEmpty(loadedFilePath) ? Path.Combine(Environment.CurrentDirectory, "ModdingData", "data.cdb") : loadedFilePath;

            try
            {
                File.WriteAllText(savePath, currentCdbJson.ToString(Newtonsoft.Json.Formatting.Indented));
                MessageBox.Show($"Database salvato con successo in:\n{savePath}", "Salvato", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio:\n{ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSideEditorToggle_Click(object sender, EventArgs e)
        {
            isSideEditorOpen = !isSideEditorOpen;

            if (sideGuidedPanel == null)
            {
                sideGuidedPanel = new Panel
                {
                    Dock = DockStyle.Right,
                    Width = 480,
                    Padding = new Padding(15)
                };

                Label lblTitle = new Label { Text = "✨ Smart Editor Guidato", Top = 15, Left = 15, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true };

                Label lblCat = new Label { Text = "1. Categoria (Tabella):", Top = 50, Left = 15, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                cbSideCategory = new ComboBox { Top = 70, Left = 15, Width = 440, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.Suggest, AutoCompleteSource = AutoCompleteSource.ListItems };
                cbSideCategory.SelectedIndexChanged += CbSideCategory_SelectedIndexChanged;
                cbSideCategory.TextChanged += CbSideCategory_TextChanged;

                Label lblItem = new Label { Text = "2. Elemento / Riga:", Top = 110, Left = 15, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                cbSideItem = new ComboBox { Top = 130, Left = 15, Width = 440, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.Suggest, AutoCompleteSource = AutoCompleteSource.ListItems };
                cbSideItem.SelectedIndexChanged += CbSideItem_SelectedIndexChanged;

                Label lblProp = new Label { Text = "3. Parametro:", Top = 170, Left = 15, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
                cbSideProperty = new ComboBox { Top = 190, Left = 15, Width = 440, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.Suggest, AutoCompleteSource = AutoCompleteSource.ListItems };
                cbSideProperty.SelectedIndexChanged += CbSideProperty_SelectedIndexChanged;

                Label lblVal = new Label { Text = "4. Valore / Tabella Dati:", Top = 230, Left = 15, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

                // Casella di testo standard per valori semplici
                txtSmartValue = new TextBox
                {
                    Top = 255,
                    Left = 15,
                    Width = 440,
                    Height = 280,
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Consolas", 9.5f)
                };

                // Griglia tabellare dinamica (stile HIDE per array/oggetti complessi tipo inputs/outputs/buyout)
                dgvSmartGrid = new DataGridView
                {
                    Top = 255,
                    Left = 15,
                    Width = 440,
                    Height = 280,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BorderStyle = BorderStyle.FixedSingle,
                    EnableHeadersVisualStyles = false,
                    SelectionMode = DataGridViewSelectionMode.CellSelect,
                    Visible = false
                };

                btnAddGridRow = new Button
                {
                    Text = "➕ Aggiungi Riga",
                    Top = 542,
                    Left = 15,
                    Width = 215,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(45, 120, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Visible = false
                };
                btnAddGridRow.Click += BtnAddGridRow_Click;

                btnRemoveGridRow = new Button
                {
                    Text = "➖ Rimuovi Selezionato",
                    Top = 542,
                    Left = 240,
                    Width = 215,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(160, 50, 50),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Visible = false
                };
                btnRemoveGridRow.Click += BtnRemoveGridRow_Click;

                btnApplySmart = new Button
                {
                    Text = "⚡ Salva Modifiche nel DB",
                    Top = 585,
                    Left = 15,
                    Width = 440,
                    Height = 38,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnApplySmart.Click += BtnApplySmart_Click;

                lblSideWarning = new Label
                {
                    Top = 630,
                    Left = 15,
                    Width = 440,
                    Height = 50,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Text = "💡 Modifica i dati direttamente nella tabella o nel campo."
                };

                sideGuidedPanel.Controls.Add(lblTitle);
                sideGuidedPanel.Controls.Add(lblCat);
                sideGuidedPanel.Controls.Add(cbSideCategory);
                sideGuidedPanel.Controls.Add(lblItem);
                sideGuidedPanel.Controls.Add(cbSideItem);
                sideGuidedPanel.Controls.Add(lblProp);
                sideGuidedPanel.Controls.Add(cbSideProperty);
                sideGuidedPanel.Controls.Add(lblVal);
                sideGuidedPanel.Controls.Add(txtSmartValue);
                sideGuidedPanel.Controls.Add(dgvSmartGrid);
                sideGuidedPanel.Controls.Add(btnAddGridRow);
                sideGuidedPanel.Controls.Add(btnRemoveGridRow);
                sideGuidedPanel.Controls.Add(btnApplySmart);
                sideGuidedPanel.Controls.Add(lblSideWarning);

                this.Controls.Add(sideGuidedPanel);
            }

            sideGuidedPanel.Visible = isSideEditorOpen;
            if (isSideEditorOpen) PopulateSideCategories();
            ApplyTheme();
        }

        private void PopulateSideCategories()
        {
            if (cbSideCategory == null) return;
            cbSideCategory.Items.Clear();
            if (currentCdbJson == null || !currentCdbJson.ContainsKey("sheets")) return;

            JArray sheets = (JArray)currentCdbJson["sheets"];
            foreach (JObject sheet in sheets)
            {
                string sheetName = sheet["name"]?.ToString();
                if (!string.IsNullOrEmpty(sheetName) && !sheetName.Contains("@"))
                {
                    cbSideCategory.Items.Add(sheetName);
                }
            }
        }

        private void CbSideCategory_TextChanged(object sender, EventArgs e)
        {
            string typed = cbSideCategory.Text;
            if (cbSideCategory.Items.Contains(typed)) LoadItemsForCategory(typed);
        }

        private void CbSideCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbSideCategory.SelectedItem != null) LoadItemsForCategory(cbSideCategory.SelectedItem.ToString());
        }

        private void LoadItemsForCategory(string selectedSheetName)
        {
            if (cbSideItem == null) return;
            cbSideItem.Items.Clear();
            cbSideProperty.Items.Clear();

            JArray sheets = (JArray)currentCdbJson["sheets"];
            JObject targetSheet = null;
            foreach (JObject sheet in sheets)
            {
                if (sheet["name"]?.ToString() == selectedSheetName)
                {
                    targetSheet = sheet;
                    break;
                }
            }

            if (targetSheet != null)
            {
                if (targetSheet["columns"] is JArray columns)
                {
                    foreach (JObject col in columns)
                    {
                        string colName = col["name"]?.ToString();
                        if (!string.IsNullOrEmpty(colName)) cbSideProperty.Items.Add(colName);
                    }
                }

                if (targetSheet["lines"] is JArray lines)
                {
                    foreach (JObject line in lines)
                    {
                        string identifier = line["id"]?.ToString() ?? line["name"]?.ToString() ?? "Riga senza ID";
                        cbSideItem.Items.Add(identifier);
                    }
                }
            }
        }

        private void CbSideItem_SelectedIndexChanged(object sender, EventArgs e) => UpdateSmartEditorView();
        private void CbSideProperty_SelectedIndexChanged(object sender, EventArgs e) => UpdateSmartEditorView();

        private void UpdateSmartEditorView()
        {
            if (cbSideCategory.SelectedItem == null || cbSideItem.SelectedItem == null || cbSideProperty.SelectedItem == null) return;

            string sheetName = cbSideCategory.SelectedItem.ToString();
            string selectedId = cbSideItem.SelectedItem.ToString();
            string propName = cbSideProperty.SelectedItem.ToString();

            JArray sheets = (JArray)currentCdbJson["sheets"];
            foreach (JObject sheet in sheets)
            {
                if (sheet["name"]?.ToString() == sheetName && sheet["lines"] is JArray lines)
                {
                    foreach (JObject line in lines)
                    {
                        string lineId = line["id"]?.ToString() ?? line["name"]?.ToString() ?? "";
                        if (lineId == selectedId)
                        {
                            var targetProp = line[propName];

                            // Verifichiamo se il campo è un Array o un Oggetto complesso (es. inputs, outputs, buyout, props)
                            bool isComplex = (targetProp is JArray || (targetProp is JObject obj && obj.Count > 0 && propName != "val"));

                            txtSmartValue.Visible = !isComplex;
                            dgvSmartGrid.Visible = isComplex;
                            btnAddGridRow.Visible = isComplex;
                            btnRemoveGridRow.Visible = isComplex;

                            if (isComplex)
                            {
                                PopulateDynamicGrid(targetProp, propName);
                            }
                            else
                            {
                                txtSmartValue.Text = targetProp?.ToString() ?? "";
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void PopulateDynamicGrid(JToken targetProp, string propName)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("#", typeof(int));

            if (targetProp is JArray jArr)
            {
                // Raccogliamo tutte le chiavi presenti negli elementi dell'array per creare le colonne
                HashSet<string> keys = new HashSet<string>();
                foreach (var item in jArr)
                {
                    if (item is JObject jObj)
                    {
                        foreach (var p in jObj.Properties()) keys.Add(p.Name);
                    }
                }

                if (keys.Count == 0) keys.Add("item");
                foreach (var k in keys) dt.Columns.Add(k, typeof(string));

                int idx = 0;
                foreach (var item in jArr)
                {
                    DataRow row = dt.NewRow();
                    row["#"] = idx++;
                    if (item is JObject jObj)
                    {
                        foreach (var k in keys)
                        {
                            row[k] = jObj[k]?.ToString() ?? "";
                        }
                    }
                    else
                    {
                        row[dt.Columns.Count > 1 ? dt.Columns[1].ColumnName : "item"] = item.ToString();
                    }
                    dt.Rows.Add(row);
                }
            }
            else if (targetProp is JObject jObj)
            {
                dt.Columns.Add("item", typeof(string));
                dt.Columns.Add("value", typeof(string));

                int idx = 0;
                foreach (var prop in jObj.Properties())
                {
                    dt.Rows.Add(idx++, prop.Name, prop.Value?.ToString() ?? "");
                }
            }

            dgvSmartGrid.DataSource = dt;
            if (dgvSmartGrid.Columns["#"] != null) { dgvSmartGrid.Columns["#"].Width = 30; dgvSmartGrid.Columns["#"].ReadOnly = true; }
        }

        private void BtnAddGridRow_Click(object sender, EventArgs e)
        {
            if (dgvSmartGrid.DataSource is DataTable dt)
            {
                DataRow newRow = dt.NewRow();
                newRow["#"] = dt.Rows.Count;
                if (dt.Columns.Contains("item")) newRow["item"] = "NuovoElemento";
                if (dt.Columns.Contains("qty")) newRow["qty"] = "1";
                if (dt.Columns.Contains("value")) newRow["value"] = "0";
                dt.Rows.Add(newRow);
            }
        }

        private void BtnRemoveGridRow_Click(object sender, EventArgs e)
        {
            if (dgvSmartGrid.CurrentRow != null && dgvSmartGrid.DataSource is DataTable dt)
            {
                int rowIdx = dgvSmartGrid.CurrentRow.Index;
                if (rowIdx >= 0 && rowIdx < dt.Rows.Count)
                {
                    dt.Rows.RemoveAt(rowIdx);
                    for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["#"] = i;
                }
            }
        }

        private void BtnApplySmart_Click(object sender, EventArgs e)
        {
            if (cbSideCategory.SelectedItem == null || cbSideItem.SelectedItem == null || cbSideProperty.SelectedItem == null)
            {
                MessageBox.Show("Seleziona categoria, elemento e parametro prima di applicare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sheetName = cbSideCategory.SelectedItem.ToString();
            string selectedId = cbSideItem.SelectedItem.ToString();
            string propName = cbSideProperty.SelectedItem.ToString();

            JArray sheets = (JArray)currentCdbJson["sheets"];
            foreach (JObject sheet in sheets)
            {
                if (sheet["name"]?.ToString() == sheetName && sheet["lines"] is JArray lines)
                {
                    foreach (JObject line in lines)
                    {
                        string lineId = line["id"]?.ToString() ?? line["name"]?.ToString() ?? "";
                        if (lineId == selectedId)
                        {
                            if (dgvSmartGrid.Visible && dgvSmartGrid.DataSource is DataTable dt)
                            {
                                var originalToken = line[propName];

                                if (originalToken is JArray)
                                {
                                    JArray newArr = new JArray();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        JObject obj = new JObject();
                                        foreach (DataColumn col in dt.Columns)
                                        {
                                            if (col.ColumnName == "#") continue;
                                            string val = row[col.ColumnName]?.ToString()?.Trim() ?? "";
                                            if (!string.IsNullOrEmpty(val))
                                            {
                                                if (int.TryParse(val, out int iVal)) obj[col.ColumnName] = iVal;
                                                else if (double.TryParse(val.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dVal)) obj[col.ColumnName] = dVal;
                                                else obj[col.ColumnName] = val;
                                            }
                                        }
                                        if (obj.Count > 0) newArr.Add(obj);
                                    }
                                    line[propName] = newArr;
                                }
                                else if (originalToken is JObject)
                                {
                                    JObject newObj = new JObject();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        string key = row["item"]?.ToString()?.Trim();
                                        string val = row["value"]?.ToString()?.Trim();

                                        if (!string.IsNullOrEmpty(key))
                                        {
                                            if (int.TryParse(val, out int iVal)) newObj[key] = iVal;
                                            else if (double.TryParse(val?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dVal)) newObj[key] = dVal;
                                            else newObj[key] = val ?? "";
                                        }
                                    }
                                    line[propName] = newObj;
                                }

                                MessageBox.Show($"Dati salvati con successo per '{selectedId}'!", "Smart Editor Tabellare", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                string textVal = txtSmartValue.Text;
                                try
                                {
                                    line[propName] = JToken.Parse(textVal);
                                }
                                catch
                                {
                                    line[propName] = textVal;
                                }
                                MessageBox.Show($"Valore aggiornato con successo per '{selectedId}'!", "Smart Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            if (dgvData.Tag is JObject currentSheet && currentSheet["name"]?.ToString() == sheetName)
                            {
                                DisplaySheetData(currentSheet);
                            }
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateSidePanelTheme()
        {
            if (sideGuidedPanel == null) return;
            if (isDarkMode)
            {
                sideGuidedPanel.BackColor = Color.FromArgb(37, 37, 38);
                foreach (Control c in sideGuidedPanel.Controls)
                {
                    if (c is Label l) l.ForeColor = Color.Gainsboro;
                    if (c is TextBox t) { t.BackColor = Color.FromArgb(50, 50, 52); t.ForeColor = Color.White; }
                    if (c is ComboBox cb) { cb.BackColor = Color.FromArgb(50, 50, 52); cb.ForeColor = Color.White; }
                }

                dgvSmartGrid.BackgroundColor = Color.FromArgb(30, 30, 30);
                dgvSmartGrid.DefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
                dgvSmartGrid.DefaultCellStyle.ForeColor = Color.Gainsboro;
                dgvSmartGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
                dgvSmartGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                dgvSmartGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
                dgvSmartGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvSmartGrid.GridColor = Color.FromArgb(60, 60, 60);

                if (lblSideWarning != null) lblSideWarning.ForeColor = Color.Gold;
            }
            else
            {
                sideGuidedPanel.BackColor = Color.FromArgb(245, 245, 245);
                foreach (Control c in sideGuidedPanel.Controls)
                {
                    if (c is Label l) l.ForeColor = Color.Black;
                    if (c is TextBox t) { t.BackColor = Color.White; t.ForeColor = Color.Black; }
                    if (c is ComboBox cb) { cb.BackColor = Color.White; cb.ForeColor = Color.Black; }
                }

                dgvSmartGrid.BackgroundColor = Color.White;
                dgvSmartGrid.DefaultCellStyle.BackColor = Color.White;
                dgvSmartGrid.DefaultCellStyle.ForeColor = Color.Black;
                dgvSmartGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
                dgvSmartGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
                dgvSmartGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
                dgvSmartGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dgvSmartGrid.GridColor = Color.LightGray;

                if (lblSideWarning != null) lblSideWarning.ForeColor = Color.FromArgb(160, 100, 0);
            }
        }
    }
}