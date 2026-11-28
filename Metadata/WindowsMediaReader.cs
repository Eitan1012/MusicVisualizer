using Windows.Media.Control;
using Windows.Storage.Streams;
namespace MusicVisualizer.Metadata;

// Reads the current track information from the Windows Media session
public static class WindowsMediaReader
{
    public static async Task<TrackInfo?> ReadAsync()
    {
        var manager =await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        var session = manager.GetCurrentSession();
        if (session == null)
        {
            return null;
        }
        var properties = await session.TryGetMediaPropertiesAsync();
        var track = new TrackInfo
        {
            Title = properties.Title,
            Artist = properties.Artist,
            Album = properties.AlbumTitle
        };
// Read cover art as a byte array
        if (properties.Thumbnail != null)
        {
            using var stream = await properties.Thumbnail.OpenReadAsync();
            using var dataReader = new DataReader(stream);
            uint size = (uint)stream.Size;
            uint lodad = await dataReader.LoadAsync(size);
            if (lodad == size && size > 0)
            {
                byte[] cover = new byte[size];
                dataReader.ReadBytes(cover);
                track.CoverArt = cover;
            }

        }
        return track;

    }
}