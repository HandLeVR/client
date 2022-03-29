**For an english version see below.**

Eine Beschreibung des kompletten HandLeVR-Projekts und die nötigen Schritte um die Anwendungen, die aus dem Projekt hervorgegangen sind, zu kompilieren, können auf der [Organisationsseite](https://github.com/HandLeVR) gefunden werden.

# Client

Der Client enthält das Autorenwerkzeug, die Trainingsanwendung und das Reflexionswerkzeug. Das Autorenwerkzeug ermöglicht das Erstellen, Bearbeiten und Löschen von Nutzer/innen, Gruppen, Aufgaben, Aufgabensammlungen, Medien, Lacken und Aufnahmen, sowie das Zuweisen von Aufgaben, die dann in der Trainingsanwendung durchgeführt werden können. Aufgaben können verschiedene Teilschritte enthalten, wie z.B. Multiple-Choice-Fragen, Schätzaufgaben, eine Spritzprobe oder einen Farbauftrag. In der Trainingsanwendung können zugewiesene Aufgaben in VR durchgeführt werden. Es ist auch möglich einen Probiermodus zu nutzen, in dem verschiedene Werkstücke mit verschiedenen Lacken lackiert werden können. Während des Lackierens stehen verschiedene Hilfsmittel zur Verfügung, wie ein Distanzstrahl, Distanzmarker oder ein Winkelstrahl. Im Probiermodus ist es außerdem möglich eine Aufnahme von einem Farbauftrag zu erstellen, die dann in einer Aufgabe eingebunden werden kann. Das Reflexionswerkzeug ermöglicht es Farbaufträge, die bei der Durchführung von Aufgaben entstanden sind, am PC anzuschauen und mit Hilfe verschiedener Erfolgskriterien zu reflektieren. Der Client wurde mit Unity 2021.2.7f1 entwickelt.


# Konfiguration

Die folgenden Änderungen können in der Konfigurationsdatei vorgenommen werden (`Client_Data\StreamingAssets\Config.txt`). Die Konfigurationsdatei wird in allen Anwendungen (Autorenwerkzeug, Trainingsanwendung und Reflexionswerkzeug) verwendet.

## Verbindung zum Server 

Wird ein lokaler Server verwendet, der auf dem gleichen System wie die Anwendung läuft, sollten die folgenden Einstellungen nicht verändert werden. Wird ein dedizierter Server verwendet, müssen die folgenden Einstellungen angepasst werden, um eine Verbindung mit dem Server zu ermöglichen: 

 
```
server.https=true 
server.url=localhost:8080 
server.client-user-name=handlevrclient 
server.client-secret=XY7kmzoNzl100 
server.oauth-login-url=localhost:8080/oauth/token?grant_type=password 
server.oauth-refresh-token-url=localhost:8080/oauth/token?grant_type=refresh_token 
```

Wurde auf dem Server HTTPS eingerichtet (siehe [Konfiguration Server](https://github.com/HandLeVR/server/blob/master/README.md#konfiguration)), muss `server.https` auf `true` gesetzt werden. Die restlichen Felder müssen entsprechend den Einstellungen und der URL des Servers angepasst werden. Bei den URLs muss in der Regel `localhost:8080` ersetzt werden. 

## SSL 

HTTPS wird verwendet, sobald `server.https` auf `true` gesetzt wird. Soll zusätzlich noch das Zertifikat des Servers geprüft werden, müssen folgende Einstellungen angepasst werden: 

```
ssl.enabled=true 
ssl.keystore.path=SSL/handlevr.p12 
ssl.keystore.password=passwort 
```

Das Feld `ssl.enabled` aktiviert die Zertifikatsprüfung. Dann muss im Pfad, der bei `ssl.keystore.path` angegeben wurde, ein entsprechender Keystore zu finden sein (siehe see [Konfiguration Server](https://github.com/HandLeVR/server/blob/master/README.md#konfiguration)). Der Pfad kann relativ zum Ordner `Client_Data/StreamingAssets` oder absolut sein. Das Passwort zum Keystore muss bei `ssl.keystore.password` angegeben werden. 

 
## Erfolgskriterien 

Die Konfigurationsdatei erlaubt es die Erfolgskriterien in der Trainingsanwendung und im Reflexionswerkzeug anzupassen. Dies sollte allerdings nur von erfahrenen Benutzenden gemacht werden. In der Konfigurationsdatei wird erklärt, wie sich diese Einstellungen auswirken.

## System

```
system.windowed-mode=false
```
Wenn dieser Wert auf "false" gesetzt ist, startet die Anwendung im Vollbildmodus, ansonsten im Fenstermodus.

```
system.max-file-size=1000
```
Beeinflusst die maximale Dateigröße von Medien und Aufnahmen in MB. Dieser Wert kann maximal auf 2000 gesetzt werden.

```
system.language=de_DE
```
Beeinflusst die Sprache des Systems. Es ist möglich English (en_US) oder Deutsch (de_DE) auszuwählen. Bisher steht nur der Probiermodus und das Reflexionswerkzeug auf Englisch zur Verfügung.

# Szenen

**Login Screen**: Der Login-Bildschirm ist das erste, was Nutzer/innen sehen und ermöglicht die Anmeldung am System. Um die Anmeldedaten zu verifizieren wird eine Anfrage an den Server geschickt. Wenn sich der/die Nutzer/in das erste Mal anmeldet, muss er/sie die Datenschutzbestimmungen akzeptieren und das Passwort ändern, sowie eine Sicherheitsfrage bestimmen. Über die Beantwortung der Sicherheitsfrage ist es möglich sein Passwort zu ändern, wenn man es vergessen hat.

Über diesen Bildschirm ist es außerdem möglich das Tutorial zu starten, um eine Einführung in die Möglichkeiten und die Steuerung der Anwendung zu erhalten. Die Anwendung kann auch ohne Anmeldung im Probiermodus gestartet werden.

![](Images/login_screen.png)

**Main Menu**: Diese Szene ermöglicht die Auswahl der Anwendung, die der/die Nutzer/in starten möchte. Es ist möglich das Autorenwerkzeug, die Trainingsanwendung oder das Reflektionswerkzeug zu starten.

Außerdem ist es hier möglich das eigene Profil zu bearbeiten oder zu löschen.

![](Images/login_screen.png)

**Authoring Tool**: Die Szene enthält das Authorenwerkzeug. Hier können Aufgaben, Aufgabensammlungen, Nutzer/innen, Gruppen, Lacke, Medien und Aufnahmen erstellt, modifiziert oder entfernt werden.

![](Images/authoring_tool.png)

**Paint Shop**: Auf dieser Szene basieren alle 3D-Szenen im Projekt. Sie enthält eine Lackierkabine, die einer echten Lackierkabine entsprechend lackiert wurde. Der Monitor an der Wand ist nur im Probiermodus sichtbar und erlaubt den/der Nutzer/in verschiedene Dinge auszuprobieren.

![](Images/paint_shop.png)

**Paint Shop Dynamic Scenario**: Diese Szene erweitert die *Paint Shop* Szene und muss als additive Szene geladen werden. Hier wird der Monitor durch einen anderen Monitor ersetzt, der die Auswahl der Lernaufgaben ermöglicht, die dem/der Nutzer/in im Autorenwerkzeug zugewiesen wurden. Eine Aufgabe kann aus verschiedenen Teilschritten bestehen, wie Multiple-Choice-Fragen, Sortieraufgaben, Unterstützenden Informationen (z.B. Bilder oder Videos), einer Spritzprobe, einem Farbauftrag und mehr. Ein Virtueller Ausbildungsmeister kann auditiv Hinweise geben.

![](Images/paint_shop_dynamic_scenario.png)

**Reflection Tool**: Diese Szene erweitert die *Paint Shop* Szene, entfernt aber den VR-Modus. In dieser Szene können die Farbaufträge, die bei der Durchführung von Aufgaben aufgenommen wurde, geladen und abgespielt werden. In der 3D-Ansicht auf der linken Seite wird die Aufnahme abgespielt. Mit Hilfe der unteren Leiste kann zu verschiedenen Zeitstempeln gesprungen werden. Es ist möglich sich in der Szene mit den Pfeiltasten und der Maus zu bewegen. Auf der rechten Seite werden verschiedene Reflektionsparameter oder Einstellungen dargestellt.

![](Images/reflection_tool.png)

# Weitere Dokumentation
- [Neue Werkstücke hinzufügen](docs/NEW_WORKPIECES.md)
- [Implementierungsdetails zur Lackapplikation](docs/PAINTING_PROCESS.md) (Englisch)

---
See the [organization page](https://github.com/HandLeVR) for a complete description of the HandLeVR, the structure and building process of the applications emerged from the the HandLeVR project.

# Client
The client contains the authoring tool, the training application and the reflection tool. The authoring tool allows you to create, edit, and delete users, groups, tasks, task collections, media, paints, and recordings, as well as assign tasks that can then be performed in the training application. Tasks can contain various sub-steps, such as multiple choice questions, estimation tasks, a spray sample, or a paint application. In the training application, assigned tasks can be performed in VR. It is also possible to use a trial mode in which different workpieces can be painted with different coats. During painting, various tools are available, such as a distance beam, distance marker or angle beam. In the trial mode, it is also possible to create a recording of a painting process, which can then be included in a task. The reflection tool allows to view paint jobs without a VR headset, which are recorded while performing tasks. This allows to reflect on them using various success criteria. The client was developed using Unity 2021.2.7f1.

# Configuration
The following changes can be made in the configuration file (`Client_Data\StreamingAssets\Config.txt`) to configure the application(s). The configuration file is used in all applications (authoring tool, training application and reflection tool).

## Server Connection
If a local server running on the same system as the application is used, the following settings should not be changed. If a dedicated server is used, the following settings must be adjusted to allow connection to the server: 

```
server.https=true 
server.url=localhost:8080 
server.client-user-name=handlevrclient 
server.client-secret=XY7kmzoNzl100 
server.oauth-login-url=localhost:8080/oauth/token?grant_type=password 
server.oauth-refresh-token-url=localhost:8080/oauth/token?grant_type=refresh_token 
``` 

If HTTPS has been set up on the server (see [server configuration](https://github.com/HandLeVR/server/blob/master/README.md#configuration)), `server.https` must be set to `true`. The remaining fields must be adjusted according to the server's settings and URL. Normally, `localhost:8080` must be replaced in all URLs. 

## SSL
HTTPS is used as soon as `server.https` is set to `true`. If the server's certificate should also be checked, the following settings must be adjusted: 

```
ssl.enabled=true 
ssl.keystore.path=SSL/handlevr.p12 
ssl.keystore.password=password 
```

The `ssl.enabled` field enables certificate checking. Then there must be a corresponding keystore in the path specified by `ssl.keystore.path` (see [server configuration](https://github.com/HandLeVR/server/blob/master/README.md#configuration). The path can be relative to the Client_Data/StreamingAssets folder or absolute. The password to the keystore must be specified by `ssl.keystore.password`. 

## Success Criteria 
The configuration file allows to customize the success criteria in the training application and in the reflection tool. However, this should only be done by experienced users. How is affects the applications is explained in the configuration file.

## Other
```
system.windowed-mode=false
```
If this value is set to false, the application starts in full screen mode, otherwise in windowed mode.

```
system.max-file-size=1000
```
Affects the maximum file size of media and recordings in MB. This value can be set to a maximum of 2000.

```
system.language=de_DE
```
Affects the language of the system. It is possible to select English (en_US) or German (de_DE). So far, only the trial mode and the reflection tool are available in English.

# Scenes

**Login Screen**: The login screen is the first thing users see and allows them to log in to the system. To verify the login data, a request is sent to the server. When the user logs in for the first time, he/she must accept the privacy policy and change the password, as well as determine a security question. By answering the security question it is possible to change his/her password if he/she has forgotten it.

From this screen it is also possible to start the tutorial to get an introduction to the possibilities and controls of the application. The application can also be started in trial mode without logging in.

![](Images/login_screen.png)

**Main Menu**: This scene allows to select the application that the user wants to launch. It is possible to start the authoring tool, the training application or the reflection tool.

It is also possible to edit or delete one's own profile here.

![](Images/main_menu.png)

**Authoring Tool**: The *Authoring Tool* scene is the main scene of the tool. Here learning tasks, tasks collections, user, groups, coats, media and recordings can be created, modified and removed.

![](Images/authoring_tool.png)

**Paint Shop**: The *Paint Shop* scene is the base scene for every 3D scene in the project. It contains paint booth modelled after a real paint both. The monitor at the wall is only visible in the test mode and allows the user to try out various things. 

![](Images/paint_shop.png)

**Paint Shop Dynamic Scenario**: The *Paint Shop Dynamic Scenario* scene extends the *Paint Shop* scene and needs to be loaded as an additive scene. It replaces the monitor with another monitor which allows the select tasks assigned to the user. A task can consist of various sub task like multiple or single choice tasks, a sorting task, supportive information (e.g. pictures or videos), a spray test, a painting task and more. A virtual instructor can give audio hints. 

![](Images/paint_shop_dynamic_scenario.png)

**Reflection Tool**: The *Reflection Tool* scene extends the *Paint Shop* scene but removes the VR mode. In this scene a 2D UI is shown where the user can select learning tasks and load result of the tasks of existent. In the 3D view on the left a replay of the coat application can be seen. The play bar on the bottom can be used to jump to different time stamps. It is possible to move around in the scene with the mouse and the arrow keys. On the right side various reflection parameters and setting options are displayed. 

# Additional Documentation
- [Adding new workpieces](docs/NEW_WORKPIECES.md)
- [Paint application implementation details](docs/PAINTING_PROCESS.md)