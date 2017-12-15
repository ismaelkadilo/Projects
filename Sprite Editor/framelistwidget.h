/*
 * framelistwidget.h
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

#ifndef FRAMELISTWIDGET_H
#define FRAMELISTWIDGET_H

#include "smartlistwidget.hpp"
#include "framelistwidgetitem.h"
#include "framewidget.h"

class FrameListWidget
        : public SmartListWidget<FrameListWidgetItem, FrameWidget>
{
    Q_OBJECT

public:
    explicit FrameListWidget(
            EditorWindow *controller,
            QWidget *parent = Q_NULLPTR);

    QList<FrameWidget *> renderOrderWidgetList() const override;

signals:
    void frameImageChanged(
            FrameListWidget *frameList,
            FrameListWidgetItem *frameItm,
            LayerListWidget *layerList,
            LayerListWidgetItem *layerItm,
            QImage img,
            QRect uptRect);
    void frameRenderingChanged();
    void currentFrameChanged();

public slots:
    FrameListWidgetItem * addNewFrame(FrameListWidget *parent = Q_NULLPTR);
    FrameListWidgetItem * duplicateFrame(FrameListWidget *parent = Q_NULLPTR,
                                         FrameWidget *frame = Q_NULLPTR);
    FrameListWidgetItem * removeFrame(FrameListWidget *parent = Q_NULLPTR,
                                      FrameListWidgetItem *itm = Q_NULLPTR);

protected:
    void dropEvent(QDropEvent *event) override;
    void resizeEvent(QResizeEvent *event) override;

private:
    QSize getItemSize() const;
    QSize getFrameSize() const;
    QSize getScaledSizeWithSeparation(QSize size, int separation) const;

    bool isLeftToRight() const;
};

#endif // SMARTLISTWIDGET_H
