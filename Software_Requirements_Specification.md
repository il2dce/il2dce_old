# Introduction #

This document specifies the vision and the requirements of IL2DCE. The introduction contains the vision statement and examples for possible campaigns. The details contain the functional requirements in form of use cases and the non-functional requirements.

## Vision ##

The vision of IL2DCE is to provide a dynamic campaign engine for IL-2 _Sturmovik: Cliffs of Dover_. The core feature of IL2DCE is a persistent world where the actions of the participating human and AI actors have a permanent effect and influence the outcome of a campaign. It has a cooperative gameplay that concentrates on aircraft groups, contrary to a dogfight gameplay that concentrates on individual aircraft. It has a single-player and multiplayer mode. The target audience are individual players in single-player, one or more virtual squadrons in closed cooperative campaigns, several virtual squadrons in a closed competitive campaign and individual players and several virtual squadrons in a open campaign.

## Examples for campaigns ##

### Adlerangriff (13 August - 6 September 1940) ###

The Luftwaffe tries to crush Fighter Command. Their bombers attack the airfields and radar stations, while their fighters provide escort. Fighter Command undertakes intercept and patrol operations.

The main goal for both sides is to reduce the amount of aircraft of the opposing side.

# Details #

## Functional requirements (Use cases) ##

The following diagram contains the actors and the high level use cases of the multiplayer mode of IL2DCE.

![https://docs.google.com/drawings/pub?id=1anvH6yd0M7m6vCF5E12WWAKNYHxmKpWbr3MKWR6EGAc&w=1146&h=777&nonsense=null.png](https://docs.google.com/drawings/pub?id=1anvH6yd0M7m6vCF5E12WWAKNYHxmKpWbr3MKWR6EGAc&w=1146&h=777&nonsense=null.png)

The following diagram contains the actors and the high level use cases for the single-player mode of IL2DCE.

![https://docs.google.com/drawings/pub?id=1hdD5yHPC5_K_CSHHCF_JTdvMPu2tgRCZWWALWESS2jc&w=1123&h=877&nonsense=null.png](https://docs.google.com/drawings/pub?id=1hdD5yHPC5_K_CSHHCF_JTdvMPu2tgRCZWWALWESS2jc&w=1123&h=877&nonsense=null.png)

### Host world ###

Actor: Server admin

Steps:

  1. Load mission: The actor selects and loads the mission, either a initial campaign template to start a new campaign or a save-game to continue an existing campaign.
  1. Start battle: The campaign progress continues as soon as the battle is started.
  1. Auto save: The save-game is created periodically to recover the progress in case of a server crash.
  1. Stop battle: The campaign progress is stopped and a save-game is created.

### Attend mission ###

Actor: Multiplayer pilot, single-player pilot, squadron commander

Steps:

  1. Select army: The actor selects his army.
  1. Get aircraft information: The actor can get information over the current state and task for each of the available squadrons.
  1. Select aircraft: He can then selects a squadron and one of its available aircraft.
  1. Read briefing: The actor can read the briefing that contains the mission goal, mission parameter and flight plan.
  1. Fly mission: The actor flies the mission as the pilot of the selected aircraft.
  1. Read debriefing: After the actor has finished the mission he can read the debriefing containing the mission result, achievements and statistics.

Variations:

  * If the actor leaves the mission before it is finished his aircraft will continue under AI control.
  * After selecting a aircraft the actor may switch to a different aircraft.
  * The actor may be tied to one squadron and aircraft. In this case he is unable to select a different squadron and aircraft.

### Adjust mission ###

Actor: Squadron commander, single-player pilot

Steps:
  1. ...

Variations:

  * ...

### Manage squadron ###

Actor: Squadron commander, single-player pilot

Steps:
  1. ...

Variations:

  * ...

### Issue order ###

Actor: Commander-in-chief

Steps:
  1. ...

Variations:

  * New air mission: A new mission for a random air group is started periodically. The mission type depends on the capabilities of the air group's aircraft. The air group must remain idle for a certain time to allow players to select an aircraft, read the briefing and prepare for the flight. Some mission types are escorted, they trigger a escort mission for another random air group.

### Advance world ###

Actor: AMission, Simulator

Steps:

  1. Notify change: The actor notifies the system about a change.
  1. React on change: The system calls the actor to react on a change.

Variations:

  * Request data: The system requests additional data from the actor to impose the proper reaction.

### Create campaign ###

Actor: Campaign author

Steps:
  1. Define strategic points: The actor defines strategic points on the map such as cities, harbours, airfields, ... .
  1. Define front line: The actor defines the initial front of the campaign.
  1. Define unit position: The actor defines the initial position of air, ground and sea units.
  1. Adjust configuration: The actor adjusts the default values in configuration files to fit the requirements of the particular campaign.

Variations:

  * Define events: The actor defines events that are triggered by certain conditions (e.g. date or front line) and may influence front line, position of air and ground units.

## Non-functional requirments ##

### Radar, Acoustics detectors, Observer Corps ###

### Air mission types ###

#### Ground attack mission ####

An air group on a ground attack mission flies to the target area and attack the target. If a escort air group is available, they meet a rendezvous point with the escort directly after the take-off. The rendezvous point is located between the take-off airfields of the participating air groups.

#### Escort mission ####

An air group on a escort mission meets with the escorted air group at a rendezvous point. The accompany the other air group until it has returned close to the landing airfield and then they return to their own landing airfield.

#### Intercept mission ####

An air group on an intercept mission attacks an incoming opposing air group. The air group tries to meet the opposing air group as early as possible. After the meetup the air group returns to the landing airfield.

#### Defensive patrol mission ####

An air group on a defensive patrol protects an area or route over own territory against incoming opposing air groups. After a specific time it returns to the landing airfield.

### Persistent world ###

Create savegame, Load savegame, Load template, Aircraft loss, ...

### Campaign creation ###

New campaigns can be created with little effort. A campaign template file is created within the _Full Mission Builder_ that defines the initial situation of the campaign. Configuration files can be provided optionally to overwrite the global configuration.

### Rapid release cycles ###

New versions are released frequently.

### Scalability ###

#### Performance ####

Only the units that are involved in an operation are visible in the 3D engine. The amount of operations in each battle can be scaled to the available hardware.

#### Game content ####

New game content (e.g. Aircraft, vehicles, statics, maps, ...) can be added without changes in the source code.