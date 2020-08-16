using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace PlaylistRandomizer
{
    class AppVM : INotifyPropertyChanged
    {
        public AppVM()
        {
            RandDirectory = Randomizer.GetPreviousDirectory();
            SongsCount = 50;
        }

        private string randDirectory;
        /// <summary>
        /// Директория для выбора файлов
        /// </summary>
        public string RandDirectory
        {
            get => randDirectory;
            set
            {
                randDirectory = value;
                OnPropertyChanged("RandDirectory");
            }
        }

        private int songsCount;
        /// <summary>
        /// Требуемое количество песен в плейлисте
        /// </summary>
        public int SongsCount
        {
            get => songsCount;
            set
            {
                songsCount = value;
                OnPropertyChanged("SongsCount");
            }
        }

        private List<PlaylistElement> playlist;
        /// <summary>
        /// Плейлист
        /// </summary>
        public List<PlaylistElement> Playlist
        {
            get => playlist;
            set
            {
                playlist = value;
                OnPropertyChanged("Playlist");
            }
        }

        private List<PlaylistElement> selectedSongs = new List<PlaylistElement>();
        /// <summary>
        /// Выбранные песни
        /// </summary>
        public List<PlaylistElement> SelectedSongs
        {
            get => selectedSongs;
            set
            {
                selectedSongs = value;
                OnPropertyChanged("SelectedSongs");
            }
        }


        private Command btnSelectPath_Click;
        /// <summary>
        /// Обработчик нажатия на кнопку "Выбрать папку"
        /// </summary>
        public Command BtnSelectPath_Click => btnSelectPath_Click
                   ?? (btnSelectPath_Click = new Command(o =>
                   {
                       RandDirectory = Randomizer.SelectDirectory(RandDirectory);
                       Playlist = null;
                   }));


        private Command btnMix_Click;
        /// <summary>
        /// Обработчик нажатия на кнопку "Создать плейлист"
        /// </summary>
        public Command BtnMix_Click => btnMix_Click
                   ?? (btnMix_Click = new Command(o =>
                   {
                       Playlist = Randomizer.GetRandomizedSongs(RandDirectory, SongsCount);
                       if (Playlist == null) MessageBox.Show("В выбранной директории недостаточно музыкальных файлов");
                   }, f => (RandDirectory == null || SongsCount == 0) ? false : true));


        private Command btnSave_Click;
        /// <summary>
        /// Обработчик нажатия на кнопку "Сохранить"
        /// </summary>
        public Command BtnSave_Click => btnSave_Click
                    ?? (btnSave_Click = new Command(o => Randomizer.SavePlaylist(Playlist), 
                        f => (Playlist == null || Playlist.Count == 0) ? false : true));


        private Command btnClear_Click;
        /// <summary>
        /// Обработчик нажатия на кнопку "Очистить"
        /// </summary>
        public Command BtnClear_Click => btnClear_Click
                    ?? (btnClear_Click = new Command(o =>
                    {
                        Playlist = null;
                        SelectedSongs = new List<PlaylistElement>();
                    }, f => (Playlist == null || Playlist.Count == 0) ? false : true));


        private Command btnRemoveSelected_Click;
        /// <summary>
        /// Обработчик нажатия на кнопку "Удалить выбранное"
        /// </summary>
        public Command BtnRemoveSelected_Click => btnRemoveSelected_Click
                    ?? (btnRemoveSelected_Click = new Command(o => 
                    {
                        SelectedSongs = Playlist.FindAll(s => s.IsSelected);
                        Playlist = Randomizer.ReplaceSongs(RandDirectory, Playlist, SelectedSongs, true);
                        SelectedSongs = new List<PlaylistElement>();
                    }, f => Playlist?.Exists(s => s.IsSelected) != true ? false : true));


        private Command btnReplaceSelected_Click;
        /// <summary>
        /// Обработчик нажатия на кнопку "Заменить выбранное"
        /// </summary>
        public Command BtnReplaceSelected_Click => btnReplaceSelected_Click
                    ?? (btnReplaceSelected_Click = new Command(o =>
                    {
                        SelectedSongs = Playlist.FindAll(s => s.IsSelected);
                        Playlist = Randomizer.ReplaceSongs(RandDirectory, Playlist, SelectedSongs, false);
                        SelectedSongs = new List<PlaylistElement>();
                    }, f => Playlist?.Exists(s => s.IsSelected) != true ? false : true));


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
