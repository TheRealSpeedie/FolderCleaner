# 🗂️ Folder Cleaner 🚀  

A **smart solution** to keep your Downloads folder **organized and clutter-free!**  

## 🔍 What is it?  

**FolderCleaner** is a background service that **monitors** your Downloads folder and keeps it clean by:  
✅ **Sorting files** into subfolders based on file type.  
✅ **Moving images** to the user's Pictures directory (Windows).  
✅ **Automatically deleting old files** that exceed a specified age.  

With **FolderCleaner**, you can **maintain order** and **optimize storage space** effortlessly!  

## ✨ Features  

✔️ **Real-time folder monitoring** – Automatically tracks changes.  
✔️ **File organization** – Moves files into categorized subfolders.  
✔️ **Auto-cleanup** – Deletes files older than a set number of days (default: **30 days**).  
✔️ **Configurable settings** – Customize paths and cleanup rules.  

## 🛠️ Setup  

1. **Download the latest release** from [GitHub Releases](#).  
2. Open **`FolderCleaner.exe.config`**.  
3. Locate **`PathToUser`** and **enter your username**.  
4. If you are using the source code, update the paths manually in:  
   - `FolderCleaner/Properties/Settings.Designer.cs`  
   - `FolderCleaner/Properties/Settings.settings`  
5. **Run CMD as Administrator**.  
6. Enter the following command to register FolderCleaner as a service:  

   ```cmd
   sc create "FolderCleaner" binpath= "[Path to unzipped FolderCleaner.exe]" 
   ```

7. If it doesn’t work:
      - Ensure the system has the correct permissions.
      - Verify that the EXE file is accessible.
        
## 📌 To-Do / Future Improvements
- Add a GUI for easier customization.
-  Support for multiple folders (e.g., Desktop, Documents).
-  Cloud storage integration (OneDrive, Google Drive, etc.).
    
🤝 Contributing
We welcome contributions! Feel free to fork the repo, submit issues, or create pull requests.

📜 License
This project is licensed under the MIT License – see the LICENSE file for details.
