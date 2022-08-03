#include "IgnoreWheelEventFilter.h"

namespace NickvisionTagger::UI
{
	IgnoreWheelEventFilter::IgnoreWheelEventFilter(QObject* parent) : QObject{ parent }
	{
		
	}

	bool IgnoreWheelEventFilter::eventFilter(QObject* watched, QEvent* event)
	{
		if (event->type() == QEvent::Wheel)
		{
			return true;
		}
		return QObject::eventFilter(watched, event);
	}
}