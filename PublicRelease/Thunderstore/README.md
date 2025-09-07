# Automatic Backups For Schedule I
Automatically backs-up your save files every time you save.  
  
Allows you to recover from disaster!
  

There are builds for both the main (IL2cpp) and alternate (Mono) branches of the game.
  
# Installation
1. Requires [MelonLoader](https://melonwiki.xyz/), so install that if you haven't already.
1. Extract the .zip and copy the appropriate .dll file to the `Mods` folder in your Schedule I installation.
	1. If you're on the main branch (haven't changed to "alternate" in Steam) you'll want the IL2Cpp version.
	1. If you're on the alternate branch, you'll want the Mono version.

# Usage
Every time you save your game, this mod exports your save to a timestamped .zip file.
These files can be found alongside your other saves in a folder called `Backups`. To load a previous save, just
select `Import` on the `Continue` screen in Schedule I and navigate to that folder.

Notes:
- Backups are local only and not synced with the Steam cloud.
- Untested in Mac or Linux

# Settings
You can change the settings for this mod through the settings menu when you first load the game up. Just click the `Backups` tab.
  
### Delete Old Backups
You can configure the mod to automatically delete the oldest backups once a certain limit has been reached.
You can turn this feature on or off and the available range is between 25 - 250 in increments of 5. I recommend leaving this setting off or setting the limit very high; it's not always obvious right-away if the game has bugged your save.

When you first load a save slot, you can check the MelonLoader console to see how much disk space your backups are taking up. It's pretty negligible in my experience.

### AutoSave
The mod can automatically save your progress if some amount of time passes since your last save. The timer resets on every save.  

Auto-saves are also backed up. The filenames are prepended with "auto_" so it's easy to find and import your last manual save if you'd like.

By default, autosaving is enabled and the timer is set to 10 minutes. You can enable or disable this feature and adjust the time before saving anywhere between 1 and 60 minutes.

## Like it?
Consider coming back to give this mod a thumbs-up. That will help other players find it and hopefully prevent more save-related disasters! Most importantly, it helps my ego.

### Source
Available on [GitHub](https://github.com/coderTrevor/Automatic_Backups).
  
## Acknowledgements
Mod boilerplate was created with the [MelonLoader.VSWizard](https://github.com/TrevTV/MelonLoader.VSWizard) wizard by TrevTV with additions from the [S1MONO_IL2CPP_Template](https://github.com/weedeej/S1MONO_IL2CPP_Template) template by weedeej.

Thanks to [ifBars](https://github.com/ifBars) who helped me diagnose an Il2Cpp-specific issue.