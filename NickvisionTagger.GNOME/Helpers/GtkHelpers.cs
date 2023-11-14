using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NickvisionTagger.GNOME.Helpers;

public unsafe static partial class GtkHelpers
{
    [StructLayout(LayoutKind.Sequential)]
    private struct GLibList
    {
        public nint data;
        public GLibList* next;
        public GLibList* prev;
    }

    [LibraryImport("libadwaita-1.so.0")]
    private static partial int gtk_flow_box_child_get_index(nint row);
    [LibraryImport("libadwaita-1.so.0")]
    private static partial GLibList* gtk_list_box_get_selected_rows(nint box);
    [LibraryImport("libadwaita-1.so.0")]
    private static partial int gtk_list_box_row_get_index(nint row);

    /// <summary>
    /// Helper extension method for Gtk.ListBox to get indices of selected row
    /// </summary>
    /// <param name="box">List box</param>
    /// <returns>List of indices</returns>
    public static List<int> GetSelectedRowsIndices(this Gtk.ListBox box)
    {
        var list = new List<int>();
        var firstSelectedRowPtr = gtk_list_box_get_selected_rows(box.Handle);
        for (var ptr = firstSelectedRowPtr; ptr != null; ptr = ptr->next)
        {
            list.Add(gtk_list_box_row_get_index(ptr->data));
        }
        return list;
    }
}
