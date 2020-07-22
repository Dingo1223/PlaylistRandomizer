using System;
using System.Collections.Generic;
using System.IO;

namespace PlaylistRandomizer
{
    public static class Randomizer
    {
        private static readonly Random RNG = new Random();

        /// <summary>
        /// Возвращает заданное количество рандомных mp3 из заданной директории
        /// </summary>
        /// <param name="defaultPath">Директория для поиска</param>
        /// <param name="count">Количество требуемых mp3-файлов</param>
        /// <returns>Список mp3 (либо null, если в директории не хватает mp3)</returns>
        public static List<string> GetRandomizedMP3s(string defaultPath, int count)
        {
            //Проверяем, достаточно ли в заданной директории mp3-файлов
            DirectoryInfo di = new DirectoryInfo(defaultPath);
            if (di.GetFiles("*.mp3", SearchOption.AllDirectories).Length < count) return null;

            List<string> songs = new List<string>();
            for (int i = 0; i < count; i++)
            {
                string song = null;
                while (song == null) song = GetSong(defaultPath);

                //Если данная песня уже есть в плейлисте -- пропускаем
                if (songs.Contains(song))
                {
                    i--;
                    continue;
                }
                else songs.Add(song);
            }

            return songs;
        }

        /// <summary>
        /// Выбирает случайный mp3 файл из данной директории
        /// </summary>
        /// <param name="defaultPath">Директория для поиска</param>
        /// <returns>Случайный mp3-файл (либо null, если в папке нет mp3-файлов)</returns>
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
