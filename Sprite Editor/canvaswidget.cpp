/*
 * canvaswidget.h
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

#include "canvaswidget.h"

#include "layerlistwidget.h"
#include "layerwidget.h"
#include "toolmanagerwidget.h"

using namespace constants;
using namespace std;

/*
 * public
 */
CanvasWidget::CanvasWidget(
        EditorWindow *controller,
        QWidget *parent,
        Qt::WindowFlags flags) :
    ImageViewerWidget(controller, parent, flags),
    previewImage(controller->getSpriteSize(),
                 controller->getSpriteImageFormat()),
    topOverlayImage(controller->getSpriteSize(),
                    controller->getSpriteImageFormat())
{
    // internal states
    clearPreviewImage();
    topOverlayImage.fill(TRANSPARENT_BLACK);

    // window settings
    setCursor(QCursor(Qt::CrossCursor));
    setMouseTracking(true);
    setProperty("textured", true);
}

QPoint CanvasWidget::getStartPos() const
{
    return startPos;
}

QPoint CanvasWidget::getPrevPos() const
{
    return prevPos;
}

QPoint CanvasWidget::getCurrentPos() const
{
    return currentPos;
}

QImage CanvasWidget::getPreviewImage() const
{
    return previewImage;
}

QImage CanvasWidget::getClearedPreviewImage() const
{
    QImage preview = getPreviewImage();
    preview.fill(TRANSPARENT_BLACK);
    return preview;
}

QImage CanvasWidget::getTopOverlayImage() const
{
    return topOverlayImage;
}

QImage CanvasWidget::getClearedTopOverlayImage() const
{
    QImage topOverlay = getTopOverlayImage();
    topOverlay.fill(TRANSPARENT_BLACK);
    return topOverlay;
}

/*
 * public slots
 */
void CanvasWidget::setStartPos(QPoint pos)
{
    if (getStartPos() != pos)
    {
        startPos = pos;
    }
}

void CanvasWidget::setPrevPos(QPoint pos)
{
    if (getPrevPos() != pos)
    {
        prevPos = pos;
    }
}

void CanvasWidget::setCurrentPos(QPoint pos)
{
    if (getCurrentPos() != pos)
    {
        currentPos = pos;
    }
}

void CanvasWidget::setPreviewImage(QImage preview, QRect uptRect)
{
    // update the entire image if the rect wasn't specified
    if (!uptRect.isValid()) uptRect = preview.rect();

    previewImage = preview;
    update(spriteToWidgetTf().mapRect(uptRect));
}

void CanvasWidget::clearPreviewImage()
{
    setPreviewImage(getClearedPreviewImage());
}

void CanvasWidget::setTopOverlayImage(QImage topOverlay, QRect uptRect)
{
    if (!uptRect.isValid()) uptRect = topOverlay.rect();

    topOverlayImage = topOverlay;
    update(spriteToWidgetTf().mapRect(uptRect));
}

void CanvasWidget::clearTopOverlayImage()
{
    setTopOverlayImage(getClearedTopOverlayImage());
}

void CanvasWidget::imageUpdate(QRect uptRect)
{
    update(spriteToWidgetTf().mapRect(uptRect));
}

/*
 * protected
 */
void CanvasWidget::mousePressEvent(QMouseEvent *event)
{
    controller->ensureSelectedLayer();
    handleMouseEvent(event, &ToolManagerWidget::handleMousePress);
    setStartPos(getCurrentPos());

    // clear just in case
    clearTopOverlayImage();
}

void CanvasWidget::mouseMoveEvent(QMouseEvent *event)
{
    handleMouseEvent(event, &ToolManagerWidget::handleMouseMove);

    // highlight the pixel the mouse is currently over
    clearTopOverlayImage();
    if (!event->buttons())
    {
        QImage topOverlay = getTopOverlayImage();
        QPainter painter(&topOverlay);
        painter.setPen(DEFAULT_UI_PEN);
        painter.drawPoint(getCurrentPos());
        setTopOverlayImage(topOverlay,
                           controller->getToolManager()->addPenWidth(
                               QRect(getCurrentPos(), getCurrentPos())));
    }
}

void CanvasWidget::mouseReleaseEvent(QMouseEvent *event)
{
    handleMouseEvent(event, &ToolManagerWidget::handleMouseRelease);
}

void CanvasWidget::paintEvent(QPaintEvent *event)
{
    ImageViewerWidget::paintEvent(event);

    // render the canvas
    QPainter painter(this);
    auto drawImage = getTransformedDrawImage(&painter, event);

    LayerListWidget *layerList = controller->getCurrentLayerListWidget();
    LayerWidget * const CURRENT_LAYER = controller->getCurrentLayerWidget();
    bool isPreviewImageDrawn = false;
    if (layerList)
    {
        for (LayerWidget *layer : layerList->renderOrderWidgetList())
        {
            if (layer->isLayerVisible()) drawImage(layer->getImage());
            if (CURRENT_LAYER == layer)
            {
                // draw the preview on top of the current layer
                drawImage(getPreviewImage());
                isPreviewImageDrawn = true;
            }
        }
    }
    // ensure that the preview image is always drawn
    if (!isPreviewImageDrawn) drawImage(getPreviewImage());

    // draw top overlay on top
    drawImage(getTopOverlayImage());

    // draw grid lines
    paintGridLines();
}

void CanvasWidget::handleMouseEvent(
        QMouseEvent *event,
        void (ToolManagerWidget::*handler)
             (QMouseEvent *, EditorWindow *, CanvasWidget *))
{
    // use floor, not round, to get pixel position in sprite coord system
    setCurrentPos(widgetToSpriteTf().map(
                      event->pos() + widgetToSpriteRoundingToFloorShift()));
    (controller->getToolManager()->*handler)(event, controller, this);
    setPrevPos(getCurrentPos());
}
