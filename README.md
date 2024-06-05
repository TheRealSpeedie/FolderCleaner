# Folder Cleaner
## What is it?
Der FolderCleaner überwacht den Download Ordner und führt regelmäßig Wartungsaufgaben durch. Diese beinhalten das Sortieren von Dateien in Unterordner basierend auf Dateitypen oder Bilder werden in das Windows verzeichnis Bilder des Benutzers verschoben. Ebenfallls werden Daten automatische gelöscht, die ein bestimmtes Alter überschritten haben. Dieser Service hilft, Ordnung zu halten und den Speicherplatz effizient zu nutzen.

### Funktionen:
- Überwacht kontinuierlich einen definierten Ordner.
Sortiert Dateien automatisch in vordefinierte Unterordner (z. B. nach Dateityp oder Datum).
- Löscht Dateien, die älter sind als eine vorgegebene Zeitspanne (z. B. 30 Tage).


## Setup
1. Download Release
2. Open FolderCleaner.exe.config 
3. Go to PathToUser
4. Enter ur Username
5. Open cmd with Admin rights
6. Enter 
```CMD
sc create "FolderCleaner" binpath= "[Path to unzipped FolderCleaner.exe]" 
```

if it is not working check the rights of the system and if it can access the EXE-File.
