using Microsoft.Win32;
using System.IO;

namespace PlaylistRandomizerCore
{
    public class Randomizer
    {
        private readonly Random RNG = new();

        public static string GetDefaultDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        /// <summary>
        /// Выбор директории для составления плейлиста
        /// </summary>
        public static string SelectDirectory()
        {
            OpenFolderDialog ofd = new()
            {
                Multiselect = false
            };
            if (ofd.ShowDialog() == true)
            {
                return ofd.FolderName;
            }
            else return GetDefaultDirectory();
        }

        /// <summary>
        /// Сохранение плейлиста в файл .aimppl4
        /// </summary>
        /// <param name="songs">Список песен в плейлисте</param>
        public static void SavePlaylist(List<PlaylistElement> songs)
        {
            string playlist = "#-----SUMMARY-----#\n" +
                $"ID={{{Guid.NewGuid()}}}\n" +
                "Name=Randomized\n" +
                "NameIsAutoSet=0\n" +
                "PlaybackCursor=0\n" +
                "PlaybackCursorAutoBookmark=0\n\n" +
                "#-----CONTENT-----#\n";
            foreach (PlaylistElement song in songs) playlist += "\n" + song.FullPath;

            SaveFileDialog dialog = new()
            {
                DefaultExt = ".aimppl4",
                Filter = "AIMP playlist (*.aimppl4)|*.aimppl4",
            };

            if (dialog.ShowDialog() == true)
            {
                StreamWriter sw = new(dialog.FileName, false, System.Text.Encoding.BigEndianUnicode);
                sw.Write(playlist + "\n");
                sw.Close();
            }
        }

        /// <summary>
        /// Выбирает заданное количество рандомных файлов из заданной директории
        /// </summary>
        /// <param name="path">Директория для поиска</param>
        /// <param name="count">Количество требуемых файлов</param>
        /// <returns>Составленный плейлист</returns>
        public List<PlaylistElement>? GetRandomizedSongs(string path,
                                                         int count)
        {
            //Проверяем, достаточно ли в заданной директории mp3-файлов
            DirectoryInfo di = new(path);
            if (di.GetFiles("*.mp3", SearchOption.AllDirectories).Length < count) return null; // TODO : moar

            List<PlaylistElement> songs = [];
            while (songs.Count < count)
            {
                string? song = null;
                while (song == null) song = GetSong(path);

                //Если данная песня уже есть в плейлисте -- пропускаем
                if (songs.Exists(s => s.FullPath == song)) continue;
                else songs.Add(new PlaylistElement(song, path));
            }

            return songs;
        }


        /// <summary>
        /// Заменяет выбранные песни в плейлисте
        /// </summary>
        /// <param name="path">Директория для поиска</param>
        /// <param name="songsForReplace">Список песен, которые нужно заменить</param>
        /// <param name="isDelete">Если true, выбранные песни удаляются из плейлиста без замены новыми</param>
        public List<PlaylistElement> ReplaceSongs(string path,
                                                  List<PlaylistElement> playlist,
                                                  List<PlaylistElement> songsForReplace,
                                                  bool isDelete)
        {
            int count = playlist.Count;
            foreach (PlaylistElement song in songsForReplace) playlist.Remove(song);

            if (!isDelete)
            {
                while (playlist.Count < count)
                {
                    string? song = null;
                    while (song == null) song = GetSong(path);

                    //Если данная песня уже есть в плейлисте, либо она удалялась -- пропускаем
                    if (playlist.Exists(s => s.FullPath == song) ||
                        songsForReplace.Exists(s => s.FullPath == song)) continue;
                    else playlist.Add(new PlaylistElement(song, path));
                }
            }

            return playlist;
        }

        /// <summary>
        /// Выбирает случайный файл из данной директории
        /// </summary>
        /// <param name="path">Директория для поиска</param>
        /// <returns>Случайный файл (либо null, если в папке нет файлов в нужном формате)</returns>
        private string? GetSong(string path)
        {
            DirectoryInfo di = new(path);
            DirectoryInfo[] dirs = di.GetDirectories();
            FileInfo[] mp3s = di.GetFiles("*.mp3"); // TODO : moar

            //Если там, куда нас завело, нет ни других папок, ни песен -- выходим
            if (dirs.Length == 0 && mp3s.Length == 0) return null;

            if (dirs.Length != 0 && (mp3s.Length == 0 || Convert.ToBoolean(RNG.Next(0, 2))))
            {
                return GetSong(dirs[RNG.Next(dirs.Length)].FullName);
            }
            else
            {
                return mp3s[RNG.Next(mp3s.Length)].FullName;
            }
        }
    }
}
