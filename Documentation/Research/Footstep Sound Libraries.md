# Footstep Sound Library Research

**Purpose:** Compare free and paid footstep libraries against WhoDunBit's movement, surface and presentation requirements.  
**Version:** 1.0  
**Created:** 16 November 2026  
**Updated:** 16 November 2026  
**Status:** Research shortlist — no asset selected  
**Related documents:** [Game Design Document](../Game%20Design%20Document.md), [Technical Design Document](../Technical%20Design%20Document.md), [Documentation index](../README.md)

## 1. Requirements and research scope

The requirements supplied for this investigation are walking, **running as the default**, sprinting, jumping and landing. Core target surfaces include dirt, wood, grass, stone, metal and concrete. Additional surface coverage is valuable for varied maps. The desired sound is grounded and natural, consistent with the game's semi-realistic presentation.

This report compares publisher listings, available metadata and published licensing information. Full library downloads have not been auditioned or tested in-game. Recommendations are therefore based on coverage and suitability on paper; recording quality, consistency and final subjective fit require listening. No purchase, installation or change to the gameplay specification is made by this report.

Prices are publisher-listed USD amounts checked on 16 November 2026. Taxes and checkout prices may differ. Advertised sales may expire. File totals are not necessarily counts of distinct, usable footsteps: some packs include clothing Foley, bonus assets, long sequences or alternate formats. An advertised action is not proof that every surface and footwear combination has that action.

## 2. Recommended starting points

**Free foundation:** NOX Sound's Essentials Series, with its Freesound recordings used to fill specific action gaps. It offers a practical starting collection with particularly straightforward CC0 terms. Expect some surface/action editing and supplementation.

**Low-cost coherent base:** WASD Standard is the first paid pack to audition against the free options. Its surface list covers all six core targets, and its organised action banks are a useful fit for an interactive FPS. Test the free version before buying.

**More complete movement performances:** WOW Sound Essential Footsteps is the strongest candidate here for explicitly advertised walking, jogging, running, jumping and landing recordings. It needs a stone supplement and a check of the precise surface/action matrix before purchase.

These are recommendations for audition, not adopted asset choices.

## 3. Free libraries

### 3.1 NOX Sound — Essentials Series

**Price:** Free / optional contribution. **Published licence:** CC0.

