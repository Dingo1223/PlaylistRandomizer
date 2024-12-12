namespace PlaylistRandomizerCore
{
    public class PlaylistElement
    {
        public string FullPath { get; set; }
        public string ViewPath { get; set; }
        public bool IsSelected { get; set; }

        public PlaylistElement(string fullPath, string directory)
        {
            FullPath = fullPath;
            ViewPath = fullPath.Replace(directory + "\\", ""); // TODO : сомнительно но окэй
            IsSelected = false;
        }
    }
}
