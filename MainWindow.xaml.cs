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
        /// Директория с музыкальными файлами
        /// </summary>
        private string Path { get; set; }

        /// <summary>
        /// Музыкальные файлы
        /// </summary>
        private List<string> Songs { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //Получение предыдущего выбранного расположения директории с музыкой
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DirectoryInfo di = Directory.CreateDirectory(appDataPath + "\\PlaylistRandomizer");
            StreamReader sr;
            try
            {
                sr = new StreamReader(System.IO.Path.Combine(di.FullName, "path.txt"));
                TbPath.Text =  Path = sr.ReadLine();
                sr.Close();
            }
            catch
            {
                //Если первый запуск, то директория -- стандартная системная папка с музыкой пользователя
                TbPath.Text = Path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(di.FullName, "path.txt"), false);
                sw.WriteLine(Path);
                sw.Close();
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Выбрать папку"
        /// </summary>
        private void BtnSelectPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                SelectedPath = Path
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TbPath.Text = Path = fbd.SelectedPath;
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + 
                    "\\PlaylistRandomizer", "path.txt"), false);
                sw.WriteLine(Path);
                sw.Close();
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Создать плейлист"
        /// </summary>
        private void BtnMix_Click(object sender, RoutedEventArgs e)
        {
            LbResult.Items.Clear();
            BtnSave.Visibility = Visibility.Hidden;

            if (!int.TryParse(TbAmount.Text, out int count))
            {
                System.Windows.MessageBox.Show("Указано неверное количество");
                return;
            }

            Songs = Randomizer.GetRandomizedMP3s(Path, count);
            if (Songs == null)
            {
                System.Windows.MessageBox.Show("В выбранной папке недостаточно mp3-файлов");
                return;
            }

            foreach (string song in Songs) LbResult.Items.Add(song);
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
