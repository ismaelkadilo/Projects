/*
 * smartdockwidget.h
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

#ifndef SMARTDOCKWIDGET_H
#define SMARTDOCKWIDGET_H

#include <functional>

#include <QDockWidget>

class SmartDockWidget : public QDockWidget
{
    Q_OBJECT

public:
    explicit SmartDockWidget(
            const QString &title,
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags flags = Qt::WindowFlags());

    void setResizeEventHandler(
            std::function<void(SmartDockWidget *, QResizeEvent *)> closure);

    void setAddWidgetHandler(
            std::function<void(SmartDockWidget *, QWidget *)> closure);

    void setRemoveWidgetHandler(
            std::function<void(SmartDockWidget *, QWidget *)> closure);

    void addWidget(QWidget *widget);
    void removeWidget(QWidget *widget);

protected:
    void resizeEvent(QResizeEvent *event) override;

private:
    std::function<void(SmartDockWidget *, QResizeEvent *)> resizeEventHandler;
    std::function<void(SmartDockWidget *, QWidget *)> addWidgetHandler;
    std::function<void(SmartDockWidget *, QWidget *)> removeWidgetHandler;
};

#endif // SMARTDOCKWIDGET_H
