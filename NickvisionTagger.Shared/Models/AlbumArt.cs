using ATL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace NickvisionTagger.Shared.Models;

/// <summary>
/// Types of album arts
/// </summary>
public enum AlbumArtType
{
    Front = 0,
    Back
}

/// <summary>
/// A model of an AlbumArt
/// </summary>
public class AlbumArt : IEquatable<AlbumArt>
{
    private byte[] _image;

    /// <summary>
    /// The type of the album art
    /// </summary>
    public AlbumArtType Type { get; init; }
    /// <summary>
    /// The width of the album art
    /// </summary>
    public int Width { get; init; }
    /// <summary>
    /// The height of the album art
    /// </summary>
    public int Height { get; init; }
    /// <summary>
    /// <see cref="Image"/> converted to 32x32 JPEG, as byte[]
    /// </summary>
    public byte[] Icon { get; init; }
    /// <summary>
    /// The ATL.PictureInfo object of the album art
    /// </summary>
    public PictureInfo ATLPictureInfo { get; init; }

    /// <summary>
    /// Whether or not the album art is empty
    /// </summary>
    public bool IsEmpty => Image.Length == 0;
    /// <summary>
    /// The mime type of the album art
    /// </summary>
    public string MimeType => ATLPictureInfo.MimeType;

    /// <summary>
    /// Constructs an AlbumArt
    /// </summary>
    /// <param name="data">The byte[] of the album art image</param>
    public AlbumArt(byte[] data, AlbumArtType type)
    {
        Image = data;
        Type = type;
        ATLPictureInfo = PictureInfo.fromBinaryData(Image, Type == AlbumArtType.Front ? PictureInfo.PIC_TYPE.Front : PictureInfo.PIC_TYPE.Back);
    }

    /// <summary>
    /// Constructs an AlbumArt
    /// </summary>
    /// <param name="pictureInfo">The ATL.PictureInfo object</param>
    public AlbumArt(PictureInfo pictureInfo)
    {
        Image = pictureInfo.PictureData;
        Type = pictureInfo.PicType == PictureInfo.PIC_TYPE.Back ? AlbumArtType.Back : AlbumArtType.Front; //PIC_TYPE.Generic classifies as AlbumArtTpye.Front
        ATLPictureInfo = pictureInfo;
        ATLPictureInfo.PicType = Type == AlbumArtType.Front ? PictureInfo.PIC_TYPE.Front : PictureInfo.PIC_TYPE.Back; //ensure PicType in case of generic
    }

    /// <summary>
    /// The byte[] of the album art image
    /// </summary>
    public byte[] Image
    {
        get => _image;

        init
        {
            _image = value;
            if (_image.Length > 0)
            {
                using var image = SixLabors.ImageSharp.Image.Load(_image);
                Width = image.Width;
                Height = image.Height;
                image.Mutate(x => x.Resize(32, 32));
                using var ms = new MemoryStream();
                image.SaveAsJpeg(ms);
                Icon = ms.ToArray();
            }
            else
            {
                Width = 0;
                Height = 0;
                Icon = Array.Empty<byte>();
            }
        }
    }

    /// <summary>
    /// Gets a string representation of the AlbumArt
    /// </summary>
    /// <returns>The string representation of the AlbumArt</returns>
    public override string ToString() => Encoding.UTF8.GetString(Image);

    /// <summary>
    /// Gets whether or not an object is equal to this AlbumArt
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equals, else false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is AlbumArt toCompare)
        {
            return Equals(toCompare);
        }
        return false;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this AlbumArt
    /// </summary>
    /// <param name="obj">The AlbumArt? object to compare</param>
    /// <returns>True if equals, else false</returns>
    public bool Equals(AlbumArt? obj) => Image.SequenceEqual(obj?.Image ?? Array.Empty<byte>());

    /// <summary>
    /// Gets a hash code for the object
    /// </summary>
    /// <returns>The hash code for the object</returns>
    public override int GetHashCode() => Image.GetHashCode();

    /// <summary>
    /// Compares two AlbumArt objects by ==
    /// </summary>
    /// <param name="a">The first AlbumArt object</param>
    /// <param name="b">The second AlbumArt object</param>
    /// <returns>True if a == b, else false</returns>
    public static bool operator ==(AlbumArt? a, AlbumArt? b) => (a?.Image ?? Array.Empty<byte>()).SequenceEqual(b?.Image ?? Array.Empty<byte>());

    /// <summary>
    /// Compares two AlbumArt objects by !=
    /// </summary>
    /// <param name="a">The first AlbumArt object</param>
    /// <param name="b">The second AlbumArt object</param>
    /// <returns>True if a != b, else false</returns>
    public static bool operator !=(AlbumArt? a, AlbumArt? b) => !(a?.Image ?? Array.Empty<byte>()).SequenceEqual(b?.Image ?? Array.Empty<byte>());
}
