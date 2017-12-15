/*
 * storewindow.h
 *
 * Anthony Chyr (u0627375)
 * Carlos Enrique Guerra Chan (u0847821)
 * Elliot C Carr-Lee (u0549837)
 * Ismael Kadilo Wa Ngoie (u1120347)
 * Jared Earl (u1120117)
 * Kameron Service (u0963620)
 * Wesley Barth (u0488618)
 *
 * CS 3505 Software Practices II Johnson
 * A8: An Agile Educational Application
 */

#ifndef STOREWINDOW_H
#define STOREWINDOW_H

#include "creditcarddialog.h"
#include "storemodel.h"

#include <QtWidgets>

#include <vector>

namespace Ui {
class StoreWindow;
}

class StoreWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit StoreWindow(StoreModel *model, QWidget *parent = 0);
    ~StoreWindow();

private slots:
    void initFromModel(
            std::vector<Skin> allSkins,
            QString defaultSkinResource,
            int initialZCoins);

    void newZCoins(int zcoins);
    void zCoinsAddedByModel(int zcoins);
    void notEnoughZCoins();

    void skinClickedByUser();
    void skinFoundByModel(QString resource, int coins);
    void skinNotFoundByModel();
    void skinResourceStringReciever(QString resources);

    void cheaterCannonBallActivated(int coins);
    void cheaterCannonBallDeactivated();

    void on_newSkinButton_clicked();
    void on_buyZCoinsButton_clicked();
    void on_upgradeCannonButton_clicked();

    void on_solutionButton_clicked();

signals:
    void init();

    void needNewSkin();

    void upgradeCannon();
    void cheaterCannonBallToggled(bool isActive);

    void solutionRequested();
    void doSolution();

    void addZCoinsToModel(int zcoins);

    void skinSelected(QString resource);
    void needSkinResourceString(int index);
    void resourceString(QString resource);

private:
    const QString BUTTONSELECTEDSTYLE = "border: 2px solid #ff0000";
    const QString BUTTONUNSELECTEDSTYLE = "border: 1px solid #000000";
    const int BUTTONWIDTH = 100;
    const int BUTTONHEIGHT = 100;
    const int STARTINGCOINS = 100;

    Ui::StoreWindow *ui;
    CreditCardDialog *prompt;

    QList<QPushButton *> skins;
    int selectedButtonIndex;

    QPushButton * createNewButton(int index, QString iconResource);
    void addCatalogItem(QWidget* catalogWidget);
    void updateCoins(int coins);
    void addSkinToInventory(QString skinResource);
};

#endif // STOREWINDOW_H
