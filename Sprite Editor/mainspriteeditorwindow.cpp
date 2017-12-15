/*
 * mainspriteeditorwindow.cpp
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

#include "mainspriteeditorwindow.h"

#include "gif.h"

#include <algorithm>

using namespace constants;
using namespace std;
using namespace toolmanager;

/*
 * public
 */
MainSpriteEditorWindow::MainSpriteEditorWindow(
        QWidget *parent,
        Qt::WindowFlags flags) :
    QMainWindow(parent, flags),
    currentFrameListWidget(Q_NULLPTR),
    currentLayerListWidget(Q_NULLPTR),
    toolManagerWidget(new ToolManagerWidget(this))
{
    // overall style sheet
    setStyleSheet(
        "QWidget[textured=true]\n"
        "{\n"
        "    background: url(:/images/transparentBackgroundTexture.png);\n"
        "    background-repeat: repeat-xy;\n"
        "}\n"
        "\n"
        "QWidget[boxed=true]\n"
        "{\n"
        "    border: 1px solid #7fcccccc;\n"
        "}\n"
    );

    // create gui elements
    createMainWindow();
    createActions();

    // generate gui elements and connections from existing gui elements
    // requires core actions to exist first
    generatePaintDock();
    generateHistoryDock();
    generateFrameDock();
    generatePreviewDock();
    generateLayerDock();

    // requires actions and docks to exist first
    generateViewMenuItemsFromType<QDockWidget>("Dock");

    // requires menus to be mostly done
    generateToolBar();

    // requires toolbars to be populated
    generateViewMenuItemsFromType<QToolBar>("Toolbar");

    // further initializations
    emit scribbleTool->triggered(); // initial tool
    QTimer::singleShot(100, this, &MainSpriteEditorWindow::initialized);
}

MainSpriteEditorWindow::~MainSpriteEditorWindow()
{
}

EditorWindow * MainSpriteEditorWindow::getCurrentEditorWindow() const
{
    QMdiSubWindow *subWindow = mdiArea->currentSubWindow();
    return subWindow ? qobject_cast<EditorWindow *>(subWindow->widget())
                     : Q_NULLPTR;
}

FrameListWidget * MainSpriteEditorWindow::getCurrentFrameListWidget() const
{
    return currentFrameListWidget;
}

LayerListWidget * MainSpriteEditorWindow::getCurrentLayerListWidget() const
{
    return currentLayerListWidget;
}

ToolManagerWidget * MainSpriteEditorWindow::getToolManagerWidget() const
{
    return toolManagerWidget;
}

int MainSpriteEditorWindow::getPreviewFramesPerSecond() const
{
    return frameRateSpinBox->value();
}

/*
 * public slots
 */
void MainSpriteEditorWindow::newFile()
{
    // ask user to specify new project
    QDialog dialog(this);
    dialog.setWindowTitle(tr("New File"));

    QFormLayout form(&dialog);

    QSpinBox widthSpinBox;
    widthSpinBox.setValue(DEFAULT_SPRITE_SIZE.width());
    widthSpinBox.setRange(MIN_SPRITE_SIZE.width(), MAX_SPRITE_SIZE.width());

    QSpinBox heightSpinBox;
    heightSpinBox.setValue(DEFAULT_SPRITE_SIZE.height());
    heightSpinBox.setRange(MIN_SPRITE_SIZE.height(), MAX_SPRITE_SIZE.height());

    QDialogButtonBox buttonBox(QDialogButtonBox::Ok | QDialogButtonBox::Cancel,
                               Qt::Horizontal, &dialog);
    connect(&buttonBox, &QDialogButtonBox::accepted,
            &dialog, &QDialog::accept);
    connect(&buttonBox, &QDialogButtonBox::rejected,
            &dialog, &QDialog::reject);

    form.addRow(tr("Width in pixels:"), &widthSpinBox);
    form.addRow(tr("Height in pixels:"), &heightSpinBox);
    form.addRow(&buttonBox);

    // create the new project
    if (QDialog::Accepted == dialog.exec())
    {
        EditorWindow *editorWindow = makeEditorWindow(
                    QSize(widthSpinBox.value(), heightSpinBox.value()), this);

        // add it to the main window
        mdiArea->addSubWindow(editorWindow);
        editorWindow->showMaximized();
    }
}

void MainSpriteEditorWindow::openFile()
{
    EditorWindow *editorWindow = Q_NULLPTR;
    try
    {
        // get the file
        QString path = QFileDialog::getOpenFileName(
                    this, tr("Open File"), QDir::homePath(),
                    tr("Sprite (*.ssp);;All Files (*)"));
        if (path.isNull()) return; // nothing to open

        QFile file(path);
        if (!file.open(QIODevice::ReadOnly | QIODevice::Text))
            throw tr("File not readable.");

        QTextStream in(&file);

        // read the size of the sprite
        QString line = in.readLine();
        if (line.isEmpty()) throw tr("Could not find sprite size.");

        QStringList sizeStr = line.split(' ', QString::SkipEmptyParts);
        if (sizeStr.count() != 2) throw tr("Sprite size was not two numbers.");

        bool hOk = false, wOk = false;
        QSize size(sizeStr.at(1).toInt(&hOk), sizeStr.at(0).toInt(&wOk));
        if (!hOk || !wOk
                || size.width() < MIN_SPRITE_SIZE.width()
                || size.width() > MAX_SPRITE_SIZE.width()
                || size.height() < MIN_SPRITE_SIZE.height()
                || size.height() > MAX_SPRITE_SIZE.height())
            throw tr("Invalid sprite size.");

        // create the editor window, enough information and checks
        editorWindow = makeEditorWindow(size, this);
        editorWindow->setFileInfo(QFileInfo(path));

        FrameListWidget *frameList = editorWindow->getFrameListWidget();

        // only non-critical errors from this point on
        bool isInconsistent = false;

        // read the number of frames
        if (!in.readLineInto(&line)) isInconsistent = true;

        bool numFrameOk = false;
        const int NUM_FRAMES = line.toInt(&numFrameOk);
        if (!numFrameOk) isInconsistent = true;

        // read the frames
        int lineNum = 0;
        QImage *img;
        while (in.readLineInto(&line))
        {
            // new frame
            if (!(lineNum % size.height()))
            {
                FrameListWidgetItem *frameItem = frameList->addNewFrame();
                FrameWidget *frame = frameList->itemWidget(frameItem);
                frameList->setCurrentItem(frameItem);

                LayerListWidget *layerList = frame->getLayerListWidget();
                LayerListWidgetItem *layerItem = layerList->addNewLayer();
                LayerWidget *currentLayer = layerList->itemWidget(layerItem);
                img = currentLayer->imagePtr(); // mutate the images directly
            }

            // each row lists pixels one after the other separated with spaces
            // each pixel's color channels is represented by an unsigned int
            // encoded as a string separated with space in the order: R G B A
            QStringList vals = line.split(' ', QString::SkipEmptyParts);
            if (4 * size.width() != vals.count()) isInconsistent = true;

            int col = 0;
            QRgb pixel = 0x0; // 4 byte unsigned int formatted #AARRBBGG
            for (QString v : vals)
            {
                bool ok = false;
                uint rawVal = v.toUInt(&ok);
                if (!ok || rawVal > 255) isInconsistent = true;

                uint8_t channelVal = static_cast<uint8_t>(rawVal);
                switch (col % 4)
                {
                case 0: // red
                    pixel |= channelVal << 16;
                    break;
                case 1: // green
                    pixel |= channelVal << 8;
                    break;
                case 2: // blue
                    pixel |= channelVal << 0;
                    break;
                case 3: // alpha
                    pixel |= channelVal << 24;

                    // set the pixel
                    img->setPixel(col/4, lineNum % size.height(), pixel);

                    // reset for the next pixel
                    pixel = 0x0;
                    break;
                }

                // don't go past the last pixel in the image's row
                if (4 * size.width() <= ++col) break;
            }

            // iterate number of lines
            ++lineNum;
        }

        // cleanly finish reading the file
        file.close();

        // set the images (to itself) to get the signals to propagate to update
        // the canvas and frame
        for (FrameWidget *frame : frameList->renderOrderWidgetList())
            for (LayerWidget *layer
                    : frame->getLayerListWidget()->renderOrderWidgetList())
                layer->setImage(layer->getImage());

        // reset the current frame to be the first
        if (frameList->count()) frameList->setCurrentRow(0);

        // check whether number of frames actually matches what was specified
        if (size.height() * NUM_FRAMES != lineNum) isInconsistent = true;

        // warn the user that the file may be inconsistent
        if (isInconsistent)
        {
            QMessageBox::warning(this, tr("Error opening file"),
                    tr("The file was inconsistent.\n"
                       "An attempt was made to recover the file."));
        }

        // add it to the main window
        mdiArea->addSubWindow(editorWindow);
        editorWindow->showMaximized();
    }
    catch (QString msg)
    {
        QMessageBox::critical(this, tr("Error opening file"),
                              tr("Could not open file. %1").arg(msg));
        delete editorWindow; // cleanup if necessary
    }
    catch (...)
    {
        QMessageBox::critical(this, tr("Error opening file"),
                              tr("A critical error occured.\n"
                                 "File could not be properly opened."));
    }
}

