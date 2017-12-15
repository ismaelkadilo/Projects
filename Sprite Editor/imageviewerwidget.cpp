/*
 * imageviewerwidget.cpp
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

#include "imageviewerwidget.h"

using namespace std;

/*
 * public
 */
ImageViewerWidget::ImageViewerWidget(
        EditorWindow *controllingEditorWindow,
        QWidget *parent,
        Qt::WindowFlags flags) :
    controller(controllingEditorWindow),
    QWidget(parent, flags)
{
}

function<void(const QImage &)> ImageViewerWidget::getTransformedDrawImage(
        QPainter *painter,
        QPaintEvent *event,
        QWidget *widget)
{
    if (!widget) widget = this;

    // use QRectF, QRect integer truncation causes 1px gap
    painter->setTransform(spriteToWidgetTf(widget));
    QRectF dirtyRect = !event ? widgetToSpriteTf(widget).mapRect(
                                    static_cast<QRectF>(widget->rect()))
                              : widgetToSpriteTf(widget).mapRect(
                                    static_cast<QRectF>(event->rect()));

    return [painter, dirtyRect](const QImage &image){
        painter->drawImage(dirtyRect, image, dirtyRect);
    };
}

QTransform ImageViewerWidget::spriteToWidgetTf(QWidget *widget)
{
    if (!widget) widget = this;
    return getTransform(controller->getSpriteSize(), widget->size());
}

QTransform ImageViewerWidget::widgetToSpriteTf(QWidget *widget)
{
    if (!widget) widget = this;
    return getTransform(widget->size(), controller->getSpriteSize());
}

QTransform ImageViewerWidget::getTransform(QSize size1, QSize size2)
{
    QTransform tf;
    return tf.scale(static_cast<qreal>(size2.width()) / size1.width(),
                    static_cast<qreal>(size2.height()) / size1.height());
}

QPoint ImageViewerWidget::widgetToSpriteRoundingToFloorShift(QWidget *widget)
{
    if (!widget) widget = this;
    return -spriteToWidgetTf(widget).map(QPoint(1, 1)) / 2;
}

QPoint ImageViewerWidget::spriteToWidgetRoundingToFloorShift(QWidget *widget)
{
    if (!widget) widget = this;
    return -widgetToSpriteTf(widget).map(QPoint(1, 1)) / 2;
}

void ImageViewerWidget::paintGridLines(
        int showGridZoomPercentThreshold,
        int lineSpacingPx,
        QColor lineColor,
        QWidget *widget)
{
    if (!widget) widget = this;
    QSize spriteSize = controller->getSpriteSize();
    const qreal SCALE = static_cast<qreal>(widget->width())
                        / spriteSize.width();
    if (SCALE > showGridZoomPercentThreshold / 100.0)
    {
        QPainter gridPainter(this);
        gridPainter.setPen(lineColor);

        // vertical lines
        for (int i = 0; i < spriteSize.width(); i += lineSpacingPx)
            gridPainter.drawLine(i*SCALE, 0, i*SCALE, widget->height());

        // horizontal lines
        for (int i = 0; i < spriteSize.height(); i += lineSpacingPx)
            gridPainter.drawLine(0, i*SCALE, widget->width(), i*SCALE);
    }
}

/*
 * protected
 */
void ImageViewerWidget::paintEvent(QPaintEvent *event)
{
    Q_UNUSED(event)

    QPainter painter(this);

    // apply stylesheet
    QStyleOption styleOption;
    styleOption.initFrom(this);
    style()->drawPrimitive(QStyle::PE_Widget, &styleOption, &painter, this);
}
