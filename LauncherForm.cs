#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SpaceCraft_Mod_Tool
{
    public partial class LauncherForm : Form
    {
        private Label lblGame;
        private TextBox txtGamePath;
        private TextBox txtOutputPath;
        private CheckBox chkDefaultOutputPath;
        private Label lblOutput;
        private Button btnBrowseGame;
        private Button btnBrowseOutput;
        private Button btnExtractOrDetect;
        private Button btnOpenExistingMod;
        private Button btnCreateModPak;
        private Button btnLaunchEditor;
        private Label lblStatus;

        public LauncherForm()
        {
            InitializeLauncherUI();
            LanguageManager.OnLanguageChanged += UpdateLanguageUI;
            UpdateLanguageUI();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LanguageManager.OnLanguageChanged -= UpdateLanguageUI;
            base.OnFormClosed(e);
        }

        private void InitializeLauncherUI()
        {
            this.Size = new System.Drawing.Size(650, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            lblGame = new Label { Left = 20, Top = 20, Width = 300 };
            txtGamePath = new TextBox { Left = 20, Top = 45, Width = 500, Height = 25 };
            btnBrowseGame = new Button { Left = 530, Top = 44, Width = 80, Height = 27 };
            btnBrowseGame.Click += BtnBrowseGame_Click;

            chkDefaultOutputPath = new CheckBox { Left = 20, Top = 85, Width = 400, Checked = true };
            chkDefaultOutputPath.CheckedChanged += ChkDefaultOutputPath_CheckedChanged;

            lblOutput = new Label { Left = 20, Top = 115, Width = 350 };
            txtOutputPath = new TextBox { Left = 20, Top = 140, Width = 500, Height = 25, Enabled = false };
            btnBrowseOutput = new Button { Left = 530, Top = 139, Width = 80, Height = 27, Enabled = false };
            btnBrowseOutput.Click += BtnBrowseOutput_Click;

            btnExtractOrDetect = new Button { Left = 20, Top = 185, Width = 280, Height = 40 };
            btnExtractOrDetect.Click += BtnExtractOrDetect_Click;

            btnOpenExistingMod = new Button { Left = 315, Top = 185, Width = 295, Height = 40 };
            btnOpenExistingMod.Click += BtnOpenExistingMod_Click;

            btnLaunchEditor = new Button { Left = 20, Top = 235, Width = 590, Height = 45, Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold) };
            btnLaunchEditor.Click += BtnLaunchEditor_Click;

            btnCreateModPak = new Button { Left = 20, Top = 290, Width = 590, Height = 45, Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold), BackColor = System.Drawing.Color.LightGreen };
            btnCreateModPak.Click += BtnCreateModPak_Click;

            lblStatus = new Label { Left = 20, Top = 355, Width = 590, Height = 60, ForeColor = System.Drawing.Color.DarkBlue };

            this.Controls.Add(lblGame);
            this.Controls.Add(txtGamePath);
            this.Controls.Add(btnBrowseGame);
            this.Controls.Add(chkDefaultOutputPath);
            this.Controls.Add(lblOutput);
            this.Controls.Add(txtOutputPath);
            this.Controls.Add(btnBrowseOutput);
            this.Controls.Add(btnExtractOrDetect);
            this.Controls.Add(btnOpenExistingMod);
            this.Controls.Add(btnLaunchEditor);
            this.Controls.Add(btnCreateModPak);
            this.Controls.Add(lblStatus);
        }

        private void UpdateLanguageUI()
        {
            this.Text = LanguageManager.GetText("LauncherTitle");
            lblGame.Text = LanguageManager.GetText("LblGameFolder");
            btnBrowseGame.Text = LanguageManager.GetText("BtnBrowse");
            btnBrowseOutput.Text = LanguageManager.GetText("BtnBrowse");
            chkDefaultOutputPath.Text = LanguageManager.GetText("ChkDefaultOutput");
            lblOutput.Text = LanguageManager.GetText("LblOutputFolder");
            btnExtractOrDetect.Text = LanguageManager.GetText("BtnExtract");
            btnOpenExistingMod.Text = LanguageManager.GetText("BtnOpenExisting");
            btnLaunchEditor.Text = LanguageManager.GetText("BtnLaunchEditor");
            btnCreateModPak.Text = LanguageManager.GetText("BtnCreatePak");
            lblStatus.Text = LanguageManager.GetText("StatusWait");
        }

        private void BtnBrowseGame_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtGamePath.Text = fbd.SelectedPath;
                    if (chkDefaultOutputPath.Checked)
                    {
                        txtOutputPath.Text = Path.Combine(fbd.SelectedPath, "ModdingData");
                    }
                }
            }
        }

        private void ChkDefaultOutputPath_CheckedChanged(object sender, EventArgs e)
        {
            bool isCustom = !chkDefaultOutputPath.Checked;
            txtOutputPath.Enabled = isCustom;
            btnBrowseOutput.Enabled = isCustom;

            if (!isCustom && !string.IsNullOrEmpty(txtGamePath.Text))
            {
                txtOutputPath.Text = Path.Combine(txtGamePath.Text, "ModdingData");
            }
        }

        private void BtnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnExtractOrDetect_Click(object sender, EventArgs e)
        {
            string gamePath = txtGamePath.Text;
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
            {
                MessageBox.Show("Please select a valid game folder first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string resPakPath = Path.Combine(gamePath, "res.pak");
            if (!File.Exists(resPakPath))
            {
                MessageBox.Show("'res.pak' file not found in the selected folder!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string outputPath = string.IsNullOrEmpty(txtOutputPath.Text) ? Path.Combine(gamePath, "ModdingData") : txtOutputPath.Text;
            Directory.CreateDirectory(outputPath);
            string destinationFile = Path.Combine(outputPath, "data.cdb");

            if (File.Exists(destinationFile))
            {
                var result = MessageBox.Show(
                    $"A 'data.cdb' file already exists in:\n{destinationFile}\n\nDo you want to overwrite it?",
                    "Confirm Overwrite",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No) return;
                string backupFile = Path.Combine(outputPath, "data.cdb.bak");
                File.Copy(destinationFile, backupFile, overwrite: true);
            }

            try
            {
                lblStatus.Text = "Status: Extracting data.cdb...";
                Application.DoEvents();

                HeapsPakReader reader = new HeapsPakReader(resPakPath);
                var cdbEntry = reader.FindFile("data.cdb");

                if (cdbEntry == null)
                {
                    MessageBox.Show("Could not find 'data.cdb' inside res.pak.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                byte[] cdbData = reader.ExtractFile(cdbEntry);
                File.WriteAllBytes(destinationFile, cdbData);

                lblStatus.Text = $"Status: Extraction completed at {destinationFile}";
                MessageBox.Show($"Database extracted successfully!\nSaved to: {destinationFile}", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during extraction:\n{ex.Message}", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenExistingMod_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Mod Packages (*.pak)|*.pak|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        HeapsPakReader reader = new HeapsPakReader(ofd.FileName);
                        var cdbEntry = reader.FindFile("data.cdb");

                        if (cdbEntry != null)
                        {
                            string outputPath = string.IsNullOrEmpty(txtOutputPath.Text) ? Path.GetDirectoryName(ofd.FileName) : txtOutputPath.Text;
                            Directory.CreateDirectory(outputPath);
                            string destinationFile = Path.Combine(outputPath, "data.cdb");

                            byte[] cdbData = reader.ExtractFile(cdbEntry);
                            File.WriteAllBytes(destinationFile, cdbData);

                            MessageBox.Show($"Mod loaded and extracted for editing!\nFile ready at: {destinationFile}", "Mod Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening mod:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnLaunchEditor_Click(object sender, EventArgs e)
        {
            Form1 editorForm = new Form1();
            this.Hide();
            editorForm.ShowDialog();
            this.Show();
        }

        private void BtnCreateModPak_Click(object sender, EventArgs e)
        {
            string outputPath = txtOutputPath.Text;
            if (string.IsNullOrEmpty(outputPath) || !Directory.Exists(outputPath))
            {
                MessageBox.Show("No valid working folder selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cdbFile = Path.Combine(outputPath, "data.cdb");
            if (!File.Exists(cdbFile))
            {
                MessageBox.Show($"No 'data.cdb' file found in:\n{outputPath}", "Missing File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Dictionary<string, byte[]> filesToPack = new Dictionary<string, byte[]>();
                filesToPack["data.cdb"] = File.ReadAllBytes(cdbFile);

                string gamePath = txtGamePath.Text;
                string targetPakPath = !string.IsNullOrEmpty(gamePath) && Directory.Exists(gamePath)
                    ? Path.Combine(gamePath, "res2.pak")
                    : Path.Combine(outputPath, "res2.pak");

                HeapsPakWriter.CreatePak(filesToPack, targetPakPath);

                MessageBox.Show($"Mod recompiled successfully!\n'res2.pak' created at:\n{targetPakPath}", "Mod Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating mod package:\n{ex.Message}", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}