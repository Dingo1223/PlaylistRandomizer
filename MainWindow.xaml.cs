using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using swf = System.Windows.Forms;

namespace PlaylistRandomizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TbPath.Text = GetPreviousDirectory();
        }

        /// <summary>
        /// Получение предыдущего выбранного расположения директории с музыкой
        /// </summary>
        /// <returns>Предыдущее выбранное расположение (если нет -- стандартная системная папка с музыкой пользователя)</returns>
        private string GetPreviousDirectory()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DirectoryInfo di = Directory.CreateDirectory(appDataPath + "\\PlaylistRandomizer");
            StreamReader sr;
            string path;
            try
            {
                sr = new StreamReader(System.IO.Path.Combine(di.FullName, "path.txt"));
                path = sr.ReadLine();
                sr.Close();
            }
            catch
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(di.FullName, "path.txt"), false);
                sw.WriteLine(path);
                sw.Close();
            }
            return path;
        }

        /// <summary>
        /// Печать песен, содержащихся в сгенерированном плейлисте
        /// </summary>
        /// <param name="path">Расположение директории с музыкой</param>
        private void PrintSongsFromList(string path)
        {
            LbResult.Items.Clear();

            foreach (string song in Randomizer.Songs)
            {
                var cb = new CheckBox()
                {
                    Content = song.Replace(path, ""),
                    Tag = song
                };
                cb.Checked += Cb_CheckedChanged;

                LbResult.Items.Add(cb);
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Выбрать папку"
        /// </summary>
        private void BtnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            string path = TbPath.Text;
            swf.FolderBrowserDialog fbd = new swf.FolderBrowserDialog
            {
                SelectedPath = path
            };
            if (fbd.ShowDialog() == swf.DialogResult.OK)
            {
                TbPath.Text = path = fbd.SelectedPath;
                StreamWriter sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + 
                    "\\PlaylistRandomizer", "path.txt"), false);
                sw.WriteLine(path);
                sw.Close();

                Randomizer.Songs = null;
                LbResult.Items.Clear();
                BtnDeleteSelected.Visibility = BtnReplaceSelected.Visibility = BtnSave.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Создать плейлист"
        /// </summary>
        private async void BtnMix_Click(object sender, RoutedEventArgs e)
        {
            BtnSave.Visibility = Visibility.Hidden;
            string path = TbPath.Text;

            if (!int.TryParse(TbAmount.Text, out int count))
            {
                MessageBox.Show("Указано неверное количество");
                return;
            }

            pb.Visibility = Visibility.Visible;
            await Task.Run(() => Randomizer.GetRandomizedSongs(path, count));
            pb.Visibility = Visibility.Hidden;

            if (Randomizer.Songs == null)
            {
                MessageBox.Show("В выбранной папке недостаточно mp3-файлов");
                return;
            }

            PrintSongsFromList(path);
            BtnSave.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик выбора позиции из списка
        /// </summary>
        private void Cb_CheckedChanged(object sender, EventArgs e)
        {
            BtnDeleteSelected.Visibility = Visibility.Visible;
            BtnReplaceSelected.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Сохранить этот плейлист"
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Randomizer.Songs == null) return;

            string playlist = ConfigurationManager.AppSettings["playlist"] + "\n";
            foreach (string song in Randomizer.Songs) playlist += "\n" + song;

            Stream stream;
            swf.SaveFileDialog saveFileDialog = new swf.SaveFileDialog
            {
                Filter = "AIMP playlist (*.aimppl4)|*.aimppl4",
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == swf.DialogResult.OK)
            {
                if ((stream = saveFileDialog.OpenFile()) != null)
                {
                    StreamWriter sw = new StreamWriter(stream, Encoding.BigEndianUnicode);
                    sw.Write(playlist + "\n");
                    sw.Close();
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Удалить/Заменить выбранное"
        /// </summary>
        private void BtnReplaceSelected_Click(object sender, RoutedEventArgs e)
        {
            List<string> selectedSongs = new List<string>();
            foreach (CheckBox cb in LbResult.Items)
            {
                if (cb.IsChecked == true) selectedSongs.Add(cb.Tag as string);
            }

            string path = TbPath.Text;
            Randomizer.ReplaceSongs(path, selectedSongs, sender == BtnDeleteSelected);
            PrintSongsFromList(TbPath.Text);
        }
    }
}
