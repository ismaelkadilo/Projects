/*
 * imageviewerwidget.h
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

#ifndef IMAGEVIEWERWIDGET_H
#define IMAGEVIEWERWIDGET_H

#include "constants.h"
#include "editorwindow.h"

#include <functional>

class ImageViewerWidget : public QWidget
{
    Q_OBJECT

public:
    ImageViewerWidget(
            EditorWindow *controller,
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());

    std::function<void(const QImage &)> getTransformedDrawImage(
            QPainter *painter,
            QPaintEvent *event = Q_NULLPTR,
            QWidget *widget = Q_NULLPTR);

    QTransform spriteToWidgetTf(QWidget *widget = Q_NULLPTR);
    QTransform widgetToSpriteTf(QWidget *widget = Q_NULLPTR);
    QTransform getTransform(QSize size1, QSize size2);

    // QTransform rounds fractional numbers, add this shift to get floor
    QPoint widgetToSpriteRoundingToFloorShift(QWidget *widget = Q_NULLPTR);
    QPoint spriteToWidgetRoundingToFloorShift(QWidget *widget = Q_NULLPTR);

    void paintGridLines(
            int  showGridZoomPercentThreshold = \
                    constants::DEFAULT_SHOW_GRID_ZOOM_PERCENT_THRESHOLD,
            int lineSpacingPx = constants::DEFAULT_GRID_LINE_SPACING,
            QColor lineColor = constants::DEFAULT_GRID_LINE_COLOR,
            QWidget *widget = Q_NULLPTR);

protected:
    EditorWindow *controller;

    void paintEvent(QPaintEvent *event) override;

};

#endif // IMAGEVIEWERWIDGET_H
