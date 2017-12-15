/*
 * smartlistwidget.h
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

#ifndef SMARTLISTWIDGET_HPP
#define SMARTLISTWIDGET_HPP

#include"editorwindow.h"

#include <QListWidget>

template<class I, class W>
class SmartListWidget : public QListWidget
{
public:
    explicit SmartListWidget(
            EditorWindow *controller,
            QWidget *parent = Q_NULLPTR) :
        controller(controller),
        QListWidget(parent)
    {
    }

    I * item(int row) const
    {
        return dynamic_cast<I *>(QListWidget::item(row));
    }

    W * itemWidget(I *item) const
    {
        return qobject_cast<W *>(QListWidget::itemWidget(item));
    }

    I * takeItem(int row)
    {
        return dynamic_cast<I *>(QListWidget::takeItem(row));
    }

    W * widget(int row) const
    {
        return itemWidget(item(row));
    }

    I * currentItem() const
    {
        return item(currentRow());
    }

    W * currentWidget() const
    {
        return widget(currentRow());
    }

    QList<W *> widgetList() const
    {
        QList<W *> list;
        for (int i = 0; i < count(); ++i)
            list.push_back(widget(i));
        return list;
    }

    virtual QList<W *> renderOrderWidgetList() const = 0;

protected:
    EditorWindow *controller;
};

#endif // SMARTLISTWIDGET_H
