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

#ifndef CANVASWIDGET_H
#define CANVASWIDGET_H

#include "constants.h"
#include "imageviewerwidget.h"

class CanvasWidget : public ImageViewerWidget
{
    Q_OBJECT

public:
    explicit CanvasWidget(
            EditorWindow *controller,
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());

    QPoint getStartPos() const;
    QPoint getPrevPos() const;
    QPoint getCurrentPos() const;

    QImage getPreviewImage() const;
    QImage getClearedPreviewImage() const;

    QImage getTopOverlayImage() const;
    QImage getClearedTopOverlayImage() const;

public slots:
    void setStartPos(QPoint pos);
    void setPrevPos(QPoint pos);
    void setCurrentPos(QPoint pos);

    void setPreviewImage(QImage preview, QRect uptRect = QRect());
    void clearPreviewImage();

    void setTopOverlayImage(QImage topOverlay, QRect uptRect = QRect());
    void clearTopOverlayImage();

    void imageUpdate(QRect uptRect = QRect());

protected:
    void mousePressEvent(QMouseEvent *event) override;
    void mouseMoveEvent(QMouseEvent *event) override;
    void mouseReleaseEvent(QMouseEvent *event) override;
    void paintEvent(QPaintEvent *event) override;

private:
    QPoint startPos;
    QPoint prevPos;
    QPoint currentPos;

    QImage previewImage;
    QImage topOverlayImage;

    QPen pen;
    QBrush brush;

    void handleMouseEvent(
            QMouseEvent *event,
            void(ToolManagerWidget::*handler)
                (QMouseEvent *, EditorWindow *, CanvasWidget *));
};

#endif // CANVASWIDGET_H
