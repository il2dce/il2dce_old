# Introduction #

A campaign for IL2DCE consists of a campaign template mission and a campaign configuration file.

## Campaign types ##

### Dynamic campaign ###

For a dynamic campaign only the initial strategic and tactical situation is defined for ground units, air units, supplies and reinforcements. The progress and outcome of the campaign is not defined and evolves completely dynamic depending on the past events.

This campaign type is recommended for fictional and semi-historical campaigns.

### Semi-dynamic campaign ###

For a semi-dynamic campaign the strategic and tactical situation for ground units, air units, supplies and reinforcements is defined for various stages of the campaign. The interval between these stages may vary. Between two defined stages the progress of the campaign evolves dynamic. At the end of one stage the definitions of the following stage are applied, however certain information are propagated forward, like strength, supplies and experience of the units. To reduce the inflicted conflicts between the dynamic progress and the defined situation of the following stage the intervals between two stages should be small.

This campaign type is recommended for historical campaigns.

# Details #

## Campaign template file ##

The campaign template file is created within the FMB of _IL-2 Sturmovik: Cliffs of Dover_. There are some regulations for the template file creation which are described in the following section.


### FrontMarkers ###

FrontMarkers are used to specify important locations on the map. There are different locations depending on the ground type at the position of the front marker. If the front marker is placed above city type this is interpreted as a city. If the front marker is placed above water ground type this is interpreted as a harbour. If the front marker is placed above a railway this is interpreted as a train station. The colour of the front marker define friendly, enemy or neutral locations.


### AirGroups ###

AirGroups are configured (aircraft, aircraft count, squadron, weapon, skill, ...) and placed on the map. One waypoint is sufficiant. If the first waypoint is TAKEOFF the AirGroup will spawn on the nearest airfield. Use the SpawnOnPark or Scramble option to specify the stance. These options might be overwritten by the IL2DCE configuration. If the first waypoint is NORMALFLY the AirGroup will spawn in the air at the specified coordinates and altitude.


### GroundGroups (not implemented) ###

Armours are configured (type, vehicle count, regiment) and placed on the map. The armours will spawn at the nearest friendly city and will attack the nearest neutral or enemy city.

Vessels are configured (type, skill, cargo) and placed on the map. The vessels will spawn at the nearest harbout and will move to the nearest friendly harbour.


## Campaign configuration file (not implemented) ##

### AircraftInfo.ini (not implemented) ###

The "AircraftInfo.ini" file has the [Aircrafts](Aircrafts.md) section that lists all available aircraft and defines the AircraftType. The [`<Aircraft>`] section lists the available MissionTypes for the aircraft. The [`<Aircraft>_<MissionType>`] section lists the available loadouts for the Aircraft for that MissionType.

Example:
```
[Aircrafts]
  Aircraft.BlenheimMkIV
  ...
[Aircraft.BlenheimMkIV]
  type Bomber
  missionType0 RECON
  missionType1 GROUND_ATTACK_AREA
  ...
[Aircraft.BlenheimMkIV_RECON]
  altitudeMin 2000.0
  altitudeMax 9000.0
  loadout0 Weapons 1 1 0 0 0
[Aircraft.BlenheimMkIV_GROUND_ATTACK_AREA]
  altitudeMin 500.0
  altitudeMax 6000.0
  loadout0 Weapons 1 1 1 0 0
  loadout1 Weapons 1 1 2 0 0
  ...
  fuel0 100
  fuel1 100
```

There is a global "AircraftInfo.ini" file. A campaign can provide a local "AircraftInfo.ini" file where the information of the global file can be overwritten for selected aircraft.

### TOE.ini (not implemented) ###
```
[Placeholders]
  Armor.Cruiser_Mk_IVA
  Artillery.40mm_2pdr_AT_Gun_Mk_II
  
  ...
[Armor.Cruiser_Mk_IVA]
  type Armor
  templateFile Artillery.40mm_2pdr_AT_Gun_Mk_II.mis
[Artillery.40mm_2pdr_AT_Gun_Mk_II]
  type Artillery
  templateFile Artillery.40mm_2pdr_AT_Gun_Mk_II.mis
```

There is a global "TOE.ini" file. A campaign can provide a local "TOE.ini" file where the information of the global file can be overwritten for selected TOE.