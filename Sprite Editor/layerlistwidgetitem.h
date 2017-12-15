/*
 * layerlistwidgetitem.h
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

#ifndef LAYERLISTWIDGETITEM_H
#define LAYERLISTWIDGETITEM_H

#include <QListWidgetItem>

class LayerListWidgetItem : public QListWidgetItem
{
public:
    explicit LayerListWidgetItem(QListWidget *parent = Q_NULLPTR,
                                 int type = QListWidgetItem::Type);
};

#endif // LAYERLISTWIDGETITEM_H
