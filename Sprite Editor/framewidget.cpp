/*
 * framewidget.cpp
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

#include "framewidget.h"

/*
 * public
 */
FrameWidget::FrameWidget(
        EditorWindow *controller,
        QWidget *parent,
        Qt::WindowFlags flags) :
    ImageViewerWidget(controller, parent, flags),
    layerListWidget(new LayerListWidget(controller))
{
    // gui
    setProperty("textured", true);

    // connections
    connect(layerListWidget, &LayerListWidget::layerImageChanged,
            [=](LayerListWidget *layerList,
                LayerListWidgetItem *layerItm,
                QImage img,
                QRect uptRect){
        emit imageChanged(layerList, layerItm, img, uptRect);
        update(spriteToWidgetTf().mapRect(uptRect));
    });
    connect(layerListWidget, &LayerListWidget::layerRenderingChanged,
            [=]{ emit frameRenderingChanged(); update(); });
}

FrameWidget::~FrameWidget()
{
    delete layerListWidget;
}

LayerListWidget * FrameWidget::getLayerListWidget() const
{
    return layerListWidget;
}

QImage FrameWidget::getFlattenedImage() const
{
    // construct a new image according to sprite's params
    QImage img(controller->getSpriteSize(),
               controller->getSpriteImageFormat());
    img.fill(controller->getSpriteBackgroundFill());

    // paint it with the visible layers
    QPainter painter(&img);
    for (LayerWidget *layer : getLayerListWidget()->renderOrderWidgetList())
        if (layer->isLayerVisible())
            painter.drawImage(QPoint(0, 0), layer->getImage());

    return img;
}

/*
 * protected
 */
void FrameWidget::paintEvent(QPaintEvent *event)
{
    ImageViewerWidget::paintEvent(event);

    QPainter painter(this);
    auto drawImage = getTransformedDrawImage(&painter, event);
    drawImage(getFlattenedImage());
}
