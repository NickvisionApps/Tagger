#pragma once

#include <adwaita.h>
#include <taglib/tbytevector.h>

namespace NickvisionTagger::Helpers::GtkHelpers
{
	/**
	 * Sets a GtkImage's source from the TagLib::ByteVector
	 *
	 * @param image The GtkImage
	 * @param byteVector The TagLib::ByteVector representing the image
	 */
    void gtk_image_set_from_byte_vector(GtkImage* image, const TagLib::ByteVector& byteVector);
}