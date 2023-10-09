using System.Collections.Generic;

namespace NickvisionTagger.WinUI.Helpers;

/// <summary>
/// A list with a title
/// </summary>
public class TitledList : List<object>
{
    /// <summary>
    /// The title of the list
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Constructs a TitledList
    /// </summary>
    /// <param name="title">The title of the list</param>
    public TitledList(string title)
    {
        Title = title;
    }

    /// <summary>
    /// Constructs a TitledList
    /// </summary>
    /// <param name="title">The title of the list</param>
    /// <param name="items">Items to add to the list</param>
    public TitledList(string title, IEnumerable<object> items) : base(items)
    {
        Title = title;
    }
}

