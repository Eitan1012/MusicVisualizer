namespace MusicVisualizer.Metadata;

// Reads metadata from MP3 files
public static class Mp3MetadataReader
{
    public static TrackInfo Read(string filePath)
    {
        using var file = TagLib.File.Create(filePath);
        var track = new TrackInfo
        {
            Title = file.Tag.Title,
            Artist = file.Tag.FirstPerformer,
            Album = file.Tag.Album
        };

        if (file.Tag.Pictures.Length > 0)
        {
            track.CoverArt = file.Tag.Pictures[0].Data.Data;
        }
        return track;
    }
}