/*
 * layerwidget.h
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

#ifndef LAYERWIDGET_H
#define LAYERWIDGET_H

#include "constants.h"
#include "imageviewerwidget.h"

class LayerWidget : public ImageViewerWidget
{
    Q_OBJECT

public:
    explicit LayerWidget(
            EditorWindow *controller,
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());

    bool isLayerVisible() const;
    QImage getImage() const;
    QString getText() const;

    QImage * imagePtr();

signals:
    void layerVisibilityChanged(bool visibility);
    void imageChanged(QImage img, QRect rect);
    void textChanged(QString text);

public slots:
    void setLayerVisibility(bool visbility);
    void setImage(QImage img, QRect uptRect = QRect());
    void setText(const QString &text);

private:
    // gui elements + internal states
    QHBoxLayout *layout;

    QCheckBox *checkbox;
    QLabel *preview;
    QLabel *label;

    // internal states
    EditorWindow *controller;
    QImage image;
};

#endif // LAYERWIDGET_H
