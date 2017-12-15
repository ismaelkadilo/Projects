/*
 * toolmanagerwidget.h
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

#ifndef TOOLMANAGERWIDGET_H
#define TOOLMANAGERWIDGET_H

#include "constants.h"
#include "canvaswidget.h"
#include "editorwindow.h"

#include <functional>

typedef std::function<void(QMouseEvent *,
                           EditorWindow *,
                           CanvasWidget *,
                           ToolManagerWidget *)> MouseEventHandler;

class ToolManagerWidget : public QWidget
{
    Q_OBJECT

public:
    explicit ToolManagerWidget(
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());

    void handleMousePress(QMouseEvent *event,
                          EditorWindow *controller,
                          CanvasWidget *canvas);
    void handleMouseMove(QMouseEvent *event,
                         EditorWindow *controller,
                         CanvasWidget *canvas);
    void handleMouseRelease(QMouseEvent *event,
                            EditorWindow *controller,
                            CanvasWidget *canvas);

    MouseEventHandler getMousePressHandler() const;
    MouseEventHandler getMouseMoveHandler() const;
    MouseEventHandler getMouseReleaseHandler() const;

    void setMouseEventHandler(
            MouseEventHandler mousePressClosure,
            MouseEventHandler mouseMoveClosure,
            MouseEventHandler mouseReleaseClosure);

    void configurePainter(QPainter *painter) const;
    QRect addPenWidth(QRect rect) const;

    QBrush getBrush() const;
    QPen getPen() const;

signals:

public slots:
    void pickBrushColor();
    void pickPenColor();
    void pickPenWidth();

    void setBrush(const QBrush &brush);
    void setPen(const QPen &pen);

private:
    // gui
    QPushButton *brushColorPushButton;
    QPushButton *penColorPushButton;
    QPushButton *penWidthPushButton;

    // internal states
    MouseEventHandler mousePressHandler;
    MouseEventHandler mouseMoveHandler;
    MouseEventHandler mouseReleaseHandler;

    QBrush toolManagerBrush;
    QPen toolManagerPen;
};

namespace toolmanager
{
const MouseEventHandler NO_OP_MOUSE_EVENT_HANLDER = [](
        QMouseEvent *, EditorWindow *, CanvasWidget *, ToolManagerWidget *){};
}

#endif // TOOLMANAGER_H
