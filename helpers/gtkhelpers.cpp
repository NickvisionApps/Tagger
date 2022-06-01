#include "gtkhelpers.h"

using namespace NickvisionTagger::Helpers;

void GtkHelpers::gtk_image_set_from_byte_vector(GtkImage* image, const TagLib::ByteVector& byteVector)
{
    if(byteVector.isEmpty())
    {
        gtk_image_clear(image);
    }
    else
    {
        GdkPixbufLoader* pixbufLoader{gdk_pixbuf_loader_new()};
        gdk_pixbuf_loader_write(pixbufLoader, (unsigned char*)byteVector.data(), byteVector.size(), nullptr);
        gtk_image_set_from_pixbuf(image, gdk_pixbuf_loader_get_pixbuf(pixbufLoader));
        gdk_pixbuf_loader_close(pixbufLoader, nullptr);
        g_object_unref(pixbufLoader);
    }
}

TagLib::ByteVector GtkHelpers::gdk_pixbuf_get_byte_vector(GdkPixbuf* pixbuf)
{
    unsigned int pixbufLength{0};
    unsigned char* data{gdk_pixbuf_get_pixels_with_length(pixbuf, &pixbufLength)};
    return {(char*)data, pixbufLength};
}