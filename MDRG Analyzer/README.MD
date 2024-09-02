# MDRG (My Dystopian Robot Girlfriend) Analyzer

### Usage

1. Extract the Application from the Archive
2. Open the Application
3. Click "File" -> "Load File"
4. The saves are now stored in "C:\Users\USER\AppData\LocalLow\IncontinentCell\My Dystopian Robot Girlfriend\Saves". You can also export your saves in the Main Menu's "Load" menu, but if you use this, you will have to import the saves in the same menu yourself.
5. In the "Info" Tab, select your Save Game Slot Number (Select a File, its Save Slot will be displayed at the bottom (This is important!).) You will know it worked when it tells you the Game Version at the Bottom-Right.
6. Done. You may now check through the other tabs to find your game info.

### Linux

Linux is **not** supported, however, it is confirmed to work using "PlayOnLinux with 64bit" on Pop_OS 22.04 (Thanks to bgrmystr2 for testing this!)

If you use Linux, any technical difficulties will **not** be supported, and I cannot give any support for it.

Known issues:
- It may not work with Pop_OS' built-in Wine.
- The Debug tab may use up a lot of resources, and depending on your save size, may cause the program to stop responding until it finishes loading.
- Resizing the program may mess up labels and more.

### Links
[Click here to get to the GitHub repository of the program.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer)

[Click here to get the latest version of the program.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer/releases/latest)

[Click here to report bugs.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer/issues)

[Click here to suggest things.](https://github.com/Wehrmachtserdbeere/MDRG-Analyzer/discussions/categories/ideas)

### Frequently Asked Questions

#### How do I open an Autosave?

Right now, it's not natively supported. However, you can still do it:

1. Backup your save.mdrg file
2. Open your save.mdrg file in a File Editor
3. Observe: The structure follows a JSON file format, if you have worked with it, you might already know what to do, and can skip a few steps. If the file is in a single line, use a "JSON Beautifyer" to make it easier to work with.
4. Your saves are stored inside the `"saves": []` part. Your autosaves are stored inside the `"autoSaves": []` part.
5. Every save is encompassed by two curly brackets: `{ }`. This is important to remember!
6. Select in your `"autoSaves": []` one of the saves. This includes the curly brackets. For reference, a save has things like `"slot": 0,`, `"_time": 12345678900,`, and more.
7. Inside your `"saves": []` part, after the other saves, add a new line and paste your clipboard. You will know it worked if it looks very similar to the other saves, only with the exception of your new save having `"notes": "AutoSave"` in it.
8. Change your `"slot":` to a free slot. Example: If the previous saves are `"slot": 0,` and `"slot": 1,`, then in your new save, write `"slot": 2,`.
9. Make sure it works. Start the game, and load the slot. You should be able to load it.
10. Close the game, open MDRG Analyzer, and choose your slot.
11. You should now be able to view/edit your Autosave.