void MainSpriteEditorWindow::saveFile()
{
    try
    {
        // get the current editor window
        EditorWindow *editorWindow = getCurrentEditorWindow();
        if (!editorWindow) return; // nothing to save

        // check if the editor window has an associated file
        QFileInfo fileInfo = editorWindow->getFileInfo();

        if (fileInfo.absoluteFilePath().isEmpty())
        {
            // there isn't an existing path to save to, ask for one
            saveAsFile();
            return;
        }

        // open the file
        QFile file(fileInfo.absoluteFilePath());
        if (!file.open(QIODevice::WriteOnly | QIODevice::Text))
            throw tr("File not writable.");

        QTextStream out(&file);

        // write the sprite size
        QSize size = editorWindow->getSpriteSize();
        out << size.height() << ' ' << size.width() << '\n';

        // write the number of frames
        FrameListWidget *frameList = editorWindow->getFrameListWidget();
        out << frameList->count() << '\n';

        // write the frames
        for (FrameWidget *frame : frameList->renderOrderWidgetList())
        {
            QImage img = frame->getFlattenedImage();
            for (int r = 0; r < img.height(); ++r)
            {
                for (int c = 0; c < img.width(); ++c)
                {
                    // .ssp is formatted: R G B A separated with spaces
                    // each pixels on the same row are placed one after the
                    // other with a space in between
                    // rows and frames are separated with a singe new line
                    QColor pixel = img.pixelColor(c, r);
                    out << pixel.red() << ' '
                        << pixel.green() << ' '
                        << pixel.blue() << ' '
                        << pixel.alpha() << (img.width() != c+1 ? ' ' : '\n');
                }
            }
        }

        // cleanly finish writing the file
        out.flush();
        file.close();
    }
    catch (QString msg)
    {
        QMessageBox::critical(this, tr("Error saving file"),
                              tr("Could not save file. %1").arg(msg));
    }
    catch (...)
    {
        QMessageBox::critical(this, tr("Error saving file"),
                              tr("A critical error occured.\n"
                                 "File could not be properly saved."));
    }
}

void MainSpriteEditorWindow::saveAsFile()
{
    // get the current editor window
    EditorWindow *editorWindow = getCurrentEditorWindow();
    if (!editorWindow) return; // nothing to save

    // get the file information if there is any
    QFileInfo fileInfo = editorWindow->getFileInfo();

    // propose a path based on existing file information
    // if there isn't, use the user's home directory
    QString proposedPath = fileInfo.absoluteFilePath();
    if (proposedPath.isEmpty()) proposedPath = QDir::homePath();

    // ask the user to specify the path
    QString path = QFileDialog::getSaveFileName(
                this, tr("Save File"), proposedPath,
                tr("Sprite (*.ssp);;All Files (*)"));
    if (path.isNull()) return; // nothing to save

    // set the new path
    editorWindow->setFileInfo(QFileInfo(path));

    // save the file
    saveFile();
}

void MainSpriteEditorWindow::exportAnimatedGif()
{
    // FUTURE: rewrite gif.h and split off function into helpers
    try
    {
        // get the current editor window
        EditorWindow *editorWindow = getCurrentEditorWindow();
        if (!editorWindow) return; // nothing to export

        // get the file path
        QString path = QFileDialog::getSaveFileName(
                    this, tr("Export Sprite"), QDir::homePath(),
                    tr("GIF (*.gif);;All Files (*)"));
        if (path.isNull()) return; // nothing to export

        // gif parameters
        QSize size = editorWindow->getSpriteSize();
        int frameRate = getPreviewFramesPerSecond(); // 0 fps = max delay
        uint32_t delay = frameRate ? 100/frameRate : static_cast<uint32_t>(-1);

        GifWriter gifWriter;
        GifBegin(&gifWriter,
                 path.toUtf8().data(),
                 size.width(),
                 size.height(),
                 delay);

        for (FrameWidget *frame
                : editorWindow->getFrameListWidget()->renderOrderWidgetList())
        {
            // convert the image into a pixel array
            QImage img = frame->getFlattenedImage();
            uint8_t *pixelArray = new uint8_t[4 * img.width() * img.height()];
            for (int r = 0; r < img.height(); ++r)
            {
                for (int c = 0; c < img.width(); ++c)
                {
                    QColor pixel = img.pixelColor(c, r);
                    size_t index = 4 * (img.width() * r + c);
                    pixelArray[index] = static_cast<uint8_t>(pixel.red());
                    pixelArray[index+1] = static_cast<uint8_t>(pixel.green());
                    pixelArray[index+2] = static_cast<uint8_t>(pixel.blue());
                    pixelArray[index+3] = static_cast<uint8_t>(pixel.alpha());
                }
            }

            // write the frame
            GifWriteFrame(&gifWriter,
                          pixelArray,
                          img.width(),
                          img.height(),
                          delay);

            // cleanup
            delete[] pixelArray;
        }

        GifEnd(&gifWriter);
    }
    catch (QString msg)
    {
        QMessageBox::critical(this, tr("Error exporting file"),
                              tr("Could not export file. %1").arg(msg));
    }
    catch (...)
    {
        QMessageBox::critical(this, tr("Error exporing file"),
                              tr("A critical error occured.\n"
                                 "File could not be properly exported."));
    }
}

