#pragma once

#include <adwaita.h>
#include <taglib/tbytevector.h>

namespace NickvisionTagger::Helpers::GtkHelpers
{
    void gtk_image_set_from_byte_vector(GtkImage* image, const TagLib::ByteVector& byteVector);
}