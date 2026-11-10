# Level 1: Rock Harbor Station

Inspired by Coast Guard Station Quillayute River (La Push, WA): a surf station just inside a river mouth.
Everything here is fictional: the town of Rock Harbor, the Alder River, Kettle Island, and all landmarks.

## Map (Content/Maps/rock_harbor.json)
- Coast runs north-south; ocean to the west, land to the east. Map about 30 km x 46 km.
- The Alder River enters the sea between the North Jetty and Kettle Island. The bar sits just outside.
- Station and small harbor about 1.2 km inside the mouth; the town wraps the harbor.
- Sea stacks offshore: The Needles (south), Split Rock (north). Beaches: North Strand, Cove Beach.

## Core mechanic: the bar
- Breakers at the bar = offshore swell, steepened by an ebb current running out against it.
- Under 4 ft open, 4-8 ft restricted, 8 ft and over closed (tunable in Content/Rules/bar.json).
- The 29' RB-S may not cross above 4 ft breakers; the 47' MLB can (placeholder limit 20 ft).
- The player sees only the last bar report, which goes stale after 4 h. A bar check refreshes it.
- An unknown or stale bar raises the GAR Environment score.

## Slice incidents
Swimmer on rocks (rising tide), fishing vessel taking on water offshore, capsized river boater, hoax flare.

## How a case plays (sim, as built)
1. A call arrives with a reported position that can be hundreds of meters off. A heat map shows where the
   target probably is, and it drifts with the river or offshore current.
2. Optional: screen the call (callback). Exposes about 60% of hoaxes; confirms persons on genuine calls.
3. Order a **Search** on the case. The pattern is laid on the moving water, so it drifts with the target.
4. When a unit spots the target it breaks off and goes straight in. The case needs the right asset:
   surf rescues need the 47' MLB; a sinking fishing vessel needs a tow-capable boat to save the hull.
5. Survival clocks: people in the water have time set by water temperature (and a life jacket);
   people on rocks are safe until the tide covers them; a flooding vessel sinks at a set time.
6. Standing down a hoax earns readiness; standing down a real case fails it.

Balance check: one RB-S searching for a river capsize 1.5 km from the station finds and saves everyone in
96% of 200 seeded runs, average 8.5 minutes.