The collection includes **Footsteps – Essentials: 479 individual sounds across 13 surfaces**, including dirty ground, grass, gravel, metal, mud, rock, sand, snow, tile, water and wood. Format is mono, 48 kHz, 24-bit. Concrete is not explicitly listed. The same download also includes **Iceland – Footsteps**, with 210 sounds and walk/run/jump performances on seven outdoor surface variants. [Publisher listing and licence](https://nox-sound-design.itch.io/essentials-series-sfx-nox-sound)

The publisher's [Freesound pack](https://freesound.org/people/Nox_Sound/packs/29889/) contains additional source sequences. The [rock sequence](https://freesound.org/people/Nox_Sound/sounds/558812/) explicitly includes walk, run, sprint and jump material. There are separate [rock sprint](https://freesound.org/people/Nox_Sound/sounds/558468/) and [rock jump/landing](https://freesound.org/people/Nox_Sound/sounds/558477/) recordings. [Grass](https://freesound.org/people/Nox_Sound/sounds/556042/) has walk/run and ten jump-and-land performances.

**Assessment:** Strong free starting point, but not a uniform five-action grid. Some Freesound files require cutting individual hits from sequences, and direct downloads require an account. Do not assume the prepared itch.io collection includes every Freesound recording. Audit individual surfaces for missing takeoffs, landings and sprint variations.

### 3.2 WASD Sound — Free Integrated Footstep SFX Bundle

**Price:** Free / optional contribution. **Published terms:** Commercial use allowed; no AI training.

144 files: boots on dirt, grass, stone and wood, with six variations per surface/action. Advertised banks are walk, sneak, run, jump, drop and shuffle. WAV, 48 kHz, 24-bit, described as mono-compatible. Unity scripts are included, but the audio is usable independently. [Publisher listing, demo and terms](https://wasd-sound.itch.io/free-integrated-footstep-sfx-bundle)

**Assessment:** Useful audition for the paid Standard pack and a coherent four-surface prototype. Missing metal and concrete. There is no separately advertised sprint bank; audition the Drop bank as a possible landing source. The linked full licence could not be retrieved during this investigation, so detailed redistribution terms remain unchecked.

### 3.3 Dan Sfx — Free Sound Pack 02: Footsteps

**Price:** Free / optional contribution. **Published terms:** Commercial use permitted; attribution optional.

Advertises 756 files over 42 surfaces, including all six core WDB targets plus carpet, tiles, water, snow, mud and several wood/metal variants. Format is mono, 48 kHz, 24-bit. The listing describes 18 step/scuff/walk variations per surface; it does not establish 18 variations for each movement action. [Publisher listing and terms](https://cdansantana.itch.io/fsp2fs)

**Assessment:** Broad free surface coverage worth investigating. A complete walk/run/sprint/jump/landing matrix is unverified. Published 8 November 2026, so this is a new candidate rather than a proven long-term recommendation. Its AI disclosure identifies graphics; it does not identify the audio as generated.

## 4. Paid libraries

### 4.1 WASD Sound — Standard Integrated Footstep SFX Bundle

**Price:** $24.99. **Coverage:** 432 files, 12 materials; all six core surfaces, plus gravel, mud, planks, rubble, sand and water.

Boot recordings provide six variations per surface/action across walk, sneak, run, jump, drop and shuffle. WAV, 48 kHz, 24-bit, described as mono-compatible. Unity integration is included. Commercial use is stated; the linked full licence was not retrievable. [Publisher listing](https://wasd-sound.itch.io/integrated-footstep-sfx-bundle)

**Assessment:** Best low-cost single-pack candidate in this shortlist. No dedicated sprint bank is advertised. Confirm the Drop sounds suit WDB's landings. The free pack provides a low-risk way to audition the recording style. Existing scripts are optional and need not dictate WDB's movement/audio architecture.

### 4.2 WOW Sound — Essential Footsteps Sound Effects Pack

**Price:** From $99. **Coverage:** 3,263 mono files, including movement and gear Foley; three footwear types and up to 20 variations.

Explicitly advertises walking, jogging, running, jumping, landing and scuffing. Surfaces are concrete, dirt, grass, metal, water and wood, with solid/hollow variants. Stone is not listed. Audio is 96 kHz/24-bit WAV with OGG versions. The page provides previews and watermarked trial files. [Publisher listing, demos and licensing options](https://wowsound.com/p/essential-footsteps-sound-effects-pack/)

**Assessment:** Strong movement-coverage candidate. Walking/jogging/running offer three performance families to evaluate against WDB's walking/default running/sprinting; the mapping needs listening. Confirm every required combination rather than inferring it from the total count. One-time and subscription licences have different conditions; evaluate a one-time game-use purchase for this comparison.

### 4.3 Epic Stock Media — Game Footsteps: Walk Run Jump Foley SFX

**Price:** $44. **Coverage:** Approximately 1,650 effects across 22 surface/footwear categories, with boots and sneakers. The page alternates between 1,655 and 1,656 files; its 22 categories are not 22 distinct ground materials.

Lists slow walking, walking, running and jumping for most footsteps. Materials include concrete, dirt, gravel, metal, multiple woods, brick and cobblestone. Astro turf is listed rather than natural grass. Includes 96 kHz/24-bit and 44.1 kHz/16-bit WAV versions. [Publisher listing and file list](https://epicstockmedia.com/product/game-footsteps-sound-effects/)

**Assessment:** Useful studio-Foley alternative, especially for urban maps. Natural grass and separately identified landing banks need checking. It is less obviously complete for the requested movement/surface grid than WASD or WOW.

### 4.4 Gamemaster Audio — Footstep and Foley Sounds

**Price:** $10. Includes 511 footsteps on ten surfaces, 141 movement/clothing/equipment Foley sounds and 160 bonus sounds, for 812 files overall. Surfaces include concrete, dirt, grass, gravel, metal, mud, water, wood, ice and snow. Stone is not listed. Format is 16-bit/48 kHz WAV; commercial royalty-free use is stated. [Publisher listing](https://gamemaster-audio.itch.io/footstep-and-foley-sounds)

**Assessment:** A budget source of basic steps and layering material. Separate gait, jump and landing coverage is not specified clearly enough to recommend it as the complete solution. The publisher also lists a $47 wider collection containing this pack; check overlap before purchasing multiple products.

### 4.5 Sound Works 12 — Footsteps Sound Kit

**Price:** $7.99 regular; the retrieved page also displayed a $1.59 Halloween sale, which should not be assumed available at checkout.

290 sounds on seven advertised surfaces: concrete, sand, wood, grass, carpet, snow and undergrowth. Explicitly lists walking, running, sneaking and sprinting. Dirt, stone, metal and separate jump/landing coverage are not listed. [Product page](https://sound-works-12.itch.io/footsteps-sound-kit); the [publisher profile](https://sound-works-12.itch.io/) states commercial royalty-free use.

**Assessment:** An inexpensive source of explicitly separate sprint performances, but a supplement rather than a six-surface solution. The linked full sound list was not accessible in this investigation.

## 5. Larger and specialist options

| Library | Listed price | Relevance and limitations |
|---|---:|---|
| [WASD Open World](https://wasd-sound.itch.io/integrated-open-world-footstep-bundle) | $149 | Advertised 32 materials, over 1,100 files and the same six action categories as Standard. Adds carpet, tiles, metal variants and wet surfaces. One boot style; no separately advertised sprint category. Consider when extra map surfaces justify the jump in cost. |
| [Epic Stock Media Designed Game Footsteps](https://epicstockmedia.com/product/designed-game-footsteps/) | $59 sale / $99 regular | 5,442 effects, including human, creature, monster and stylised material, with paired walk/run sets for many designs. Promising later for transformed Monsters. No complete jump/landing grid is established; much of the pack is outside ordinary human movement needs. |
| [Pro Sound Effects Game Audio Collection: Footsteps](https://www.prosoundeffects.com/libraries/game-audio-collection-footsteps) | $299, one user | 10,695 files including 9,500 foot sounds; six footwear styles, all six core surfaces, left/right separation and up to five variations. Includes clothing and other Foley. Extensive coverage, but exact action coverage was not verified: the metadata spreadsheet could not be retrieved. Not the first purchase recommended for WDB. |

The Designed Game Footsteps [metadata PDF](https://epicstockmedia.com/wp-content/uploads/2024/04/Designed_Game_Footsteps_24bit.pdf) is available for inspecting specific designs before purchase.

## 6. Core surface coverage

This table describes explicitly named material families, not verified completeness for every movement action. Rock is grouped with stone for comparison; their final sonic suitability still needs auditioning.

| Library | Dirt | Wood | Grass | Stone/rock | Metal | Concrete |
|---|---|---|---|---|---|---|
| NOX Essentials | Listed | Listed | Listed | Rock | Listed | Not listed |
| WASD Free | Listed | Listed | Listed | Listed | Not included | Not included |
| Dan Sfx | Listed | Listed | Listed | Listed | Listed | Listed |
| WASD Standard | Listed | Listed | Listed | Listed | Listed | Listed |
| WOW Essential | Listed | Listed | Listed | Not listed | Listed | Listed |
| Epic Game Footsteps | Listed | Listed | Astro turf only | Cobblestone | Listed | Listed |
| Gamemaster Audio | Listed | Listed | Listed | Not listed | Listed | Listed |

Sources are the linked publisher listings in sections 3–4. “Not listed” is a research limitation, not proof that no usable approximation exists in the download.

## 7. Suggested WDB sound approach

These are recommendations for evaluation, not changes to adopted gameplay rules.

- Prefer isolated steps for responsive starts/stops, arbitrary movement speeds and surface transitions. Long recordings can still supply good individual hits after editing; premade loops are less flexible as the main player-movement source.
- Treat movement labels as performance categories. A library's Jog could fit WDB's default Run, with its Run providing Sprint. Compare the actual impact and cadence rather than matching filenames literally.
- A two-gait library can support three gameplay speeds using event cadence, intensity and selected heavier/lighter variations. Volume alone cannot provide a genuinely different impact texture. Avoid making extreme pitch changes the principal distinction.
- Keep takeoff and landing as separate evaluation needs. Landings may need their own impacts or layered body/clothing weight, especially on hard surfaces. Do not assume a folder named Jump contains cleanly separated events.
- Start with one consistent human footwear family across the six core surfaces. Add other footwear or Monster layers when they provide enough audible benefit to justify the content and tuning work.
- Cosmetic character models should not automatically imply different faction audio. Any future role/Monster-specific sound information needs deliberate game design.
- Prioritise clarity at everyday running cadence: that is the state players will hear most. Ensure variation does not overpower speech, gunfire or investigative sounds.

## 8. Licensing considerations

NOX publishes the free series and the inspected Freesound recordings under CC0. This offers the most flexible candidate here for reusable examples or materials that may need to be shared with modders. [Series licence](https://nox-sound-design.itch.io/essentials-series-sfx-nox-sound), [inspected recording licence](https://freesound.org/people/Nox_Sound/sounds/558468/).

For paid libraries, use in a shipped game and redistribution of the raw WAV files are different permissions. Epic's licence permits game use, but limits raw sharing and requires appropriate user coverage. Keep this distinction in mind for collaborators and a future SDK. [Epic licence](https://epicstockmedia.com/licensing-agreement/).

WOW distinguishes permanent purchases from subscriptions, with subscription project-release conditions, and does not permit standalone asset redistribution. Its advertised base game-use SFX tier is for one user. [WOW product licensing information](https://wowsound.com/p/essential-footsteps-sound-effects-pack/).

WASD advertises commercial use, but the full linked licence was unavailable during research. Dan Sfx and Sound Works 12 publish commercial-use statements; their detailed redistribution terms were not independently established. Preserve the actual licence supplied with any selected download.

## 9. Audition plan

1. Try NOX Essentials and WASD Free against the same short movement sequence: walking, default running, sprinting, takeoff, light landing and heavy landing. Include frequent starts/stops and surface transitions.
2. Compare natural grass, stone and wood first, since both candidates cover those families. Listen for repetitive impacts, ambient noise, excessive footwear character, inconsistent loudness and unnatural processing.
3. Test metal and concrete next. Use WASD Standard as the first paid candidate if the free recording style works; compare WOW's trial files if three genuinely distinct gait performances are important.
4. Evaluate sounds both as the local player's footsteps and as another player moving nearby. Check their usefulness as spatial/gameplay information and their balance against voice and gunfire.
5. Audit the downloaded surface/action folders before committing to a pack. Identify gaps explicitly, then decide whether editing, layering or a supplementary pack is the more practical solution.

## 10. Revision history

| Date | Revision |
|---|---|
| 16 November 2026 | 1.0 — Compared free, budget and larger footstep libraries; recorded verified coverage, uncertain action matrices, licensing distinctions and an audition plan. |
