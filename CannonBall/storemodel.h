/*
 * storemodel.h
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

#ifndef STOREMODEL_H
#define STOREMODEL_H

#include <QDirIterator>
#include <QImage>
#include <QObject>

#include <vector>

struct Skin
{
public:
    QImage skin;
    QString resource;

    Skin(QImage image, QString res)
    {
        skin = image;
        resource = res;
    }
};

class StoreModel : public QObject
{
    Q_OBJECT

public:
    explicit StoreModel(QObject *parent = nullptr);

public slots:
    void initModel();

    void newSkinNeeded();
    void doCheater();
    void doSolution();

    void newZCoinsForModel(int coins);
    void resourceRequest(int index);

signals:
    void initInfo(
            std::vector<Skin> allSkins,
            QString defaultSkinResource,
            int initialZCoins);
    void resourceFound(QString resource);

    void newSkinFound(QString resource, int zcoins);
    void noNewSkins();

    void cheaterOn(int zcoins);
    void cheaterOff();

    void solutionRequestAccepted();

    void notEnoughZCoinsInModel();
    void zcoinsAdded(int zcoins);

private:
    long zcoins;
    std::vector<Skin> skinsOwned;
    std::vector<Skin> skinsNotOwned;
    std::vector<Skin> skinsAll;
};

#endif // STOREMODEL_H
