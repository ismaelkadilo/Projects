/*
 * editorwindow.h
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

#ifndef EDITORWINDOW_H
#define EDITORWINDOW_H

#include "constants.h"

#include <QtWidgets>

// forward declarations
class CanvasWidget;
class FrameListWidget;
class FrameWidget;
class LayerListWidget;
class LayerWidget;
class ToolManagerWidget;

class EditorWindow : public QWidget
{
    Q_OBJECT

public:
    explicit EditorWindow(
            QSize size,
            ToolManagerWidget *tool,
            QWidget *parent = Q_NULLPTR,
            QImage::Format imageFormat = constants::DEFAULT_IMAGE_FORMAT,
            Qt::WindowFlags flags = Qt::WindowFlags());
    ~EditorWindow();

    QSize getSpriteSize() const;
    QImage::Format getSpriteImageFormat() const;
    QRgb getSpriteBackgroundFill() const;

    QFileInfo getFileInfo() const;

    FrameListWidget * getFrameListWidget() const;
    FrameWidget * getCurrentFrameWidget() const;
    LayerListWidget * getCurrentLayerListWidget() const;
    LayerWidget * getCurrentLayerWidget() const;

    ToolManagerWidget * getToolManager() const;

    void ensureSelectedLayer();

signals:
    void spriteSizeChanged(QSize size);
    void spriteImageFormatChanged(QImage::Format format);
    void spriteBackgroundFillChanged(QRgb fill);

public slots:
    void setSpriteSize(QSize size);
    void setSpriteImageFormat(QImage::Format format);
    void setSpriteBackgroundFill(QRgb fill);

    void setFileInfo(QFileInfo info);

private:
    // gui elements
    QScrollArea *scrollArea;
    QWidget *scrollAreaWidgetContents;
    CanvasWidget *canvas;

    QSpinBox *zoomSpinBox;
    QSlider *zoomSlider;
    QPushButton *zoomOutPushButton;
    QPushButton *zoomInPushButton;
    QPushButton *zoomNormalPushButton;
    QPushButton *zoomFitPushButton;

    QGridLayout *mainLayout;
    QGridLayout *scrollAreaLayout;

    // internal states
    QSize spriteSize;
    QImage::Format spriteImageFormat;
    QRgb spriteBackgroundFill;

    QFileInfo fileInfo;

    FrameListWidget *frameListWidget;
    ToolManagerWidget *toolManager;

    // static members
    static int numConstructed;

    // methods
    void createGui();
};

#endif // EDITORWINDOW_H
