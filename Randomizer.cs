using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using swf = System.Windows.Forms;

namespace PlaylistRandomizer
{
    public static class Randomizer
    {
        private static readonly Random RNG = new Random();

        /// <summary>
        /// Получение предыдущего выбранного расположения директории с музыкой
        /// </summary>
        /// <returns>Предыдущее выбранное расположение (если нет -- стандартная системная папка с музыкой пользователя)</returns>
        public static string GetPreviousDirectory()
        {
            DirectoryInfo di = Directory.CreateDirectory(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData) + "\\PlaylistRandomizer");
            string path;
            try
            {
                StreamReader sr = new StreamReader(System.IO.Path.Combine(di.FullName, "path.txt"));
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
        /// Выбор директории для составления плейлиста
        /// </summary>
        /// <param name="path">Предыдущее выбранное расположение</param>
        /// <returns>Директория для составления плейлиста</returns>
        public static string SelectDirectory(string path)
        {
            using (var fbd = new swf.FolderBrowserDialog { SelectedPath = path })
            {
                if (fbd.ShowDialog() == swf.DialogResult.OK)
                {
                    path = fbd.SelectedPath;
                    StreamWriter sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                        "\\PlaylistRandomizer", "path.txt"), false);
                    sw.WriteLine(path);
                    sw.Close();
                }
            }
            return path;
        }

        /// <summary>
        /// Сохранение плейлиста в файл .aimppl4
        /// </summary>
        /// <param name="songs">Список песен в плейлисте</param>
        public static void SavePlaylist(List<PlaylistElement> songs)
        {
            string playlist = ConfigurationManager.AppSettings["playlist"] + "\n";
            foreach (PlaylistElement song in songs) playlist += "\n" + song.FullPath;

            using (swf.SaveFileDialog saveFileDialog = new swf.SaveFileDialog
            {
                Filter = "AIMP playlist (*.aimppl4)|*.aimppl4",
                RestoreDirectory = true
            })
            {
                if (saveFileDialog.ShowDialog() == swf.DialogResult.OK)
                {
                    Stream stream;
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

        /// <summary>
        /// Выбирает заданное количество рандомных файлов из заданной директории
        /// </summary>
        /// <param name="defaultPath">Директория для поиска</param>
        /// <param name="count">Количество требуемых файлов</param>
        /// <returns>Составленный плейлист</returns>
        public static List<PlaylistElement> GetRandomizedSongs(string defaultPath, int count)
        {
            //Проверяем, достаточно ли в заданной директории mp3-файлов
            DirectoryInfo di = new DirectoryInfo(defaultPath);
            if (di.GetFiles("*.mp3", SearchOption.AllDirectories).Length < count) return null;

            List<PlaylistElement> songs = new List<PlaylistElement>();
            while (songs.Count < count)
            {
                string song = null;
                while (song == null) song = GetSong(defaultPath);

                //Если данная песня уже есть в плейлисте -- пропускаем
                if (songs.Exists(s => s.FullPath == song)) continue;
                else songs.Add(new PlaylistElement(song, song.Replace(defaultPath, "")));
            }

            return songs;
        }

        /// <summary>
        /// Заменяет выбранные песни в плейлисте
        /// </summary>
        /// <param name="defaultPath">Директория для поиска</param>
        /// <param name="songsForReplace">Список песен, которые нужно заменить</param>
        /// <param name="isDelete">Если true, выбранные песни удаляются из плейлиста без замены новыми</param>
        public static List<PlaylistElement> ReplaceSongs(string defaultPath, List<PlaylistElement> playlist,
            List<PlaylistElement> songsForReplace, bool isDelete)
        {
            int count = playlist.Count;
            foreach (PlaylistElement song in songsForReplace) playlist.Remove(song);

            if (!isDelete)
            {
                while (playlist.Count < count)
                {
                    string song = null;
                    while (song == null) song = GetSong(defaultPath);

                    //Если данная песня уже есть в плейлисте, либо она удалялась -- пропускаем
                    if (playlist.Exists(s => s.FullPath == song) || 
                        songsForReplace.Exists(s => s.FullPath == song)) continue;
                    else playlist.Add(new PlaylistElement(song, song.Replace(defaultPath, "")));
                }
            }

            return playlist;
        }

        /// <summary>
        /// Выбирает случайный файл из данной директории
        /// </summary>
        /// <param name="defaultPath">Директория для поиска</param>
        /// <returns>Случайный файл (либо null, если в папке нет файлов в нужном формате)</returns>
        private static string GetSong(string defaultPath)
        {
            DirectoryInfo di = new DirectoryInfo(defaultPath);
            DirectoryInfo[] dirs = di.GetDirectories();
            FileInfo[] mp3s = di.GetFiles("*.mp3");

            //Если там, куда нас завело, нет ни других папок, ни песен -- выходим
            if (dirs.Length == 0 && mp3s.Length == 0) return null;

            if (dirs.Length != 0)
            {
                //Если в исходной директории есть другие папки, а также mp3-файлы
                if (mp3s.Length != 0)
                {
                    //Выбираем, будем ли мы заходить в другие папки
                    if (Convert.ToBoolean(RNG.Next(0, 2)))
                    {
                        return GetSong(dirs[RNG.Next(dirs.Length)].FullName);
                    }
                    //Если не будем -- выбираем из тех mp3, что есть в исходной директории
                    else return mp3s[RNG.Next(mp3s.Length)].FullName;
                }
                //Если других mp3 нет -- идём в подпапки
                else return GetSong(dirs[RNG.Next(dirs.Length)].FullName);
            }
            //Если в исходной директории нет подпапок, но есть mp3 -- выбираем из них
            else return mp3s[RNG.Next(mp3s.Length)].FullName;
        }
    }
}
