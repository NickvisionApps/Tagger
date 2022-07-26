#pragma once

#include <QPalette>
#include <QWidget>

/// <summary>
/// Functions for working with an application's theme
/// </summary>
namespace NickvisionTagger::Helpers::ThemeHelpers
{
	/// <summary>
	/// Gets a light themed QPalette
	/// </summary>
	/// <returns>Light themed QPalette</returns>
	QPalette getLightPalette();
	/// <summary>
	/// Gets a dark themed QPalette
	/// </summary>
	/// <returns>Dark themed QPalette</returns>
	QPalette getDarkPalette();
	/// <summary>
	/// Applys Win32 theming to QWidget's Title Bar
	/// </summary>
	/// <param name="widget">The QWidget object</param>
	void applyWin32Theme(QWidget* widget);
}

