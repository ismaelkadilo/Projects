/*
 * framelistwidgetitem.cpp
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

#ifndef FRAMELISTWIDGETITEM_H
#define FRAMELISTWIDGETITEM_H

#include <QListWidgetItem>

class FrameListWidgetItem : public QListWidgetItem
{
public:
    explicit FrameListWidgetItem(
            QListWidget *parent = Q_NULLPTR,
            int type = Type);
};

#endif // SMARTLISTWIDGETITEM_H
