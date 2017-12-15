/*
 * framelistwidgetitem.cpp
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

#include "framelistwidget.h"

using namespace constants;

/*
 * helpers
 */
const QString LAYER_WIDGET_STYLE_SHEET = \
        "QListWidget::item {\n"
        "    background: #cc999999;\n"
        "    padding: 3px;\n"
        "}\n"
        "\n"
        "QListWidget::item:hover {\n"
        "    background: #cc444444;\n"
        "    padding: 3px;\n"
        "}\n"
        "\n"
        "QListWidget::item:selected {\n"
        "    background: #ccffd700;\n"
        "    padding: 3px;\n"
        "}\n";
const int TOTAL_ITEM_LIST_WIDGET_SEPARATION_PX = 4;
const int TOTAL_ITEM_BORDER_THICKNESS_PX = 6;
const int TOTAL_FRAME_SEPARATION_PX = TOTAL_ITEM_LIST_WIDGET_SEPARATION_PX
                                    + TOTAL_ITEM_BORDER_THICKNESS_PX;

/*
 * public
 */
FrameListWidget::FrameListWidget(EditorWindow *controller, QWidget *parent) :
    SmartListWidget(controller, parent)
{
    setDragDropMode(QAbstractItemView::InternalMove);
    setStyleSheet(LAYER_WIDGET_STYLE_SHEET);

    connect(this, &FrameListWidget::currentRowChanged,
            this, &FrameListWidget::currentFrameChanged);
}

QList<FrameWidget *> FrameListWidget::renderOrderWidgetList() const
{
    return widgetList();
}

/*
 * public slots
 */
FrameListWidgetItem * FrameListWidget::addNewFrame(FrameListWidget *parent)
{
    // use this list widget if the parent isn't specified
    if (!parent) parent = this;

    // construct
    FrameListWidgetItem *itm = new FrameListWidgetItem();
    FrameWidget *frame = new FrameWidget(parent->controller, parent);

    // connections
    parent->connect(frame, &FrameWidget::imageChanged,
            [=](LayerListWidget *layerList,
                LayerListWidgetItem *layerItm,
                QImage img,
                QRect uptRect){
        emit parent->frameImageChanged(
                    parent, itm, layerList, layerItm, img, uptRect);
    });
    parent->connect(frame, &FrameWidget::frameRenderingChanged, [=]{
        emit parent->frameRenderingChanged();
        if (parent->currentWidget() == frame)
            emit parent->currentFrameChanged();
    });

    // settings
    itm->setSizeHint(parent->getItemSize());
    frame->setFixedSize(parent->getFrameSize());

    // insert item
    parent->insertItem(parent->currentRow()+DEFAULT_INSERT_FRAME_OFFSET, itm);
    parent->setItemWidget(itm, frame);

    // notify
    emit parent->frameRenderingChanged();

    return itm;
}

FrameListWidgetItem * FrameListWidget::duplicateFrame(
        FrameListWidget *parent,
        FrameWidget *frame)
{
    // use the this list widget and current item if unspecified
    if (!parent) parent = this;
    if (!frame) frame = parent->currentWidget();

    // quietly get a new frame
    parent->blockSignals(true);
    FrameListWidgetItem *newItm = parent->addNewFrame(parent);
    parent->blockSignals(false);

    FrameWidget *newFrame = parent->itemWidget(newItm);

    // copy item's contents over, including setting the current row
    if (frame)
    {
        LayerListWidget *newLayerList = newFrame->getLayerListWidget();
        LayerListWidget *layerList = frame->getLayerListWidget();
        for (LayerWidget *layer : layerList->renderOrderWidgetList())
        {
            newLayerList->duplicateLayer("", newLayerList, layer);
        }
        newLayerList->setCurrentRow(layerList->currentRow());
    }

    // notify
    parent->frameRenderingChanged();

    return newItm;
}

FrameListWidgetItem * FrameListWidget::removeFrame(
        FrameListWidget *parent,
        FrameListWidgetItem *itm)
{
    if (!parent) parent = this;
    if (!itm) itm = parent->currentItem();
    FrameListWidgetItem *itmRemoved = parent->takeItem(row(itm));
    emit parent->frameRenderingChanged();
    return itmRemoved;
}

/*
 * protected
 */
void FrameListWidget::dropEvent(QDropEvent *event)
{
    emit frameRenderingChanged();
    SmartListWidget<FrameListWidgetItem, FrameWidget>::dropEvent(event);
}

void FrameListWidget::resizeEvent(QResizeEvent *event)
{
    Q_UNUSED(event)

    // determinethe flow direction
    setFlow(isLeftToRight() ? LeftToRight : TopToBottom);

    // always show a scrollbar to prevent infinite loop
    setHorizontalScrollBarPolicy(isLeftToRight() ? Qt::ScrollBarAlwaysOn
                                                 : Qt::ScrollBarAlwaysOff);
    setVerticalScrollBarPolicy(isLeftToRight() ? Qt::ScrollBarAlwaysOff
                                               : Qt::ScrollBarAlwaysOn);

    // determine the new size of the children
    for (int i = 0; i < count(); ++i)
    {
        FrameListWidgetItem *itm = item(i);
        FrameWidget *frame = itemWidget(itm);

        itm->setSizeHint(getItemSize());
        frame->setFixedSize(getFrameSize());
    }
}

/*
 * private
 */
QSize FrameListWidget::getItemSize() const
{
    return getFrameSize() + QSize(TOTAL_ITEM_BORDER_THICKNESS_PX,
                                  TOTAL_ITEM_BORDER_THICKNESS_PX);
}

QSize FrameListWidget::getFrameSize() const
{
    return getScaledSizeWithSeparation(
                controller->getSpriteSize(), TOTAL_FRAME_SEPARATION_PX);
}

QSize FrameListWidget::getScaledSizeWithSeparation(
        QSize size, int separation) const
{
    int scollbar = qApp->style()->pixelMetric(QStyle::PM_ScrollBarExtent);
    size.scale(isLeftToRight() ? QSize(1, height() - separation - scollbar)
                               : QSize(width() - separation - scollbar, 1),
               Qt::KeepAspectRatioByExpanding);
    return size;
}

bool FrameListWidget::isLeftToRight() const
{
    return width() > height();
}
