/*
 * layerlistwidget.cpp
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

#include "layerlistwidget.h"

#include <algorithm>

using namespace constants;
using namespace std;

/*
 * public
 */
LayerListWidget::LayerListWidget(EditorWindow *controller, QWidget *parent) :
    SmartListWidget(controller, parent),
    layerCreatedCount(0)
{
    setDragDropMode(QAbstractItemView::InternalMove);
}

QList<LayerWidget *> LayerListWidget::renderOrderWidgetList() const
{
    // the bottom layer (last item in the list) is the first rendered
    QList<LayerWidget *> list = widgetList();
    reverse(list.begin(), list.end());
    return list;
}

/*
 * public slots
 */
LayerListWidgetItem * LayerListWidget::addNewLayer(LayerListWidget *parent)
{
    // use the this list widget if the parent isn't specified
    if (!parent) parent = this;

    // construct
    LayerListWidgetItem *itm = new LayerListWidgetItem();
    LayerWidget *layer = new LayerWidget(parent->controller, parent);

    // connections
    parent->connect(layer, &LayerWidget::imageChanged,
            [=](QImage img, QRect uptRect){
        emit parent->layerImageChanged(parent, itm, img, uptRect);
    });
    parent->connect(layer, &LayerWidget::layerVisibilityChanged,
                    parent, &LayerListWidget::layerRenderingChanged);

    // settings
    itm->setSizeHint(QSize(0, DEFAULT_LAYER_ITEM_HEIGHT));
    layer->setText(tr("Layer %1").arg(++(parent->layerCreatedCount)));

    // insert item
    parent->insertItem(parent->currentRow()+DEFAULT_INSERT_LAYER_OFFSET, itm);
    parent->setItemWidget(itm, layer);

    // notify
    emit parent->layerRenderingChanged();

    return itm;
}

LayerListWidgetItem * LayerListWidget::duplicateLayer(
        const QString &appendText,
        LayerListWidget *parent,
        LayerWidget *layer)
{
    // use the this list widget and current item if unspecified
    if (!parent) parent = this;
    if (!layer) layer = parent->currentWidget();

    // quietly create a new layer
    parent->blockSignals(true);
    LayerListWidgetItem *newItm = parent->addNewLayer(parent);
    parent->blockSignals(false);

    LayerWidget *newLayer = parent->itemWidget(newItm);

    // copy widget's contents over
    if (layer)
    {
        newLayer->setImage(layer->getImage().copy());
        newLayer->setText(layer->getText() + appendText);
        newLayer->setLayerVisibility(layer->isLayerVisible());
    }

    // notify
    emit parent->layerRenderingChanged();

    return newItm;
}

LayerListWidgetItem * LayerListWidget::removeLayer(
        LayerListWidget *parent,
        LayerListWidgetItem *itm)
{
    if (!parent) parent = this;
    if (!itm) itm = parent->currentItem();
    LayerListWidgetItem *itmRemoved = parent->takeItem(row(itm));
    emit parent->layerRenderingChanged();
    return itmRemoved;
}

/*
 * protected
 */
void LayerListWidget::dropEvent(QDropEvent *event)
{
    emit layerRenderingChanged();
    SmartListWidget<LayerListWidgetItem, LayerWidget>::dropEvent(event);
}