void MainSpriteEditorWindow::exportAnimatedPng()
{
    // FUTURE: package this into its own library and splitoff this into helpers
    try
    {
        // get the current editor window
        EditorWindow *editorWindow = getCurrentEditorWindow();
        if (!editorWindow) return; // nothing to export

        // get the file path
        QString path = QFileDialog::getSaveFileName(
                    this, tr("Export Sprite"), QDir::homePath(),
                    tr("PNG (*.png);;All Files (*)"));
        if (path.isNull()) return; // nothing to export

        // open the file
        QFile file(path);
        if (!file.open(QIODevice::WriteOnly))
            throw tr("File not writable.");

        // create a base image and use it to get the png chunks
        QImage baseImage(editorWindow->getSpriteSize(),
                         editorWindow->getSpriteImageFormat());
        baseImage.fill(TRANSPARENT_BLACK);
        QBuffer baseBuffer(this);
        baseImage.save(&baseBuffer, "PNG");

        // png helpers
        const QByteArray IHDR("IHDR");
        const QByteArray IDAT("IDAT");
        const QByteArray acTL("acTL");
        const QByteArray fcTL("fcTL");
        const QByteArray fdAT("fdAT");
        const QByteArray IEND("IEND");

        auto getChunk = [&](
                const QByteArray &img, const QByteArray &hdrType)->QByteArray{
            int currentPos = 8; // skip over png signature
            while (currentPos < img.length())
            {
                // get the length of the chunk's data
                QDataStream tempStream(img.mid(currentPos, 4));
                quint32 chunkDataLength;
                tempStream >> chunkDataLength;

                // get the total chunk's length
                quint32 totalChunkLength = 4 + 4 + chunkDataLength + 4;

                // find the chunk
                QByteArray header = img.mid(currentPos+4, 4);
                if (hdrType == header)
                {
                    return img.mid(currentPos, totalChunkLength);
                }

                currentPos += totalChunkLength;
            }
            return QByteArray(); // didn't find the chunk, return empty
        };

        auto crc32 = [](const QByteArray &ba)->quint32{
            quint32 byte, crc, mask;
            crc = 0xffffffff;
            for (int i = 0; i < ba.size(); ++i)
            {
                byte = ba[i];
                crc = crc ^ byte;
                for (int j = 7; j >= 0; --j)
                {
                    mask = 0-(crc & 1);
                    crc = (crc >> 1) ^ (0xedb88320 & mask);
                }
            }
            return ~crc;
        };

        // extract the png chunks
        QByteArray baseImgByteArray = baseBuffer.data();

        QByteArray pngSignature = baseImgByteArray.mid(0, 8);
        QByteArray pngIhdr = getChunk(baseImgByteArray, IHDR);
        QByteArray pngIdat = getChunk(baseImgByteArray, IDAT);
        QByteArray pngIend = getChunk(baseImgByteArray, IEND);

        // write the png header
        file.write(pngSignature);
        file.write(pngIhdr);

        // write the acTL chunk
        FrameListWidget *frameList = editorWindow->getFrameListWidget();

        QByteArray actl;
        QDataStream actlStream(&actl, QIODevice::WriteOnly);
        actlStream << static_cast<quint32>(8);
        actlStream.writeRawData(acTL.data(), acTL.length());
        actlStream << static_cast<quint32>(frameList->count());
        actlStream << static_cast<quint32>(0);
        actlStream << crc32(actl.mid(4, 4+8));

        file.write(actl);

        // write the frames
        quint32 sequenceNumber = 0;
        for (FrameWidget *frame : frameList->renderOrderWidgetList())
        {
            QImage img = frame->getFlattenedImage();
            quint16 frameRate = getPreviewFramesPerSecond();
            if (!frameRate) frameRate = 1; // minimum frame rate

            // fcTL
            QByteArray fctl;
            QDataStream fctlStream(&fctl, QIODevice::WriteOnly);
            fctlStream << static_cast<quint32>(26);
            fctlStream.writeRawData(fcTL.data(), fcTL.length());
            fctlStream << sequenceNumber++;
            fctlStream << static_cast<quint32>(img.width());
            fctlStream << static_cast<quint32>(img.height());
            fctlStream << static_cast<quint32>(0); // x-offset
            fctlStream << static_cast<quint32>(0); // y-offset
            fctlStream << static_cast<quint16>(1); // numerator seconds
            fctlStream << frameRate; // denominator seconds
            fctlStream << static_cast<quint8>(1); // clear frame
            fctlStream << static_cast<quint8>(0); // source over
            fctlStream << crc32(fctl.mid(4, 4+26));

            file.write(fctl);

            // faDT
            QBuffer buffer(this);
            img.save(&buffer, "PNG");
            QByteArray idat = getChunk(buffer.data(), IDAT);
            if (1 == sequenceNumber) // first frame uses IDAT
            {
                file.write(idat);
            }
            else
            {
                // after the header, before the CRC
                QByteArray frameData = idat.mid(4+4, idat.length()-4-4-4);
                // length of the frame data + the sequence number
                quint32 fdatLength = 4 + frameData.length();

                QByteArray fdat;
                QDataStream fdatStream(&fdat, QIODevice::WriteOnly);
                fdatStream << fdatLength;
                fdatStream.writeRawData(fdAT.data(), fdAT.length());
                fdatStream << sequenceNumber++;
                fdatStream.writeRawData(frameData.data(), frameData.length());
                fdatStream << crc32(fdat.mid(4, 4+fdatLength)); // + header

                file.write(fdat);
            }
        }

        // write the end of the png
        file.write(pngIend);

        // cleanly close the file
        file.close();
    }
    catch (QString msg)
    {
        QMessageBox::critical(this, tr("Error exporting file"),
                              tr("Could not export file. %1").arg(msg));
    }
    catch (...)
    {
        QMessageBox::critical(this, tr("Error exporing file"),
                              tr("A critical error occured.\n"
                                 "File could not be properly exported."));
    }
}

void MainSpriteEditorWindow::setCurrentFrameListWidget(
        FrameListWidget *frameListWidget)
{
    if (getCurrentFrameListWidget() != frameListWidget)
    {
        currentFrameListWidget = frameListWidget;

        // when moving between frame widgets, the current row has "changed"
        if (getCurrentFrameListWidget())
            emit getCurrentFrameListWidget()->currentRowChanged(
                    getCurrentFrameListWidget()->currentRow());
    }
}

