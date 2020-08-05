using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace PlaylistRandomizer
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Музыкальные файлы
        /// </summary>
        private List<string> Songs { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            TbPath.Text = GetPreviousDirectory();
        }

        /// <summary>
        /// Получение предыдущего выбранного расположения директории с музыкой
        /// </summary>
        /// <returns></returns>
        private string GetPreviousDirectory()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DirectoryInfo di = Directory.CreateDirectory(appDataPath + "\\PlaylistRandomizer");
            string path = "";
            StreamReader sr;
            try
            {
                sr = new StreamReader(System.IO.Path.Combine(di.FullName, "path.txt"));
                path = sr.ReadLine();
                sr.Close();
            }
            catch
            {
                //Если первый запуск, то директория -- стандартная системная папка с музыкой пользователя
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(di.FullName, "path.txt"), false);
                sw.WriteLine(path);
                sw.Close();
            }
            return path;
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Выбрать папку"
        /// </summary>
        private void BtnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            string path = TbPath.Text;
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                SelectedPath = path
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TbPath.Text = path = fbd.SelectedPath;
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + 
                    "\\PlaylistRandomizer", "path.txt"), false);
                sw.WriteLine(path);
                sw.Close();
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Создать плейлист"
        /// </summary>
        private async void BtnMix_Click(object sender, RoutedEventArgs e)
        {
            LbResult.Items.Clear();
            BtnSave.Visibility = Visibility.Hidden;
            string path = TbPath.Text;

            if (!int.TryParse(TbAmount.Text, out int count))
            {
                System.Windows.MessageBox.Show("Указано неверное количество");
                return;
            }

            pb.Visibility = Visibility.Visible;
            Songs = await Randomizer.GetRandomizedMP3sAsync(path, count);
            pb.Visibility = Visibility.Hidden;

            if (Songs == null)
            {
                System.Windows.MessageBox.Show("В выбранной папке недостаточно mp3-файлов");
                return;
            }

            foreach (string song in Songs) LbResult.Items.Add(song.Replace(path, ""));
            BtnSave.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Сохранить этот плейлист"
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Songs == null) return;

            string playlist = ConfigurationManager.AppSettings["playlist"] + "\n";
            foreach (string song in Songs) playlist += "\n" + song;

            Stream stream;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "AIMP playlist (*.aimppl4)|*.aimppl4",
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
    }
}
