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

#ifndef FRAMEWIDGET_H
#define FRAMEWIDGET_H

#include "imageviewerwidget.h"
#include "layerlistwidget.h"

class FrameWidget : public ImageViewerWidget
{
    Q_OBJECT

public:
    explicit FrameWidget(
            EditorWindow *controller,
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());
    ~FrameWidget();

    LayerListWidget * getLayerListWidget() const;

    QImage getFlattenedImage() const;

signals:
    void imageChanged(
            LayerListWidget *layerList,
            LayerListWidgetItem *layerItm,
            QImage img,
            QRect uptRect);
    void frameRenderingChanged();

protected:
    void paintEvent(QPaintEvent *event) override;

private:
    LayerListWidget *layerListWidget;
};

#endif // FRAMEWIDGET_H
