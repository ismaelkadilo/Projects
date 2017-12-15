/*
 * constants.h
 *
 * Anthony Chyr (u0627375)
 * Carlos Enrique Guerra Chan (u0847821)
 * Elliot C Carr-Lee (u0549837)
 * Ismael Kadilo Wa Ngoie (u1120347)
 * Kameron Service (u0963620)
 *
 * CS 3505 Software Practices II Johnson
 * A7: Sprite Editor Implementation
 */

#ifndef CONSTANTS_H
#define CONSTANTS_H

#include <QBrush>
#include <QImage>
#include <QPen>
#include <QSize>

namespace constants
{
const QSize MIN_SPRITE_SIZE = QSize(1 << 0, 1 << 0);
const QSize MAX_SPRITE_SIZE = QSize(1 << 16, 1 << 16);
const QSize DEFAULT_SPRITE_SIZE = QSize(64, 64);

const QImage::Format DEFAULT_IMAGE_FORMAT = \
        QImage::Format_ARGB32_Premultiplied;

const QRgb TRANSPARENT_BLACK = qRgba(0, 0, 0, 0);
const QRgb DEFAULT_BACKGROUND_FILL_COLOR = TRANSPARENT_BLACK;

const QSize DEFAULT_TOOL_PUSH_BUTTON_SIZE = QSize(24, 24);
const QSize DEFAULT_TOOL_PUSH_BUTTON_ICON_SIZE = QSize(24, 24);

const int ZOOM_MIN_PERCENT = 1;
const int ZOOM_MAX_PERCENT = 10000;
const int DEFAULT_ZOOM_PERCENT = 100;
const int DEFAULT_ZOOM_STEP_PERCENT = 10;

const int DEFAULT_GRID_LINE_SPACING = 1;
const QColor DEFAULT_GRID_LINE_COLOR = QColor(255, 255, 255, 127);
const int DEFAULT_SHOW_GRID_ZOOM_PERCENT_THRESHOLD = 400;

// insert after the current item (list grows left to right)
const int DEFAULT_INSERT_FRAME_OFFSET = 1;
// insert before the current item (list grows bottom to top)
const int DEFAULT_INSERT_LAYER_OFFSET = 0;

const int DEFAULT_LAYER_ITEM_HEIGHT = 28;

const QBrush DEFAULT_BRUSH = QBrush(QColor(255, 255, 255, 255),
                                    Qt::SolidPattern);
const QPen DEFAULT_PEN = QPen(QColor(0, 0, 0, 255),
                              1,
                              Qt::SolidLine,
                              Qt::RoundCap,
                              Qt:: RoundJoin);
const QPen DEFAULT_UI_PEN = QPen(QColor(127, 127, 127, 127),
                                 1,
                                 Qt::SolidLine,
                                 Qt::RoundCap,
                                 Qt::RoundJoin);
const int MIN_PEN_WIDTH = 0;
const int MAX_PEN_WIDTH = 1000;

const int MIN_FRAME_RATE_PER_SECOND = 0;
const int MAX_FRAME_RATE_PER_SECOND = 30;
const int DEFAULT_FRAME_RATE_PER_SECOND = 18;
}

#endif // CONSTANTS_H