void MainSpriteEditorWindow::setCurrentLayerListWidget(
        LayerListWidget *layerListWidget)
{
    if (getCurrentLayerListWidget() != layerListWidget)
    {
        currentLayerListWidget = layerListWidget;
    }
}

void MainSpriteEditorWindow::setToolManagerWidget(
        ToolManagerWidget *toolManager)
{
    if (getToolManagerWidget() != toolManagerWidget)
    {
        toolManagerWidget = toolManager;
    }
}

void MainSpriteEditorWindow::clearPreview()
{
    preview->setFixedSize(0, 0);
}

/*
 * private slots
 */
void MainSpriteEditorWindow::initialized()
{
    // release the initial dock size constraint
    previewDockWidget->setMaximumSize(QWIDGETSIZE_MAX, QWIDGETSIZE_MAX);
}

/*
 * private
 */
void MainSpriteEditorWindow::createMainWindow()
{
    // window settings
    setWindowIcon(QIcon(":/images/spriteEditorIcon.png"));
    setWindowTitle(tr("Sprite Editor"));
    setDockOptions(AnimatedDocks | AllowNestedDocks | AllowTabbedDocks);
    setUnifiedTitleAndToolBarOnMac(true);

    // create object hierarchy
    mdiArea = new QMdiArea(this);
    mdiArea->setHorizontalScrollBarPolicy(Qt::ScrollBarAsNeeded);
    mdiArea->setVerticalScrollBarPolicy(Qt::ScrollBarAsNeeded);
    connect(mdiArea, &QMdiArea::subWindowActivated,
            [this](QMdiSubWindow *window){
        if (getCurrentFrameListWidget())
        {
            frameDockWidget->removeWidget(getCurrentFrameListWidget());
            clearPreview();
        }

        if (window)
        {
            EditorWindow *editorWindow = \
                    qobject_cast<EditorWindow *>(window->widget());

            // update the frames dock
            setCurrentFrameListWidget(editorWindow->getFrameListWidget());
            frameDockWidget->addWidget(getCurrentFrameListWidget());

            // update the preview
            preview->setFixedSize(editorWindow->getSpriteSize());
        }
    });

    // attach to hierarchy
    setCentralWidget(mdiArea);
}

