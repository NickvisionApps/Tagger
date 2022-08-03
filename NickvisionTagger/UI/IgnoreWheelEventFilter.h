#pragma once

#include <QEvent>
#include <QObject>

namespace NickvisionTagger::UI
{
	/// <summary>
	/// A filter for ignoring the wheel event
	/// </summary>
	class IgnoreWheelEventFilter : public QObject
	{
		Q_OBJECT

	public:
		/// <summary>
		/// Constructs a IgnoreWheelEventFilter
		/// </summary>
		/// <param name="parent">The parent of the filter</param>
		IgnoreWheelEventFilter(QObject* parent);

	protected:
		/// <summary>
		/// Filters the event
		/// </summary>
		/// <param name="watched">QObject*</param>
		/// <param name="event">QEvent*</param>
		/// <returns>True if filtered, else false</returns>
		bool eventFilter(QObject* watched, QEvent* event) override;
	};
}

