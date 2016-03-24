# Introduction #

IL2DCE is a dynamic campaign engine for the combat flight simulator _IL-2 Sturmovik: Cliffs of Dover_.

# Details #

In the current state a prototype is available that features the randomised creation of missions based on a template mission file. IL2DCE randomly selects one of the AirGroups within the template and creates a primary mission for it. The available mission types depend on the aircraft type:

  * Ground attack (Bomber)
  * Recon (Bomber)
  * Escort (Fighter)
  * Intercept (Fighter)
  * Offensive patrol (Fighter)
  * Defensive patrol (Fighter) (temporary disabled)

IL2DCE randomly selects one of the available mission types and one of the target areas defined by the FrontMarkers within the template. Depending on the primary mission type a secondary mission (e.g. a intercept mission for a recon mission) or tertiary mission (e.g. a escort mission for a ground attack mission) is created. At least one primary mission is created for the AirGroup of the player. Additional primary missions for other AirGroups might be created depending on the configuration. Default is the creation of one additional primary mission.

Note that following obvious limitations of the prototype:
  * The campaign doesn't progress and is restarted every time.
  * There are no GroundGroups.
  * No mission briefing.


IL2DCE is realised as a plug-in using the official AddIn interface to integrate into _IL-2 Sturmovik: Cliffs of Dover_, i.e. it's not a hack or mod. IL2DCE can be started within the single-player menu with the new entry _IL2DCE Single_. After the user has select one of the available campaigns he can choose his army, AirGroup and aircraft.  Provisions for the integration into the multiplayer menu are already present, however due to the lack of any documentation by the Devs it seems not to work properly at the moment.

Getting started:

  * [Download the latest release.](http://code.google.com/p/il2dce/downloads/list)
  * [Read the Install Instructions.](Install_Instructions.md)
  * Developers: [Read the Build Instructions.](Build_Instructions.md)


# License #

IL2DCE uses the _GNU Affero General Public License, version 3 (AGPLv3)_. [Read more](http://www.gnu.org/licenses/why-affero-gpl).