#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SpaceCraft_Mod_Tool
{
    public partial class LauncherForm : Form
    {
        private TextBox txtGamePath;
        private TextBox txtOutputPath;
        private CheckBox chkDefaultOutputPath;
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
        }

        private void InitializeLauncherUI()
        {
            this.Text = "SpaceCraft Mod Tool - Launcher & Setup";
            this.Size = new System.Drawing.Size(650, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // --- SEZIONE GIOCO ---
            Label lblGame = new Label { Text = "Cartella di installazione SpaceCraft:", Left = 20, Top = 20, Width = 300 };
            txtGamePath = new TextBox { Left = 20, Top = 45, Width = 500, Height = 25 };
            btnBrowseGame = new Button { Text = "Sfoglia...", Left = 530, Top = 44, Width = 80, Height = 27 };
            btnBrowseGame.Click += BtnBrowseGame_Click;

            // --- SEZIONE CARTELLA MOD / OUTPUT ---
            chkDefaultOutputPath = new CheckBox { Text = "Usa cartella di default nella directory di gioco", Left = 20, Top = 85, Width = 400, Checked = true };
            chkDefaultOutputPath.CheckedChanged += ChkDefaultOutputPath_CheckedChanged;

            Label lblOutput = new Label { Text = "Cartella di output per il Modding / Estrazione:", Left = 20, Top = 115, Width = 350 };
            txtOutputPath = new TextBox { Left = 20, Top = 140, Width = 500, Height = 25, Enabled = false };
            btnBrowseOutput = new Button { Text = "Sfoglia...", Left = 530, Top = 139, Width = 80, Height = 27 };
            btnBrowseOutput.Enabled = false;
            btnBrowseOutput.Click += BtnBrowseOutput_Click;

            // --- PULSANTI AZIONE ---
            btnExtractOrDetect = new Button { Text = "1. Estrai / Rileva data.cdb", Left = 20, Top = 185, Width = 280, Height = 40 };
            btnExtractOrDetect.Click += BtnExtractOrDetect_Click;

            btnOpenExistingMod = new Button { Text = "Riapri Mod Esistente (res1.pak / res2.pak)", Left = 315, Top = 185, Width = 295, Height = 40 };
            btnOpenExistingMod.Click += BtnOpenExistingMod_Click;

            btnLaunchEditor = new Button { Text = "2. Apri Editor CDB (Stile HIDE)", Left = 20, Top = 235, Width = 590, Height = 45 };
            btnLaunchEditor.Font = new System.Drawing.Font(btnLaunchEditor.Font, System.Drawing.FontStyle.Bold);
            btnLaunchEditor.Click += BtnLaunchEditor_Click;

            btnCreateModPak = new Button { Text = "3. Compila / Crea Mod (res2.pak)", Left = 20, Top = 290, Width = 590, Height = 45 };
            btnCreateModPak.Font = new System.Drawing.Font(btnCreateModPak.Font, System.Drawing.FontStyle.Bold);
            btnCreateModPak.BackColor = System.Drawing.Color.LightGreen;
            btnCreateModPak.Click += BtnCreateModPak_Click;

            // --- BARRA DI STATO ---
            lblStatus = new Label { Text = "Stato: In attesa di configurazione...", Left = 20, Top = 355, Width = 590, Height = 60 };
            lblStatus.ForeColor = System.Drawing.Color.DarkBlue;

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
                MessageBox.Show("Seleziona prima una cartella di gioco valida.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string resPakPath = Path.Combine(gamePath, "res.pak");
            if (!File.Exists(resPakPath))
            {
                MessageBox.Show("File 'res.pak' non trovato nella cartella selezionata!", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string outputPath = string.IsNullOrEmpty(txtOutputPath.Text) ? Path.Combine(gamePath, "ModdingData") : txtOutputPath.Text;
            Directory.CreateDirectory(outputPath);
            string destinationFile = Path.Combine(outputPath, "data.cdb");

            if (File.Exists(destinationFile))
            {
                var result = MessageBox.Show(
                    $"Un file 'data.cdb' è già presente nella cartella di destinazione:\n{destinationFile}\n\n" +
                    "Se continui, il file esistente verrà sovrascritto con quello originale del gioco.\n" +
                    "Vuoi sovrascriverlo? (Verrà creata una copia di backup data.cdb.bak)",
                    "Conferma Sovrascrittura",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    lblStatus.Text = "Stato: Operazione annullata dall'utente.";
                    return;
                }

                string backupFile = Path.Combine(outputPath, "data.cdb.bak");
                File.Copy(destinationFile, backupFile, overwrite: true);
            }

            try
            {
                lblStatus.Text = "Stato: Analisi dell'indice del pacchetto res.pak (16 GB) in corso...";
                Application.DoEvents();

                HeapsPakReader reader = new HeapsPakReader(resPakPath);

                lblStatus.Text = "Stato: Ricerca intelligente del file data.cdb nell'archivio...";
                Application.DoEvents();

                var cdbEntry = reader.FindFile("data.cdb");
                if (cdbEntry == null)
                {
                    MessageBox.Show("Impossibile trovare 'data.cdb' all'interno del pacchetto res.pak.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Stato: Errore - data.cdb non trovato.";
                    return;
                }

                lblStatus.Text = $"Stato: Estrazione di {cdbEntry.Path} in corso...";
                Application.DoEvents();

                byte[] cdbData = reader.ExtractFile(cdbEntry);
                File.WriteAllBytes(destinationFile, cdbData);

                lblStatus.Text = $"Stato: Estrazione completata con successo in {destinationFile}";
                MessageBox.Show($"Database estratto con successo!\nSalvato in: {destinationFile}", "Completato", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'estrazione:\n{ex.Message}", "Errore Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Stato: Errore durante l'estrazione.";
            }
        }

        private void BtnOpenExistingMod_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Pacchetti Mod (*.pak)|*.pak|Tutti i file (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        lblStatus.Text = "Stato: Lettura pacchetto mod esistente...";
                        Application.DoEvents();

                        HeapsPakReader reader = new HeapsPakReader(ofd.FileName);
                        var cdbEntry = reader.FindFile("data.cdb");

                        if (cdbEntry != null)
                        {
                            string outputPath = string.IsNullOrEmpty(txtOutputPath.Text) ? Path.GetDirectoryName(ofd.FileName) : txtOutputPath.Text;
                            Directory.CreateDirectory(outputPath);
                            string destinationFile = Path.Combine(outputPath, "data.cdb");

                            if (File.Exists(destinationFile))
                            {
                                var result = MessageBox.Show(
                                    $"Un file 'data.cdb' è già presente nella cartella di destinazione:\n{destinationFile}\n\n" +
                                    "Vuoi sovrascriverlo con il database contenuto nella mod selezionata?\n" +
                                    "(Verrà creata una copia di backup data.cdb.bak)",
                                    "Conferma Caricamento Mod",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question
                                );

                                if (result == DialogResult.No)
                                {
                                    lblStatus.Text = "Stato: Caricamento mod annullato.";
                                    return;
                                }

                                string backupFile = Path.Combine(outputPath, "data.cdb.bak");
                                File.Copy(destinationFile, backupFile, overwrite: true);
                            }

                            byte[] cdbData = reader.ExtractFile(cdbEntry);
                            File.WriteAllBytes(destinationFile, cdbData);

                            lblStatus.Text = $"Stato: Mod riaperta con successo da {ofd.FileName}";
                            MessageBox.Show($"Mod caricata ed estratta per la modifica!\nFile pronto in: {destinationFile}", "Mod Caricata", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Il pacchetto selezionato non contiene alcun file data.cdb.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Errore nell'apertura della mod:\n{ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Nessuna cartella di lavoro valida selezionata.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cdbFile = Path.Combine(outputPath, "data.cdb");
            if (!File.Exists(cdbFile))
            {
                MessageBox.Show($"Nessun file 'data.cdb' trovato in:\n{outputPath}\nModifica e salva prima il database dall'editor!", "File Mancante", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lblStatus.Text = "Stato: Preparazione file per il pacchetto Mod...";
                Application.DoEvents();

                Dictionary<string, byte[]> filesToPack = new Dictionary<string, byte[]>();
                filesToPack["data.cdb"] = File.ReadAllBytes(cdbFile);

                // Destinazione preferenziale: direttamente nella cartella del gioco come res2.pak
                string gamePath = txtGamePath.Text;
                string targetPakPath = !string.IsNullOrEmpty(gamePath) && Directory.Exists(gamePath)
                    ? Path.Combine(gamePath, "res2.pak")
                    : Path.Combine(outputPath, "res2.pak");

                lblStatus.Text = $"Stato: Ricompattazione in corso -> {targetPakPath}";
                Application.DoEvents();

                HeapsPakWriter.CreatePak(filesToPack, targetPakPath);

                lblStatus.Text = $"Stato: Mod compilata con successo! Salvata in: {targetPakPath}";
                MessageBox.Show($"Mod ricompilata con successo!\nIl file 'res2.pak' è stato creato in:\n{targetPakPath}\n\nPuoi avviare SpaceCraft per testare le tue modifiche!", "Mod Pronta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la creazione del pacchetto mod:\n{ex.Message}", "Errore Critico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Stato: Errore durante la creazione del pacchetto mod.";
            }
        }
    }
}