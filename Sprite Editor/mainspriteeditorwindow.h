/*
 * mainspriteeditorwindow.h
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

#ifndef MAINSPRITEEDITORWINDOW_H
#define MAINSPRITEEDITORWINDOW_H

#include "constants.h"
#include "editorwindow.h"
#include "framelistwidget.h"
#include "layerlistwidget.h"
#include "smartdockwidget.h"
#include "toolmanagerwidget.h"

#include <QtWidgets>

#include <functional>

class MainSpriteEditorWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainSpriteEditorWindow(
            QWidget *parent = Q_NULLPTR,
            Qt::WindowFlags = Qt::WindowFlags());
    ~MainSpriteEditorWindow();

    EditorWindow * getCurrentEditorWindow() const;

    FrameListWidget * getCurrentFrameListWidget() const;
    LayerListWidget * getCurrentLayerListWidget() const;

    ToolManagerWidget * getToolManagerWidget() const;

    int getPreviewFramesPerSecond() const;

public slots:
    void newFile();
    void openFile();
    void saveFile();
    void saveAsFile();
    void exportAnimatedGif();
    void exportAnimatedPng();

    void setCurrentFrameListWidget(FrameListWidget *frameListWidget);
    void setCurrentLayerListWidget(LayerListWidget *layerListWidget);

    void setToolManagerWidget(ToolManagerWidget *toolManager);

    void clearPreview();

private slots:
    void initialized();

private:
    // gui
    QMdiArea *mdiArea;

    QMenu *fileMenu;
    QMenu *paintMenu;
    QMenu *frameMenu;
    QMenu *layerMenu;
    QMenu *previewMenu;
    QMenu *viewMenu;
    QMenu *windowMenu;
    QMenu *helpMenu;

    SmartDockWidget *paintDockWidget;
    SmartDockWidget *historyDockWidget;
    SmartDockWidget *frameDockWidget;
    SmartDockWidget *previewDockWidget;
    SmartDockWidget *layerDockWidget;

    QAction *scribbleTool; // initial tool

    QTimer *previewTimer;
    QSpinBox *frameRateSpinBox;
    QLabel *preview;

    // internal states
    FrameListWidget *currentFrameListWidget;
    LayerListWidget *currentLayerListWidget;

    ToolManagerWidget *toolManagerWidget;

    // helper methods
    void createMainWindow();
    void createActions();

    void generatePaintDock();
    void generateHistoryDock();
    void generateFrameDock();
    void generatePreviewDock();
    void generateLayerDock();

    void generateToolBar();

    template<class T> void generateViewMenuItemsFromType(
            const QString &typeName);

    EditorWindow * makeEditorWindow(QSize size, QWidget *parent = Q_NULLPTR);

    std::function<void()> makeScribbleMouseEventHandler(
            std::function<void(QPainter &, QRect)> paintClosure);

    std::function<void()> makeShapeMouseEventHandler(
            std::function<void(QPainter &, QRect)> paintClosure);

    QPushButton * makeToolButton(QAction *action, QWidget *parent);

    std::function<void(SmartDockWidget *, QResizeEvent *)> \
        makeResizeEventHandlerForDocksWithBigCentralWidget(
            QWidget *(MainSpriteEditorWindow::*getWidget)());
    std::function<void(SmartDockWidget *, QWidget *)> \
        makeAddWidgetHandlerForDocksWithBigCentralWidget(
            QGridLayout *layout);
    std::function<void(SmartDockWidget *, QWidget *)> \
        makeRemoveWidgetHandlerForDocksWithBigCentralWidget(
            QGridLayout *layout);
};

#endif // MAINSPRITEEDITORWINDOW_H
