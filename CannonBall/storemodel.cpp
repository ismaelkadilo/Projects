/*
 * storemodel.cpp
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

#include "storemodel.h"

#include "time.h"
#include <vector>

using namespace std;

/*
 * public
 */
StoreModel::StoreModel(QObject *parent) :
    QObject(parent),
    zcoins(100)
{
    // seed the random digit generator
    srand(time(nullptr));

    // make a quick helpder function
    auto addSkin = [&](vector<Skin> &skinVec, QString path){
        skinVec.push_back(Skin(QImage(path), path));
    };

    // load skins
    QDirIterator it(":assets/skins", QDirIterator::Subdirectories);
    while (it.hasNext()) addSkin(skinsNotOwned, it.next());

    // default skin
    addSkin(skinsOwned, ":/assets/JhonsonBall_scaled.png");

    // all the skins
    for(int i = 0; i < skinsNotOwned.size(); i++)
        skinsAll.push_back(skinsNotOwned[i]);
    for(int i = 0; i < skinsOwned.size(); i++)
        skinsAll.push_back(skinsOwned[i]);
}

/*
 * public slots
 */
void StoreModel::initModel()
{
    emit initInfo(skinsAll, skinsOwned[0].resource, zcoins);
}

void StoreModel::newSkinNeeded()
{
    // not enough money
    if (zcoins < 100)
    {
        emit notEnoughZCoinsInModel();
        return;
    }

    // user bought all the skins
    if (skinsNotOwned.size() <= 0)
    {
        emit noNewSkins();
        return;
    }

    // buy a skin randomly from the list of skins not owned
    zcoins -= 100;
    emit zcoinsAdded(zcoins);

    int randIndex = rand() % skinsNotOwned.size();

    // get the randomly selected skin
    skinsOwned.push_back(skinsNotOwned[randIndex]);
    emit newSkinFound(skinsNotOwned[randIndex].resource, zcoins);

    // remove the skins from the list of skins not owned
    skinsNotOwned.erase(skinsNotOwned.begin() + randIndex);
}

void StoreModel::doCheater()
{
    // the user doesn't have enough money to buy a cheater ball
    if (zcoins < 1000)
    {
        emit notEnoughZCoinsInModel();
        return;
    }

    // buy the cheater ball
    zcoins -= 1000;
    emit zcoinsAdded(zcoins);
    emit cheaterOn(zcoins);
}

void StoreModel::doSolution()
{
    // the user doesn't have enough money to buy the solution
    if (zcoins < 5000)
    {
        emit notEnoughZCoinsInModel();
        return;
    }

    // buy the solution
    zcoins -= 5000;
    emit zcoinsAdded(zcoins);
    emit solutionRequestAccepted();
}

void StoreModel::newZCoinsForModel(int coins)
{
    zcoins += coins;
    emit zcoinsAdded(zcoins);
}

void StoreModel::resourceRequest(int index)
{
    emit resourceFound(skinsOwned[index].resource.toUtf8().constData());
}
