# Introduction #

This document contains instructions on how to build IL2DCE from source. This is useful for developers and very ambitious users that want to work with the source code or use unreleased versions. Regular users should use a binary release and follow the [Install Instructions](Install_Instructions.md).


# Details #

The source of the current development path is available within the trunk. The sources for releases are available as tags. Parallel development paths are available as branches.


## Software requirements ##

### IL-2 Sturmovik: Cliffs of Dover ###

IL2DCE is an AddIn that integrates into _IL-2 Sturmovik: Cliffs of Dover_. It has dependencies to the following assemblies of _IL-2 Sturmovik: Cliffs of Dover_:
  * core.dll
  * gamePages.dll
  * gamePlay.dll
  * gameWorld.dll

They are located at "$home\parts\core". For the default installation this is "C:\Program Files (x86)\Steam\SteamApps\common\il-2 sturmovik cliffs of dover\parts\core". The projects of the IL2DCE solution are configured to use a relative path to these assemblies. Therefore the source has to be placed at a specific location relative to the $home folder.

See [Checkout](#Checkout.md) for more details.


### SubWCRev.exe ###

IL2DCE uses the highest SVN revision number as part of the assembly version number. To retrieve it from the SVN repository and make it available as a variable the tool "SubWCRev.exe" is used. This Windows console application is part of a _TortoiseSVN_ installation. IL2DCE projects call "SubWCRev.exe" without a specified path. This is possible because _TortoiseSVN_ adds its binary folder (e.g. "C:\Program Files\TortoiseSVN\bin") to the environment variable "path".

See the [TortoiseSVN Documentation](http://tortoisesvn.net/docs/release/TortoiseSVN_en/tsvn-subwcrev.html) for more details.


## Checkout ##

Because of the dependencies to [IL-2 Sturmovik: Cliffs of Dover](#IL-2_Sturmovik:_Cliffs_of_Dover.md) the solution has to be placed relative to the "$home" folder. The "$solution" folder that contains the "IL2DCE.sln" has to be at:

> $home\parts\IL2DCE\`<folder`>\IL2DCE\

Note that `<folder`> can be a folder of any name. It is recommended to use this folder to indicate the working copy of the trunk or a specific tag or branch.

To checkout the trunk you have to checkout:

> https://il2dce.googlecode.com/svn/trunk/ to   $home\parts\IL2DCE\trunk\

So for the default installation "IL2DCE.sln" has to be located at:

> C:\Program Files (x86)\Steam\SteamApps\common\il-2 sturmovik cliffs of dover\parts\IL2DCE\trunk\IL2DCE.sln


## Build ##

After a clean checkout the files "GlobalAssemblyInfo.cs" and "IL2DCEInst.nsi" are marked to be missing. As these files contain the highest revision number they cannot added to the SVN repository. The files are created during the first build by [SubWCRev.exe](#SubWCRev.exe.md) based on "GlobalAssemblyInfo.template" and "IL2DCEInst.template". This build will fail because the files are not available at the beginning. Therefore after a clean checkout the solution has to be build two times.


### Debug configuration ###

#### Output path ####

The AddIn configuration files are copied in:

> $home\AddIns\

The assemblies are created in:

> $home\parts\IL2DCE\


#### Run ####

To execute _IL-2 Sturmovik: Cliffs of Dover_ from Visual Studio directly  after the build the Game project is configured to execute the "$home\Launcher.exe" if the user starts debugging of the solution (Shortcut: F5). Note that the Game project has to be set as startup project.


### Release configuration ###

#### Output path ####

The AddIn configuration files are copied in:

> $solution\Release\AddIns\

The assemblies are created in:

> $solution\Release\parts\IL2DCE\


#### Version number ####

The version number uses the format:

> `<major`>.`<minor`>.`<build`>.`<revision`>

The major number is increased when fundamental changes were made compared to the previous version. The minor number is increased when new features were added compared to the previous version. The build number is increased when only bugfixes were applied. The revision number is the highest revision number of the repository of the source code.

With exception of the revision number they have to be increased by the developer within the "GlobalAssemblyInfo.template" file that is included in the Game project. The revision number is updated by [SubWCRev.exe](#SubWCRev.exe.md).


#### Installer ####

The "IL2DCEInst.nsi" script file for the [Nullsoft Scriptable Install System (NSIS)](http://nsis.sourceforge.net/) is provided to create a installer. If the solution is build in release configuration the script is copied to the folder "$solution\Release" as a post-build event. There it has to be executed by the developer. The created installer has to naming convention:

> IL2DCE-<major`>.`<minor`>.`<build`>.`<revision`[M]`>.exe

With exception of the revision number the version numbers have to be increased by the developer within the "IL2DCEInst.template" file that is included in the Game project. The revision number is updated by [SubWCRev.exe](#SubWCRev.exe.md). Note that the revision number has to suffix "M" if there are local changes in the working directory that are not yet commited.