void MainSpriteEditorWindow::createActions()
{
    // file menu
    fileMenu = menuBar()->addMenu(tr("File"));
    fileMenu->addAction(
                QIcon(":/images/newIcon.png"), tr("New"),
                this, &MainSpriteEditorWindow::newFile,
                QKeySequence::New);
    fileMenu->addAction(
                QIcon(":/images/openIcon.png"), tr("Open..."),
                this, &MainSpriteEditorWindow::openFile,
                QKeySequence::Open);
    fileMenu->addAction(
                QIcon(":/images/saveIcon.png"), tr("Save"),
                this, &MainSpriteEditorWindow::saveFile,
                QKeySequence::Save);
    fileMenu->addAction(
                tr("Save As..."),
                this, &MainSpriteEditorWindow::saveAsFile,
                QKeySequence::SaveAs);
    fileMenu->addSeparator();
    fileMenu->addAction(
                tr("Export Animated GIF..."),
                this, &MainSpriteEditorWindow::exportAnimatedGif);
    fileMenu->addAction(
                tr("Export Animated PNG..."),
                this, &MainSpriteEditorWindow::exportAnimatedPng);
    fileMenu->addSeparator();
    QAction *closeAction = fileMenu->addAction(
                tr("Close"), mdiArea, &QMdiArea::closeActiveSubWindow);
    QAction *closeAllAction = fileMenu->addAction(
                tr("Close All"), mdiArea, &QMdiArea::closeAllSubWindows);
    fileMenu->addSeparator();
    fileMenu->addAction(tr("Exit"), qApp, &QApplication::closeAllWindows);

    // paint menu
    paintMenu = menuBar()->addMenu(tr("Paint"));
    scribbleTool = paintMenu->addAction(
                QIcon(":/images/scribbleToolIcon.png"), tr("Scribble Tool"),
                makeScribbleMouseEventHandler([](QPainter &p, QRect r){
                    p.drawLine(r.topLeft(), r.bottomRight());
                }));
    paintMenu->addAction(
                QIcon(":/images/eraserToolIcon.png"), tr("Eraser Tool"),
                makeScribbleMouseEventHandler([](QPainter &p, QRect r){
                    p.setCompositionMode(QPainter::CompositionMode_Clear);
                    p.drawLine(r.topLeft(), r.bottomRight());
                }));
    paintMenu->addAction(
                QIcon(":/images/bucketToolIcon.png"), tr("Bucket Tool"),
                [this]{ toolManagerWidget->setMouseEventHandler(
        [](QMouseEvent *event, EditorWindow *controller,
           CanvasWidget *canvas, ToolManagerWidget *tool){
            if ((event->button() != Qt::LeftButton)) return;

            LayerWidget *layer = controller->getCurrentLayerWidget();
            if (!layer) return;

            QImage img = layer->getImage();

            const QRgb REPLACE_COLOR = tool->getPen().color().rgba();
            const QRgb INITIAL_COLOR = img.pixel(canvas->getCurrentPos());
            if (REPLACE_COLOR == INITIAL_COLOR) return;

            // enqueue and mutate the pixels using breadth first search
            auto addPixel = [&](QQueue<QPoint> *q, QPoint pt, QPoint offset){
                QPoint newPt = pt + offset;
                if (!(0 <= newPt.x() && newPt.x() < img.width() &&
                      0 <= newPt.y() && newPt.y() < img.height())
                    || img.pixel(newPt) != INITIAL_COLOR)
                    return;
                img.setPixel(newPt, REPLACE_COLOR);
                q->enqueue(newPt);
            };

            QQueue<QPoint> queue;
            addPixel(&queue, canvas->getCurrentPos(), QPoint(0, 0));
            while (!queue.empty())
            {
                QPoint pt = queue.dequeue();
                addPixel(&queue, pt, QPoint(1, 0));
                addPixel(&queue, pt, QPoint(0, 1));
                addPixel(&queue, pt, QPoint(-1, 0));
                addPixel(&queue, pt, QPoint(0, -1));
            }

            // save the result
            layer->setImage(img);
        }, NO_OP_MOUSE_EVENT_HANLDER, NO_OP_MOUSE_EVENT_HANLDER);
    });
    paintMenu->addAction(
                QIcon(":/images/lineToolIcon.png"), tr("Line Tool"),
                makeShapeMouseEventHandler([](QPainter &p, QRect r){
                    p.drawLine(r.topLeft(), r.bottomRight());
                }));
    paintMenu->addAction(
                QIcon(":/images/rectangleToolIcon.png"), tr("Rectangle Tool"),
                makeShapeMouseEventHandler([](QPainter &p, QRect r){
                    p.drawRect(r);
                }));
    paintMenu->addAction(
                QIcon(":/images/ellipseToolIcon.png"), tr("Ellipse Tool"),
                makeShapeMouseEventHandler([](QPainter &p, QRect r){
                    p.drawEllipse(r);
                }));
    paintMenu->addSeparator();
    paintMenu->addAction(
                QIcon(":/images/steganographyInIcon.png"),
                tr("Encode message in layer"),
                [this]{ toolManagerWidget->setMouseEventHandler(
        [this](QMouseEvent *event, EditorWindow *controller,
           CanvasWidget *canvas, ToolManagerWidget *tool){
            Q_UNUSED(canvas)
            Q_UNUSED(tool)
            if ((event->button() != Qt::LeftButton)) return;

            LayerWidget *layer = controller->getCurrentLayerWidget();
            if (!layer) return;

            // get the current message and layer
            QString msg = QInputDialog::getText(this, tr("Enter Message"),
                                                tr("Enter message to encode"));
            QByteArray msgByteArray = msg.toUtf8();
            QImage img = layer->getImage();

            // encode the bytes into the three least significant bits in the
            // pixels in the layer
            const char MASK = 0b00000111;
            auto encodeCharPieceAt = [&](QRgb *p, char v, int byteShift){
                *p &= ~(MASK << byteShift * 8);
                *p |= (MASK & (v >> byteShift * 3)) << byteShift * 8;
            };
            int msgIdx = 0;
            for (int r = 0; r < img.height(); ++r)
            {
                for (int c = 0; c < img.width(); ++c)
                {
                    QRgb pixel = img.pixel(c, r);
                    char v = msgByteArray.at(msgIdx);

                    pixel |= 0xff << 3 * 8; // don't let this get blended away
                    encodeCharPieceAt(&pixel, v, 2);
                    encodeCharPieceAt(&pixel, v, 1);
                    encodeCharPieceAt(&pixel, v, 0);

                    img.setPixel(c, r, pixel);

                    if (msgByteArray.count() <= ++msgIdx) goto end;
                }
            }
            end:
            layer->setImage(img);
        }, NO_OP_MOUSE_EVENT_HANLDER, NO_OP_MOUSE_EVENT_HANLDER);
    });
    paintMenu->addAction(
                QIcon(":/images/steganographyOutIcon.png"),
                tr("Extract message in layer"),
                [this]{ toolManagerWidget->setMouseEventHandler(
        [this](QMouseEvent *event, EditorWindow *controller,
           CanvasWidget *canvas, ToolManagerWidget *tool){
            Q_UNUSED(canvas)
            Q_UNUSED(tool)
            if ((event->button() != Qt::LeftButton)) return;

            LayerWidget *layer = controller->getCurrentLayerWidget();
            if (!layer) return;

            // get the current layer
            QImage img = layer->getImage();
            QByteArray msg;

            // extract the three least significant digits into a byte array
            const char MASK = 0b00000111;
            auto extractCharPieceAt = [&](char *v, QRgb p, int byteShift){
                *v |= (MASK & static_cast<char>(p >> byteShift * 8))
                        << byteShift * 3;
            };
            for (int r = 0; r < img.height(); ++r)
            {
                for (int c = 0; c < img.width(); ++c)
                {
                    QRgb pixel = img.pixel(c, r);
                    char v = 0x0;

                    extractCharPieceAt(&v, pixel, 2);
                    extractCharPieceAt(&v, pixel, 1);
                    extractCharPieceAt(&v, pixel, 0);

                    msg += v;

                    if (0x0 == v) goto end;
                }
            }
            end:
            QMessageBox::information(this, tr("Extracted message"),
                                     QString::fromUtf8(msg.data()));
        }, NO_OP_MOUSE_EVENT_HANLDER, NO_OP_MOUSE_EVENT_HANLDER);
    });
    paintMenu->addSeparator();
    paintMenu->addAction(tr("Pen Color"), toolManagerWidget,
                         &ToolManagerWidget::pickPenColor);
    paintMenu->addAction(tr("Pen Width"), toolManagerWidget,
                         &ToolManagerWidget::pickPenWidth);
    paintMenu->addAction(tr("Brush Color"), toolManagerWidget,
                         &ToolManagerWidget::pickBrushColor);

    // frame menu
    frameMenu = menuBar()->addMenu(tr("Frame"));
    frameMenu->addAction(
                QIcon(":/images/addItemIcon.png"), tr("Add Frame"),
                [this]{ if (getCurrentFrameListWidget())
                            getCurrentFrameListWidget()->addNewFrame(); });
    frameMenu->addAction(
                QIcon(":/images/duplicateItemIcon.png"), tr("Duplicate Frame"),
                [this]{ if (getCurrentFrameListWidget())
                            getCurrentFrameListWidget()->duplicateFrame(); });
    frameMenu->addAction(
                QIcon(":/images/trashcanIcon.png"), tr("Remove Frame"),
                [this]{ if (getCurrentFrameListWidget())
                            delete \
                                getCurrentFrameListWidget()->removeFrame(); });

    // layer menu
    layerMenu = menuBar()->addMenu(tr("Layer"));
    layerMenu->addAction(
                QIcon(":/images/addLayerIcon.png"), tr("Add Layer"),
                [this]{ if(getCurrentLayerListWidget())
                            getCurrentLayerListWidget()->addNewLayer(); });
    layerMenu->addAction(
                QIcon(":/images/duplicateLayerIcon.png"),
                tr("Duplicate Layer"),
                [this]{ if (getCurrentLayerListWidget())
                            getCurrentLayerListWidget()->duplicateLayer(); });
    layerMenu->addAction(
                QIcon(":/images/removeLayerIcon.png"), tr("Remove Layer"),
                [this]{ if (getCurrentLayerListWidget())
                            delete \
                                getCurrentLayerListWidget()->removeLayer(); });

    // view menu
    viewMenu = menuBar()->addMenu(tr("View"));

    // window menu
    windowMenu = menuBar()->addMenu(tr("Window"));
    windowMenu->addAction(closeAction);
    windowMenu->addAction(closeAllAction);
    windowMenu->addSeparator();
    windowMenu->addAction(
                tr("Tile"), mdiArea, &QMdiArea::tileSubWindows);
    windowMenu->addAction(
                tr("Cascade"), mdiArea, &QMdiArea::cascadeSubWindows);
    windowMenu->addSeparator();
    windowMenu->addAction(
                tr("Next"), mdiArea, &QMdiArea::activateNextSubWindow,
                QKeySequence::NextChild);
    windowMenu->addAction(
                tr("Previous"), mdiArea, &QMdiArea::activatePreviousSubWindow,
                QKeySequence::PreviousChild);
    windowMenu->addSeparator();
    connect(windowMenu, &QMenu::aboutToShow, [this]{
        // maintain the list of subwindows in the MDI area
        // clear old the list of subwindows
        for (QAction *action : windowMenu->findChildren<QAction *>("subwin"))
            delete action;

        // readd the list of subwindows
        QList<QMdiSubWindow *> subWindowList = mdiArea->subWindowList();
        for (int i = 0; i < subWindowList.count(); ++i)
        {
            QMdiSubWindow *sub = subWindowList.at(i);
            QAction *action = windowMenu->addAction(
                        tr("%1 %2").arg(i + 1).arg(sub->windowTitle()),
                        [this, sub]{ mdiArea->setActiveSubWindow(sub); });
            action->setObjectName("subwin");
            action->setCheckable(true);
            action->setChecked(mdiArea->activeSubWindow() == sub);
        }
    });

    // help menu
    helpMenu = menuBar()->addMenu(tr("Help"));
    helpMenu->addAction(tr("About"), [this]{
        QMessageBox::about(this, tr("About Sprite Editor"),
            tr("Repository:\n"
               "https://github.com/University-of-Utah-CS3505/"
               "sprite-editor-a7-f17-cs3505-anthonychyr\n"
               "\n"
               "Team:\n"
               "AnthonyCarlosElliotIsmaelKameron\n"
               "\n"
               "Members:\n"
               "Anthony Chyr (u0627375)\n"
               "Carlos Enrique Guerra Chan (u0847821)\n"
               "Elliot C Carr-Lee (u0549837)\n"
               "Ismael Kadilo Wa Ngoie (u1120347)\n"
               "Kameron Service (u0963620)\n"
               "\n"
               "Assignment:\n"
               "A7: Sprite Editor Implementation\n"
               "\n"
               "Due:\n"
               "Monday, November 13, 2017 by 11:59pm\n"
               "\n"
               "Class:\n"
               "CS 3505 Software Practices II\n"
               "David Johnson\n"
               "Fall 2017"));
    });
    helpMenu->addAction(tr("About Qt"), qApp, &QApplication::aboutQt);
}

