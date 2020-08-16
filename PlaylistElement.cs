namespace PlaylistRandomizer
{
    public class PlaylistElement
    {
        public string FullPath { get; set; }
        public string ViewPath { get; set; }
        public bool IsSelected { get; set; }

        public PlaylistElement(string fullPath, string viewPath)
        {
            FullPath = fullPath;
            ViewPath = viewPath;
            IsSelected = false;
        }
    }
}

