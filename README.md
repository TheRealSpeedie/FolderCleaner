# Folder Cleaner
## What is it?
FolderCleaner monitors the Downloads folder and performs regular maintenance tasks. These include sorting files into subfolders based on file types or moving images to the user's Pictures directory in Windows. Additionally, it automatically deletes files that have exceeded a certain age. This service helps maintain organization and efficiently utilize storage space.

### Features:
Continuously monitors a defined folder.
Automatically sorts files into predefined subfolders (e.g., by file type or date).
Deletes files that are older than a specified period (e.g., 30 days).

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