void MainSpriteEditorWindow::generatePaintDock()
{
    // create object hierarchy
    paintDockWidget = new SmartDockWidget(tr("Paint"), this);
    QWidget *paintDockWidgetContents = new QWidget(paintDockWidget);
    QGridLayout *paintDockLayout = new QGridLayout(paintDockWidgetContents);

    // dock settings
    paintDockWidget->setWidget(paintDockWidgetContents);
    addDockWidget(Qt::LeftDockWidgetArea, paintDockWidget);

    // generate the push buttons in the layout
    int numButtons = 0;
    for (QAction *action : paintMenu->findChildren<QAction *>())
    {
        if (action->icon().isNull()) continue;
        QPushButton *pb = makeToolButton(action, paintDockWidget);
        paintDockLayout->addWidget(pb, numButtons / 2, numButtons % 2, 1 , 1);
        ++numButtons;
    }

    // add the tool manager
    paintDockLayout->addWidget(toolManagerWidget, numButtons/2 + 1, 0, 1, 2);

    // provide space to stretch without distorting the layout
    paintDockLayout->setColumnStretch(2, 1);
    paintDockLayout->setRowStretch(numButtons / 2 + 2, 1);
}

void MainSpriteEditorWindow::generateHistoryDock()
{
    historyDockWidget = new SmartDockWidget(tr("History"), this);
    historyDockWidget->hide();
    addDockWidget(Qt::LeftDockWidgetArea, historyDockWidget);
}

void MainSpriteEditorWindow::generateFrameDock()
{
    // create object hierarchy
    frameDockWidget = new SmartDockWidget(tr("Frames"), this);
    QWidget *frameDockWidgetContents = new QWidget(frameDockWidget);
    QGridLayout *frameDockLayout = new QGridLayout(frameDockWidgetContents);

    // dock settings
    frameDockWidget->setWidget(frameDockWidgetContents);
    addDockWidget(Qt::BottomDockWidgetArea, frameDockWidget);

    // generate the push buttons
    for (QAction *action : frameMenu->findChildren<QAction *>())
        if (!action->icon().isNull()) makeToolButton(action, frameDockWidget);

    // set internal placement logic
    frameDockWidget->setResizeEventHandler(
                makeResizeEventHandlerForDocksWithBigCentralWidget(
                    reinterpret_cast<QWidget *(MainSpriteEditorWindow::*)()>(
                        &MainSpriteEditorWindow::getCurrentFrameListWidget)));
    frameDockWidget->setAddWidgetHandler(
                makeAddWidgetHandlerForDocksWithBigCentralWidget(
                    frameDockLayout));
    frameDockWidget->setRemoveWidgetHandler(
                makeRemoveWidgetHandlerForDocksWithBigCentralWidget(
                    frameDockLayout));
}

