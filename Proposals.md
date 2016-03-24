# Introduction #

IL2 DCE proposals


# Details #
**Dan Antonescu**

- **TERROR mission type** (bombing cities/civil targets)

- **delayed take off/spawning for escort/intercept missions**:
> + player is escort/intercept fighter: place the bombers on a calculated reach-point in their flight-path in which they are in range/detected and at the moment player's escort/intercept airplanes are spawned. this must also take account of bombers' already consumed fuel

> + player is bomber: delayed spawn (and take-off) for the AI escort/intercept planes (spawn them only when the bombers are in range/detected)

- **AI ground control** / central command / master mind which should do all needed actions in real time (while the mission is going on):
> + generate & insert new recon/weather recon; CAPs; attacks + escorts; interceptions flights/ground groups/sea groups

> + deal with newly appeared/reported situations

> + allow special players (admins/commanders) send commands to AI ground control)

- **player commands**:
> + online wars commander(s) player(s) / high rank single player:  create ground/see/air groups and assign them missions (to intercept incoming flights, to attack certain targets, to patrol certain areas, to escort certain flights)

> + report enemy sightings (for recon missions and not only): quadrant, group type, size, composition, heading, altitude, speed

> + report bogey sightings and eventually change flight's current waypoint to investigate the newly sighted bogeys (force AI)

> + ask for help to (AI) ground control / central command (which should spawn more flights to deal with the wrongly asserted situation)

- **customized radar/ground control** (radars which are not 100% right (altitude, number of planes, directions, etc)

- **dynamic mission objectives** (take-off objectives which may be changed at some point(s) during the mission)

- **dynamic kill confirmation** (with chances of success depending if above friendly territory, if in certain distance from friendly aircrafts, friendly ground, etc)

- **historical aces pilots** (have historical aces populate their historical squadrons at historical dates/periods)

- **a points system** (based on player's deeds during the flight:
> + number of killed/damaged enemy airplanes/groundunits/bombing objectives (radars, ships, ammo depots, etc)

> + number of lost friendly escorted planes, protection/loss of own Rottenführer/Katschmarek (or other Schwarm partners if Schwarmführer)

> + keeping a maximum distance from own Rottenführer when flying as Katschmarek (and being warned/penalized when exceeding it)

- **kill tracks and characteristics advancement for all pilots** (RPG system) plus **rank promotions and rewards**

- **limited airplanes supply system** (continuous loses might not be replaced -> smaller aircraft numbers flights, older aircraft types reactivated)

- **new airplane types gradual infusion** (new aircraft types entering service at certain dates might be in small number, replacing only part of a squadron's flights)

- **manual player request for transfer** to another Geschwader

- **AI forcing** (nearby bogey detection (based on pilots visual skill), dynamic attack waypoint addition (forcing AI to intercept, identify and attack bogey groups)

- **RRR system** (repair/rearm/resupply) (if the player lands and taxies to certain locations/areas (close to a fuel cistern for example parked somewhere) on airfields, the script will move the player into a newly spawned aircraft and destroy the old landed one) - specially for online battles

- **dynamic weather generator** (at this moment, it might be possible to change weather conditions IF the new weather conditions from a newly loaded missions during the current one are overwriting/replacing the initial missions's weather characteristics)