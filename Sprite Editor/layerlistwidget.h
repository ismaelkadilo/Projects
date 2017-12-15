/*
 * layerlistwidget.h
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

#ifndef LAYERLISTWIDGET_H
#define LAYERLISTWIDGET_H

#include "smartlistwidget.hpp"
#include "layerlistwidgetitem.h"
#include "layerwidget.h"

class LayerListWidget
        : public SmartListWidget<LayerListWidgetItem, LayerWidget>
{
    Q_OBJECT

public:
    explicit LayerListWidget(
            EditorWindow *controller,
            QWidget *parent = Q_NULLPTR);

    QList<LayerWidget *> renderOrderWidgetList() const override;

signals:
    void layerImageChanged(
            LayerListWidget *layerList,
            LayerListWidgetItem *layerItm,
            QImage img,
            QRect uptRect);
    void layerRenderingChanged();

public slots:
    LayerListWidgetItem * addNewLayer(LayerListWidget *parent = Q_NULLPTR);
    LayerListWidgetItem * duplicateLayer(const QString &appendText = " copy",
                                         LayerListWidget *parent = Q_NULLPTR,
                                         LayerWidget *layer = Q_NULLPTR);
    LayerListWidgetItem * removeLayer(LayerListWidget *parent = Q_NULLPTR,
                                      LayerListWidgetItem *itm = Q_NULLPTR);

protected:
    void dropEvent(QDropEvent *event) override;

private:
    int layerCreatedCount;
};

#endif // LAYERLISTWIDGET_H