void MainSpriteEditorWindow::generatePreviewDock()
{
    previewDockWidget = new SmartDockWidget(tr("Preview"), this);

    // prevent initial dock size from being too large
    // release this constraint after the window has been initialized
    previewDockWidget->setMaximumSize(3*DEFAULT_SPRITE_SIZE);

    // create object hierarchy
    QWidget *previewDockWidgetContents = new QWidget(previewDockWidget);

    QGridLayout *previewDockLayout = \
            new QGridLayout(previewDockWidgetContents);

    QScrollArea *scrollArea = new QScrollArea(previewDockWidgetContents);
    scrollArea->setWidgetResizable(true);

    QWidget *scrollAreaWidgetContents = new QWidget(scrollArea);
    QGridLayout *scrollAreaLayout = new QGridLayout(scrollAreaWidgetContents);
    scrollAreaLayout->setMargin(0);

    preview = new QLabel(scrollAreaWidgetContents);
    preview->setProperty("textured", true);
    preview->setScaledContents(true);
    clearPreview();

    frameRateSpinBox = new QSpinBox(previewDockWidgetContents);
    frameRateSpinBox->setRange(MIN_FRAME_RATE_PER_SECOND,
                               MAX_FRAME_RATE_PER_SECOND);
    frameRateSpinBox->setValue(DEFAULT_FRAME_RATE_PER_SECOND);
    frameRateSpinBox->setSuffix(tr(" FPS"));

    QSlider *frameRateSlider = new QSlider(Qt::Orientation::Horizontal,
                                           previewDockWidgetContents);
    frameRateSlider->setRange(frameRateSpinBox->minimum(),
                              frameRateSpinBox->maximum());
    frameRateSlider->setValue(frameRateSpinBox->value());
    frameRateSlider->setTickPosition(QSlider::TicksBelow);
    frameRateSlider->setTickInterval((frameRateSpinBox->maximum() -
                                      frameRateSpinBox->minimum()) / 3);

    connect(frameRateSpinBox, QOverload<int>::of(&QSpinBox::valueChanged),
            frameRateSlider, &QSlider::setValue);
    connect(frameRateSlider, &QSlider::valueChanged,
            frameRateSpinBox, &QSpinBox::setValue);

    QPushButton *previewZoomActual = new QPushButton(
                QIcon(":/images/zoomNormalIcon.png"),
                tr("Actual"),
                previewDockWidgetContents);
    previewZoomActual->setToolTip(tr("Actual size"));
    connect(previewZoomActual, &QPushButton::clicked, [=]{
        EditorWindow *editorWindow = getCurrentEditorWindow();
        if (!editorWindow) return;
        preview->setFixedSize(editorWindow->getSpriteSize());
    });
    QPushButton *previewZoomFit = new QPushButton(
                QIcon(":/images/zoomFitIcon.png"),
                tr("Fit"),
                previewDockWidgetContents);
    connect(previewZoomFit, &QPushButton::clicked, [=]{
        EditorWindow *editorWindow = getCurrentEditorWindow();
        if (!editorWindow) return;
        preview->setFixedSize(editorWindow->getSpriteSize().scaled(
                scrollArea->size() - QSize(2, 2), Qt::KeepAspectRatio));
    });
    previewZoomFit->setToolTip(tr("Resize to fit"));

    // create the timer
    previewTimer = new QTimer(previewDockWidgetContents);
    connect(frameRateSpinBox, QOverload<int>::of(&QSpinBox::valueChanged),
            [=](int fps){
        if (fps > 0)
        {
            previewTimer->start();
            previewTimer->setInterval(1000/fps);
        }
        else
            previewTimer->stop();
    });
    connect(previewTimer, &QTimer::timeout, [=]{
        int row; // declare so the variable exists in case of a jump to label
        FrameWidget *frame;

        if (!getCurrentFrameListWidget()) goto resetPixmap;

        row = preview->property("currentRow").toInt();
        frame = getCurrentFrameListWidget()->widget(row);

        if (!frame) goto resetFrameRow;

        preview->setPixmap(QPixmap::fromImage(frame->getFlattenedImage()));
        preview->setProperty(
                    "currentRow",
                    ++row < getCurrentFrameListWidget()->count() ? row : 0);
        return;

        resetFrameRow:
            preview->setProperty("currentRow", 0);
        resetPixmap:
            preview->setPixmap(QPixmap());
    });
    previewTimer->start();
    emit frameRateSpinBox->valueChanged(frameRateSpinBox->value());

    // set layouts
    previewDockLayout->addWidget(scrollArea, 0, 0, 1, 5);
    previewDockLayout->setColumnStretch(0, 1);
    previewDockLayout->addWidget(frameRateSlider, 1, 1, 1, 2);
    previewDockLayout->addWidget(frameRateSpinBox, 1, 3, 1, 1);
    previewDockLayout->addWidget(previewZoomActual, 2, 1, 1, 1);
    previewDockLayout->addWidget(previewZoomFit, 2, 2, 1, 2);
    previewDockLayout->setColumnStretch(4, 1);

    scrollAreaLayout->addWidget(preview, 0, 0, 1, 1);

    // attach to object hierarchy
    previewDockWidget->setWidget(previewDockWidgetContents);

    scrollArea->setWidget(scrollAreaWidgetContents);

    // dock settings
    addDockWidget(Qt::RightDockWidgetArea, previewDockWidget);
}

void MainSpriteEditorWindow::generateLayerDock()
{
    // create object hierarchy
    layerDockWidget = new SmartDockWidget(tr("Layers"), this);
    QWidget *layersDockWidgetContents = new QWidget(layerDockWidget);
    QGridLayout *layerDockLayout = new QGridLayout(layersDockWidgetContents);

    // dock settings
    layerDockWidget->setWidget(layersDockWidgetContents);
    addDockWidget(Qt::RightDockWidgetArea, layerDockWidget);

    // generate the push buttons
    for (QAction *action : layerMenu->findChildren<QAction *>())
        if(!action->icon().isNull()) makeToolButton(action, layerDockWidget);

    // set placement logic
    layerDockWidget->setResizeEventHandler(
                makeResizeEventHandlerForDocksWithBigCentralWidget(
                    reinterpret_cast<QWidget *(MainSpriteEditorWindow::*)()>(
                        &MainSpriteEditorWindow::getCurrentLayerListWidget)));
    layerDockWidget->setAddWidgetHandler(
                makeAddWidgetHandlerForDocksWithBigCentralWidget(
                    layerDockLayout));
    layerDockWidget->setRemoveWidgetHandler(
                makeRemoveWidgetHandlerForDocksWithBigCentralWidget(
                    layerDockLayout));
}

void MainSpriteEditorWindow::generateToolBar()
{
    auto makeToolBar = [&](QMenu * menu, bool iconsOnly = true)->QToolBar *{
        QToolBar *toolbar = addToolBar(menu->title());
        for (QAction *action : menu->findChildren<QAction *>())
            if (!action->menu() && (!action->icon().isNull() || !iconsOnly))
                toolbar->addAction(action);
        return toolbar;
    };
    makeToolBar(fileMenu);
    makeToolBar(paintMenu)->hide();
    makeToolBar(frameMenu)->hide();
    makeToolBar(layerMenu)->hide();
}

template<class T> void MainSpriteEditorWindow::generateViewMenuItemsFromType(
        const QString &typeName)
{
    for (T *obj : findChildren<T *>())
    {
        QAction *action = viewMenu->addAction(
                    tr("Show %1 %2").arg(obj->windowTitle()).arg(typeName),
                    obj, &T::setVisible);
        action->setCheckable(true);
        connect(obj, &T::visibilityChanged, action, &QAction::setChecked);
    }
    viewMenu->addSeparator();
}

EditorWindow * MainSpriteEditorWindow::makeEditorWindow(
        QSize size,
        QWidget *parent)
{
    EditorWindow * const editorWindow = \
            new EditorWindow(size, toolManagerWidget, parent);

    FrameListWidget * const frameListWidget = \
            editorWindow->getFrameListWidget();

    connect(editorWindow, &EditorWindow::destroyed, [=]{
        // reset poiner for list of frames if current list is destroyed
        if (frameListWidget == getCurrentFrameListWidget())
            setCurrentFrameListWidget(Q_NULLPTR);
    });
    connect(frameListWidget, &EditorWindow::destroyed, [=]{
        // reset pointer for list of layers if current list is destroyed
        for (FrameWidget * frameWidget : frameListWidget->widgetList())
            if (frameWidget->getLayerListWidget()
                    == getCurrentLayerListWidget())
                setCurrentLayerListWidget(Q_NULLPTR);
    });
    connect(frameListWidget, &FrameListWidget::currentRowChanged, [=](int row){
        // change the current list of layers (when user switches frames)
        if (getCurrentEditorWindow() == editorWindow)
        {
            // remove the current list of layers
            if (getCurrentLayerListWidget())
                layerDockWidget->removeWidget(getCurrentLayerListWidget());

            // re-add it if one exists
            FrameWidget * frameWidget = frameListWidget->widget(row);
            if (frameWidget)
            {
                setCurrentLayerListWidget(frameWidget->getLayerListWidget());
                layerDockWidget->addWidget(getCurrentLayerListWidget());
            }
            else
            {
                setCurrentLayerListWidget(Q_NULLPTR);
            }
        }
    });

    return editorWindow;
}

