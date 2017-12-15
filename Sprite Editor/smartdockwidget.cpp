/*
 * smartdockwidget.cpp
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

#include "smartdockwidget.h"

/*
 * public
 */
SmartDockWidget::SmartDockWidget(const QString &title,
                                 QWidget *parent,
                                 Qt::WindowFlags flags) :
    QDockWidget(title, parent, flags),
    resizeEventHandler([](SmartDockWidget *, QResizeEvent *){}),
    addWidgetHandler([](SmartDockWidget *, QWidget *){}),
    removeWidgetHandler([](SmartDockWidget *, QWidget *){})
{
}

void SmartDockWidget::setResizeEventHandler(
        std::function<void(SmartDockWidget *, QResizeEvent *)> closure)
{
    resizeEventHandler = closure;
}

void SmartDockWidget::setAddWidgetHandler(
        std::function<void(SmartDockWidget *, QWidget *)> closure)
{
    addWidgetHandler = closure;
}

void SmartDockWidget::setRemoveWidgetHandler(
        std::function<void(SmartDockWidget *,QWidget *)> closure)
{
    removeWidgetHandler = closure;
}

void SmartDockWidget::addWidget(QWidget *widget)
{
    addWidgetHandler(this, widget);
}

void SmartDockWidget::removeWidget(QWidget *widget)
{
    removeWidgetHandler(this, widget);
}

/*
 * protected
 */
void SmartDockWidget::resizeEvent(QResizeEvent *event)
{
    resizeEventHandler(this, event);
}
