# Introduction #

This document contains instructions on how to install a binary release IL2DCE. Developers should follow the [Compile Instructions](Compile_Instructions.md).


# Details #

At certain development stages a binary release is provided at [http://code.google.com/p/il2dce/downloads/list](http://code.google.com/p/il2dce/downloads/list).


## Install IL2DCE ##

  1. Download the latest binary release at [http://code.google.com/p/il2dce/downloads/list](http://code.google.com/p/il2dce/downloads/list) into a folder of your choice.
  1. Navigate to this folder and execute the downloaded file (e.g. IL2DCE-a.b.c.d.exe)
  1. Accept the license agreement (not implemented)
  1. Select the root path of your _IL-2 Sturmovik: Cliffs of Dover_ installation ($home). This path depends on the root path of your Steam installation ($steam). The root path of _IL-2 Sturmovik: Cliffs of Dover_ is always at "$steam\steamapps\common\il-2 sturmovik cliffs of dover\". For a default installation the path is
    * "C:\Program Files (x86)\Steam\steamapps\common\il-2 sturmovik cliffs of dover\" for _Windows 7 (64 Bit)_ or
    * "C:\Program Files\Steam\steamapps\common\il-2 sturmovik cliffs of dover\" for _Windows 7 (32 Bit)_.
  1. Finish the install process.

The installer creates files within the "$home\AddIns" and "$home\parts\IL2DCE" folder.

## Update IL2DCE (not implemented) ##
Execute the uninstall process before you install another version of IL2DCE.

## Uninstall IL2DCE ##

  1. Navigate to folder "$home\parts\IL2DCE" and execute the uninstaller (uninst.exe).
> > Example: "C:\Program Files (x86)\Steam\steamapps\common\il-2 sturmovik cliffs of dover\parts\IL2DCE\uninst.exe"
  1. Confirm the uninstall path.
  1. Finish the uninstall process.

The uninstaller removes the files that were created during the install process. It does not remove customer campaigns that were installed by the user.