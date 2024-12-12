using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace PlaylistRandomizerCore
{
    public class ViewModel : ReactiveObject
    {
        [Reactive] public string Directory { get; set; }
        [Reactive] public int Count { get; set; }
        [Reactive] public ObservableCollection<PlaylistElement> Playlist { get; set; }

        private readonly Randomizer randomizer;

        public ViewModel()
        {
            randomizer = new();
            Directory = Randomizer.GetDefaultDirectory();
            Count = 50;
            Playlist = [];
        }


        private RelayCommand? selectFolder;
        public RelayCommand SelectFolder => selectFolder ??= new RelayCommand(_ =>
        {
            Directory = Randomizer.SelectDirectory();
            Playlist = [];
        });


        private RelayCommand? createMix;
        public RelayCommand CreateMix => createMix ??= new RelayCommand(_ =>
        {
            List<PlaylistElement>? generated = randomizer.GetRandomizedSongs(Directory, Count);

            if (generated == null)
            {
                _ = MessageBox.Show("В выбранной директории недостаточно музыкальных файлов");
                return;
            }

            Playlist = new(generated);
        }, _ => !string.IsNullOrWhiteSpace(Directory) && Count > 0);


        private RelayCommand? savePlaylist;
        public RelayCommand SavePlaylist => savePlaylist ??= new RelayCommand(_ =>
        {
            Randomizer.SavePlaylist([.. Playlist]);
        }, _ => Playlist.Any());


        private RelayCommand? clear;
        public RelayCommand Clear => clear ??= new RelayCommand(_ =>
        {
            Playlist = [];
        }, _ => Playlist.Any());


        private RelayCommand? removeSelected;
        public RelayCommand RemoveSelected => removeSelected ??= new RelayCommand(_ =>
        {
            List<PlaylistElement> selected = Playlist.Where(x => x.IsSelected).ToList();
            Playlist = [.. randomizer.ReplaceSongs(Directory, Playlist.ToList(), selected, true)];
        }, _ => Playlist.Any(x => x.IsSelected));


        private RelayCommand? replaceSelected;
        public RelayCommand ReplaceSelected => replaceSelected ??= new RelayCommand(_ =>
        {
            List<PlaylistElement> selected = Playlist.Where(x => x.IsSelected).ToList();
            Playlist = [.. randomizer.ReplaceSongs(Directory, Playlist.ToList(), selected, false)];
        }, _ => Playlist.Any(x => x.IsSelected));
    }
}
