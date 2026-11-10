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
