using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Steganography
{
    public partial class Steganography : Form
    {
        private Bitmap bmp = null;
        public const string EMPTY_IMAGE = @"Resources\empty-image.png";

        public Steganography()
        {
            InitializeComponent();
        }

        private async void embedButton_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)imagePictureBox.Image;

            string text = dataTextBox.Text;

            if (text.Equals(""))
            {
                MessageBox.Show("The text you want to hide can't be empty", "Warning");

                return;
            }

            if (encryptCheckBox.Checked)
            {
                if (passwordTextBox.Text.Length < 6)
                {
                    MessageBox.Show("Please enter a password with at least 6 characters", "Warning");

                    return;
                }
                else
                {
                    text = Crypto.EncryptStringAES(text, passwordTextBox.Text);
                }
            }

            await UpdateWorkOnProgessiveBar();
            bmp = LsbProposedHelper.EmbedText(text, bmp);

            DialogResult dialogResult = MessageBox.Show("Do you want to save the image ?", "Embed Text Successfully!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(dialogResult == DialogResult.Yes)
            {
                SaveImageAfterEmbed();
            }
            else
            {
                notesLabel.Text = "Notes: don't forget to save your new image.";
                notesLabel.ForeColor = Color.OrangeRed;
            }

            progressBar1.Value = 0;
        }

        private async void extractButton_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)imagePictureBox.Image;

            var vector = ReadVector(pathVectorTextBox.Text);
                    
            await UpdateWorkOnProgessiveBar();
            string extractedText = LsbProposedHelper.ExtractText(bmp, vector);

            if (encryptCheckBox.Checked)
            {
                try
                {
                    extractedText = Crypto.DecryptStringAES(extractedText, passwordTextBox.Text);
                }
                catch
                {
                    MessageBox.Show("Wrong password", "Error");

                    return;
                }
            }

            dataTextBox.Text = extractedText;
            progressBar1.Value = 0;
        }

        private void imageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files (*.jpeg; *.png; *.bmp)|*.jpg; *.png; *.bmp";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                imagePictureBox.Image = Image.FromFile(open_dialog.FileName);
            }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveImageAfterEmbed();
        }

        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Text Files|*.txt";

            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(save_dialog.FileName, dataTextBox.Text);
            }
        }

        private void textToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Text Files|*.txt";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                dataTextBox.Text = File.ReadAllText(open_dialog.FileName);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imagePictureBox.Image != null)
            {
                imagePictureBox.Image.Dispose();
                imagePictureBox.Image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EMPTY_IMAGE));
            }

            passwordTextBox.Text = string.Empty;
            dataTextBox.Text = string.Empty;

            encryptCheckBox.Checked = false;
            pathVectorTextBox.Text = string.Empty;
            LsbProposedHelper.ResetVector();

            notesLabel.Text = "Notes:";
            notesLabel.ForeColor = Color.Black;

            progressBar1.Value = 0;
        }

        private void SaveImageAfterEmbed()
        {
            SaveFileDialog save_dialog = new SaveFileDialog
            {
                Filter = "Png Image|*.png|Bitmap Image|*.bmp"
            };

            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                switch (save_dialog.FilterIndex)
                {
                    case 0:
                        {
                            bmp.Save(save_dialog.FileName, ImageFormat.Png);
                            SaveVector(save_dialog);
                        }
                        break;
                    case 1:
                        {
                            bmp.Save(save_dialog.FileName, ImageFormat.Png);
                            SaveVector(save_dialog);
                        }
                        break;
                    case 2:
                        {
                            bmp.Save(save_dialog.FileName, ImageFormat.Bmp);
                            SaveVector(save_dialog);
                        }
                        break;
                }

                LsbProposedHelper.ResetVector();
                notesLabel.Text = "Notes:";
                notesLabel.ForeColor = Color.Black;
            }
        }

        private static void SaveVector(SaveFileDialog save_dialog)
        {
            string path = Path.GetDirectoryName(save_dialog.FileName);
            string fname = Path.GetFileNameWithoutExtension(save_dialog.FileName);
            var vector = LsbProposedHelper.Vector;

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, $"vector-{fname}.txt")))
            {
                foreach (int line in vector)
                    outputFile.WriteLine(line);
            }
        }

        private static List<int> ReadVector(string path)
        {
            try
            {
                
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    var vector = new List<int>();
                    using (StreamReader sr = new StreamReader(path))
                    {
                        while (sr.Peek() >= 0)
                        {
                            vector.Add(int.Parse(sr.ReadLine()));
                        }
                    }

                    return vector;
                }

                return new List<int>();
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
                return null;
            }
        }

        public void DoWork(IProgress<int> progress)
        {
            for (int j = 0; j < 1000; j++)
            {
                if (progress != null)
                    progress.Report((j + 1) * 100 / 1000);
            }
        }

        private async Task UpdateWorkOnProgessiveBar()
        {
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;

            var progress = new Progress<int>(v =>
            {
                progressBar1.Value = v;
                if (v % 10 == 0)
                {
                    //cú lừa =)))
                    Thread.Sleep(25);
                }
            });
            
            await Task.Run(() => DoWork(progress));
        }

        private void importVectorButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Text Files|*.txt";

            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                pathVectorTextBox.Text = Path.GetFullPath(open_dialog.FileName);
            }
        }
    }
}