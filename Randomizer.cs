using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PlaylistRandomizer
{
    public static class Randomizer
    {
        private static readonly Random RNG = new Random();
        /// <summary>
        /// Музыкальные файлы
        /// </summary>
        public static List<string> Songs { get; set; }

        /// <summary>
        /// Выбирает заданное количество рандомных файлов из заданной директории
        /// </summary>
        /// <param name="defaultPath">Директория для поиска</param>
        /// <param name="count">Количество требуемых файлов</param>
        public static void GetRandomizedSongs(string defaultPath, int count)
        {
            Songs = null;
            //Проверяем, достаточно ли в заданной директории mp3-файлов
            DirectoryInfo di = new DirectoryInfo(defaultPath);
            if (di.GetFiles("*.mp3", SearchOption.AllDirectories).Length < count) return;

            Songs = new List<string>();
            while (Songs.Count < count)
            {
                string song = null;
                while (song == null) song = GetSong(defaultPath);

                //Если данная песня уже есть в плейлисте -- пропускаем
                if (Songs.Contains(song)) continue;
                else Songs.Add(song);
            }
        }

        /// <summary>
        /// Заменяет выбранные песни в плейлисте
        /// </summary>
        /// <param name="defaultPath">Директория для поиска</param>
        /// <param name="songsForReplace">Список песен, которые нужно заменить</param>
        /// <param name="isDelete">Если true, выбранные песни удаляются из плейлиста без замены новыми</param>
        public static void ReplaceSongs(string defaultPath, List<string> songsForReplace, bool isDelete)
        {
            int count = Songs.Count;
            foreach (string song in songsForReplace) Songs.Remove(song);
            if (!isDelete)
            {
                while (Songs.Count < count)
                {
                    string song = null;
                    while (song == null) song = GetSong(defaultPath);

                    //Если данная песня уже есть в плейлисте, либо она удалялась -- пропускаем
                    if (Songs.Contains(song) || songsForReplace.Contains(song)) continue;
                    else Songs.Add(song);
                }
            }
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