function<void()> MainSpriteEditorWindow::makeScribbleMouseEventHandler(
        function<void(QPainter &, QRect)> paintClosure)
{
    return [paintClosure, this]{ toolManagerWidget->setMouseEventHandler(
        [=](QMouseEvent *event, EditorWindow *controller,
            CanvasWidget *canvas, ToolManagerWidget *tool){
            // always draw something when mouse is pressed
            if ((event->button() != Qt::LeftButton)) return;

            LayerWidget *layer = controller->getCurrentLayerWidget();
            if (!layer) return;

            QImage img = layer->getImage();
            QPainter painter(&img);
            tool->configurePainter(&painter);

            QRect enclosingRect(canvas->getCurrentPos(), QSize(1, 1));
            paintClosure(painter, enclosingRect);

            layer->setImage(img, tool->addPenWidth(enclosingRect));
        }, [=](QMouseEvent *event, EditorWindow *controller,
               CanvasWidget *canvas, ToolManagerWidget *tool){
            // follow the mouse while scribbling
            if (!(event->buttons() & Qt::LeftButton)) return;

            LayerWidget *layer = controller->getCurrentLayerWidget();
            if (!layer) return;

            QImage img = layer->getImage();
            QPainter painter(&img);
            tool->configurePainter(&painter);

            QRect enclosingRect(canvas->getPrevPos(), canvas->getCurrentPos());
            paintClosure(painter, enclosingRect);

            layer->setImage(img, tool->addPenWidth(enclosingRect));
        }, NO_OP_MOUSE_EVENT_HANLDER);
    };
}

function<void()> MainSpriteEditorWindow::makeShapeMouseEventHandler(
        function<void(QPainter &, QRect)> paintClosure)
{
    return [paintClosure, this]{ toolManagerWidget->setMouseEventHandler(
        [=](QMouseEvent *event, EditorWindow *controller,
            CanvasWidget *canvas, ToolManagerWidget *tool){
            // clear the preview
            Q_UNUSED(controller)
            Q_UNUSED(tool)
            if ((event->button() != Qt::LeftButton)) return;
            canvas->clearPreviewImage();
        }, [=](QMouseEvent *event, EditorWindow *controller,
               CanvasWidget *canvas, ToolManagerWidget *tool){
            // preview the shape
            Q_UNUSED(controller)
            if (!(event->buttons() & Qt::LeftButton)) return;

            QImage preview = canvas->getClearedPreviewImage();
            QPainter painter(&preview);
            tool->configurePainter(&painter);

            QRect enclosingRect(canvas->getStartPos(),
                                canvas->getCurrentPos());
            paintClosure(painter, enclosingRect);

            canvas->setPreviewImage(preview, tool->addPenWidth(enclosingRect));
        }, [=](QMouseEvent *event, EditorWindow *controller,
               CanvasWidget *canvas, ToolManagerWidget *tool){
            // draw the shape
            if ((event->button() != Qt::LeftButton)) return;
            canvas->clearPreviewImage();

            LayerWidget *layer = controller->getCurrentLayerWidget();
            if (!layer) return;

            QImage img = layer->getImage();
            QPainter painter(&img);
            tool->configurePainter(&painter);

            QRect enclosingRect(canvas->getStartPos(),
                                canvas->getCurrentPos());
            paintClosure(painter, enclosingRect);

            layer->setImage(img, tool->addPenWidth(enclosingRect));
        });
    };
}

QPushButton * MainSpriteEditorWindow::makeToolButton(
        QAction *action,
        QWidget *parent)
{
    QPushButton *pushButton = new QPushButton(parent);
    pushButton->setFixedSize(DEFAULT_TOOL_PUSH_BUTTON_SIZE);
    pushButton->setFlat(true);
    pushButton->setIcon(action->icon());
    pushButton->setIconSize(DEFAULT_TOOL_PUSH_BUTTON_ICON_SIZE);
    pushButton->setToolTip(action->text());
    connect(pushButton, &QPushButton::clicked, action, &QAction::triggered);
    return pushButton;
}

function<void(SmartDockWidget *, QResizeEvent *)> \
    MainSpriteEditorWindow::makeResizeEventHandlerForDocksWithBigCentralWidget(
        QWidget *(MainSpriteEditorWindow::*getWidget)())
{
    return [=](SmartDockWidget *self, QResizeEvent *event){
        // calculate which way the buttons should flow
        QSize oldSize = event->oldSize();
        const bool IS_LEFT_TO_RIGHT_OLD = oldSize.width() > oldSize.height();
        bool isLeftToRight = self->width() > self->height();

        // get the layout
        QGridLayout *layout = self->findChild<QGridLayout *>();

        // don't redo the layout if no changed or not first time
        if (oldSize.isValid() && IS_LEFT_TO_RIGHT_OLD == isLeftToRight) return;

        // remove the widgets
        for (QPushButton *pb : self->findChildren<QPushButton *>())
            layout->removeWidget(pb);
        if ((this->*getWidget)()) layout->removeWidget((this->*getWidget)());

        // reset the row and column stretches
        for (int i = 0; i < layout->rowCount(); ++i)
            layout->setRowStretch(i, 0);
        for (int i = 0; i < layout->columnCount(); ++i)
            layout->setColumnStretch(i, 0);

        // re-add the widgets
        int numButtons = 0;
        for (QPushButton *pb : self->findChildren<QPushButton *>())
        {
            if (isLeftToRight)
                layout->addWidget(pb, numButtons++, 0, 1, 1);
            else
                layout->addWidget(pb, 0, numButtons++, 1, 1);
        }
        if ((this->*getWidget)()) self->addWidget((this->*getWidget)());

        // re-add the row and column stretches
        layout->setRowStretch(isLeftToRight ? numButtons : 1, 1);
        layout->setColumnStretch(isLeftToRight ? 1 : numButtons, 1);
    };
}

function<void(SmartDockWidget *self, QWidget *widget)> \
    MainSpriteEditorWindow::makeAddWidgetHandlerForDocksWithBigCentralWidget(
        QGridLayout *layout)
{
    return [layout](SmartDockWidget *self, QWidget *widget){
        int numButtons = self->findChildren<QPushButton *>().count();
        if (self->width() > self->height())
            layout->addWidget(widget, 0, 1, numButtons+1, 1);
        else
            layout->addWidget(widget, 1, 0, 1, numButtons+1);
        widget->show();
    };
}

function<void(SmartDockWidget *self, QWidget *widget)> \
    MainSpriteEditorWindow::makeRemoveWidgetHandlerForDocksWithBigCentralWidget(
        QGridLayout *layout)
{
    return [layout](SmartDockWidget *self, QWidget *widget){
        Q_UNUSED(self)
        layout->removeWidget(widget);
        widget->hide();
    };
}
