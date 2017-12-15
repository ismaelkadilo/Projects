#
# A8.pro
#
# Anthony Chyr (u0627375)
# Carlos Enrique Guerra Chan (u0847821)
# Elliot C Carr-Lee (u0549837)
# Ismael Kadilo Wa Ngoie (u1120347)
# Jared Earl (u1120117)
# Kameron Service (u0963620)
# Wesley Barth (u0488618)
#
# CS 3505 Software Practices II Johnson
# A8: An Agile Educational Application

QT += core gui multimedia webenginewidgets widgets

TARGET = A8
TEMPLATE = app

DEFINES += QT_DEPRECATED_WARNINGS

HEADERS += \
    constants.h \
    creditcarddialog.h \
    introwindow.h \
    simulationwindow.h \
    storemodel.h \
    storewindow.h \
    tools/solver.h \
    youtubeplayerwindow.h

SOURCES += \
    main.cpp \
    creditcarddialog.cpp \
    introwindow.cpp \
    simulationwindow.cpp \
    storemodel.cpp \
    storewindow.cpp \
    tools/solver.cpp \
    youtubeplayerwindow.cpp

FORMS += \
    creditcardwindow.ui \
    simulationwindow.ui \
    storewindow.ui

RESOURCES += \
    resources.qrc

# External libraries
EXTERN_PATH = $$PWD/external

INCLUDEPATH += $${EXTERN_PATH}/include
DEPENDPATH += $${EXTERN_PATH}/include

win32 { # QT requires { to be on the same line as win32
    WIN32_PATH = $${EXTERN_PATH}/win32

    # Box2D lib
    CONFIG(debug, debug|release): LIBS += -L$${WIN32_PATH}/Box2D/lib \
        -lBox2D-d \
        -lGLEW-d \
        -lGLFW-d \
        -lIMGUI-d
    CONFIG(release, debug|release): LIBS += -L$${WIN32_PATH}/Box2D/lib \
        -lBox2D \
        -lGLEW \
        -lGLFW \
        -lIMGUI

    # SFML lib
    CONFIG(debug, debug|release): LIBS += -L$${WIN32_PATH}/SFML/lib \
        -lsfml-audio-d \
        -lsfml-graphics-d \
        -lsfml-main-d \
        -lsfml-network-d \
        -lsfml-window-d \
        -lsfml-system-d
    CONFIG(release, debug|release): LIBS += -L$${WIN32_PATH}/SFML/lib \
        -lsfml-audio \
        -lsfml-graphics \
        -lsfml-main \
        -lsfml-network \
        -lsfml-window \
        -lsfml-system

    # SFML dll
    LIBS += -L$${WIN32_PATH}/SFML/bin
    CONFIG(debug, debug|release): LIBS += -L$${WIN32_PATH}/SFML/bin/debug
    CONFIG(release, debug|release): LIBS += -L$${WIN32_PATH}/SFML/bin/release
}

macx { # QT requires { to be on the same line as macx
    MACX_PATH = $${EXTERN_PATH}/macx

    #Box2D lib
    CONFIG(debug, debug|release): LIBS += -L$${MACX_PATH}/Box2D/lib \
        -lBox2D \
        -lGLEW \
        -lGLFW \
        -lIMGUI
    CONFIG(release, debug|release): LIBS += -L$${MACX_PATH}/Box2D/lib \
        -lBox2D \
        -lGLEW \
        -lGLFW \
        -lIMGUI

    #SFML lib
    CONFIG(debug, debug|release): LIBS += -L$${MACX_PATH}/SFML/lib \
        -lsfml-audio \
        -lsfml-graphics \
        -lsfml-network \
        -lsfml-system \
        -lsfml-window
    CONFIG(release, debug|release): LIBS += -L$${MACX_PATH}/SFML/lib \
        -lsfml-audio \
        -lsfml-graphics \
        -lsfml-network \
        -lsfml-system \
        -lsfml-window

    # SFML framework link
    QMAKE_POST_LINK += install_name_tool -add_rpath \
        $${MACX_PATH}/SFML/Frameworks \
        $$OUT_PWD/A8.app/Contents/MacOS/A8
}
