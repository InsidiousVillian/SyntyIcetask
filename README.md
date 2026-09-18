# GNOME GUARD

**Protect the Christmas tree. Freeze the horde. Don’t drop Christmas.**

A cozy-chaos first-person snowball brawler built in Unity 6 with [Synty POLYGON Holiday Gnomes](https://www.syntystudios.com/). Zombie gnomes rise out of the snow and march the tree. You throw, charge, kick, and grab holiday gnomes for power-ups until the lights go out.

| | |
|---|---|
| **Status** | Playable prototype |
| **Engine** | Unity 6 (URP) · New Input System |
| **Repo** | Private · [InsidiousVillian/SyntyIcetask](https://github.com/InsidiousVillian/SyntyIcetask) |
| **Target** | $5–10 holiday arcade night · Steam / Itch |
| **Session** | 15–40 minutes |
| **Vibe** | Cute *Vampire Survivors* snack in a snowball FPS body |

---

## Play it now

1. Open the project in **Unity 6.0.3** (or the version in `ProjectSettings/ProjectVersion.txt`).
2. Open `Assets/Scenes/SampleScene.unity`.
3. Click the **Game** view, then press **Play**.
4. Click or press **Space** to start.

### Controls

| Action | Input |
|---|---|
| Move | **WASD** |
| Look | Mouse |
| Throw | Click |
| Charge throw | Hold click, release |
| Kick | **F** / **E** / **Q** |
| Sprint | **Shift** |
| Jump | **Space** |
| Pause | **Esc** |
| Restart | **R** after the tree falls |

### The loop

- **Zombie gnomes** spawn at the edge, rise from the snow, and eat the tree.
- **Rushers** from wave 3, **tanks** from wave 4, a **King Gnome** every 5 waves.
- Clear a wave with **no tree damage** for a Perfect Night (+HP, +score).
- **10 combo** stuns the field.
- Holiday gnomes drop as pickups:

| Gnome | Power |
|---|---|
| Wizard | Freeze |
| Soldier | Rapid snowballs |
| Santa | Repair the tree |
| Dwarf | Score burst |
| Beach | Slow-mo |
| Theorist | Bonus hits |
| Basalt | Tree shield |

---

## What this is (honest)

This is a **jam-quality prototype with store potential**, not a shippable 1.0.

It already has juice: orbiting title camera, charge throws, viewmodel snowball, fairy lights that die with the tree, ice shells on freeze, combo stun, last-gnome callout, procedural music bed.

It does **not** yet have a real build pipeline (assets load in-editor), menus, settings, licensed audio, distinct enemy silhouettes, meta progression, or a second arena.

The product we sell is not “more waves.” It is a **15–40 minute holiday night** you replay for loadouts, stars, and “one more King.”

---

## Versioned roadmap

Dates start **Friday 18 September 2026**. Holiday launch wants the game *felt* by mid-November.

### Prototype — now → 28 Sep 2026
**Goal:** the loop is addictive for 10 minutes without explaining it.

- [x] Playable FPS snowball defense
- [x] Waves, combo, pickups, King Gnome
- [x] GitHub repo + this living README
- [ ] Friends-and-family playtest notes in `/docs/playtests` *(folder TBD)*
- [ ] Sensitivity, FOV, and volume as a pause panel
- [ ] Distinct enemy *looks* (Soldier tank, Beach rusher, etc. — stop rescaling the same zombie)
- [ ] Editor-independent prefabs so Play works the same in a build

**Exit:** 5 people finish a run without being told the controls.

### Vertical slice / demo — 29 Sep → 26 Oct 2026
**Goal:** something you can send a streamer or put on Itch without apology.

- [ ] Real **Windows build** (no `AssetDatabase`)
- [ ] Title: New Run / How to Play / Settings / Credits
- [ ] Licensed or commissioned **music + foley** (replace the beep bed)
- [ ] Gamepad + rebinding
- [ ] 3 enemy types that read in a screenshot
- [ ] 1 extra arena lighting pass (frozen pond *or* night market)
- [ ] Run recap: wave, score, perfects, best combo
- [ ] Trailer loop on the title screen (~90s)

**Exit:** a 3-minute trailer and a build a stranger can install.

### 1.0 — 27 Oct → 24 Nov 2026
**Goal:** a $5–10 Christmas arcade people finish and *restart*.

- [ ] Loadouts before a run (starting gnome kit: Soldier / Wizard / Santa…)
- [ ] Meta currency from runs
- [ ] Campaign of **10 nights** + Endless
- [ ] 3 maps from the same kit
- [ ] Between-wave gnome turrets / lanterns / fences
- [ ] Achievements
- [ ] EN + at least one more language
- [ ] Steam page, capsules, 5 screenshots, wishlists
- [ ] Launch week patch buffer

**Target window:** **24–30 November 2026** (US Thanksgiving / Black Friday / “Christmas games” search spike).

### Live 1.x — December 2026
**Goal:** don’t go dark on Christmas week.

- [ ] Day-one crash / sensitivity patch
- [ ] Christmas sale asset
- [ ] One extra night or modifier (“Blizzard”, “Tiny Tree”)
- [ ] Wishlist / review reply pass

---

## Playtest calendar

All dates are from **today, 18 September 2026**. Move a column, don’t skip the ritual: **build → 5 players → 10 notes → patch**.

| When | Name | Who | What we need to learn |
|---|---|---|---|
| **18–19 Sep** | Desk juice | Us | Does charge-throw beat tap-throw? Is kick readable? |
| **20–21 Sep** | Friends & family | 4–6 people who are *not* on the project | Can they start from the title without a Slack message? Where do they die first? |
| **22–28 Sep** | Closed prototype #1 | 8–12 | Average session length, “I’d play again” %, control complaints |
| **29 Sep – 5 Oct** | Tune week | Us | No new features. Only what playtest #1 screamed about. |
| **6–12 Oct** | Build playtest | Same group + 5 new | Does a **build** (not the editor) still feel good? Install friction. |
| **13–19 Oct** | Content slice | Mix of cozy / FPS players | Do distinct gnomes + a second look-space keep minute 8 interesting? |
| **20–26 Oct** | Demo freeze | Wider (Itch / Discord / campus) | Trailer vs reality. Refund-risk moments. Audio fatigue. |
| **27 Oct – 2 Nov** | External #2 | Creators / “would I wishlist this?” | Store-page screenshot test. Name, price, 1-line pitch. |
| **3–16 Nov** | Polish lock | Us + 3 trusted | Bugs only. Accessibility pass. |
| **17–23 Nov** | Keys | Press, friends with audiences | 90-second clips. Wishlists. |
| **24–30 Nov** | Launch | Public | Crash rate, review sentiment, “too short” vs “perfect snack.” |
| **1–25 Dec** | Holiday live | Players | One content drop if the loop is holding. |

### Playtest script (steal this)

1. Don’t explain. Hand them the build.
2. Watch the first 60 seconds in silence.
3. After the tree falls, ask only:
   - What was the game about?
   - When did you feel powerful?
   - When did you feel cheated?
   - Would you pay $8 for a week of this in December — why not?
4. Log in a table: `date | player | wave reached | fun 1–5 | notes`.

---

## Future additions (backlog)

Ship 1.0 without these. Steal from this list when a playtest says the night ended too soon.

### Combat & toys
- Ice shard alt-fire, gift bomb, shovel viewmodel
- King Gnome slam, Wizard blizzard, corrupted-Santa raid (telegraphed bosses)
- Enemy healer you must *not* snowball
- Aim assist toggle, freeze slowdown on last gnome

### Meta & modes
- Daily seed / weekly challenge leaderboard
- 3-star nights: no damage / time / combo
- Nightmare modifiers for streamers
- Kid difficulty (tree HP, slower rushers)
- Couch co-op: one throws, one kicks / repairs the tree

### World & juice
- Footprints, snowball decals, broken ornaments, tree damage stages
- Palette-swap hats from the 12 Synty texture sheets (cheap cosmetics)
- Title-screen snowfall that sells the trailer
- “Perfect Night” medal recap with gnome portraits

### Platform
- Steam achievements, cloud save, overlay
- Trading cards, capsule, library art
- Linux / Steam Deck verify
- Localization: ES / DE / JA after EN is frozen

### Explicitly later / maybe never
- Open world, crafting, gacha, live-service seasons
- Huge narrative
- More random pickups before loadouts exist

---

## Budget (fill later)

Working envelope for a **small holiday 1.0**, not a studio pitch. Numbers are **placeholders** — replace with real quotes as we lock vendors.

**Target sale price:** $7.99 (sale $4.99 in December).  
**Wishlists before launch (stretch):** 1,000.  
**Time:** ~10 weeks calendar, evenings + weekends, 1 lead + contractors.

| Bucket | Prototype (now) | Demo | 1.0 | Notes / fill in |
|---|---:|---:|---:|---|
| Your time | $0 (sweat) | TBD | TBD | Log hours from 18 Sep |
| Unity / store fees | $0 | Steam $100 | Steam 30% cut | Plus VAT where it applies |
| Synty / extra kits | Owned | TBD | TBD | Don’t buy packs until 3 maps are blocked |
| Music & SFX | $0 (proc) | **$400–1,200** | **$800–2,000** | Highest refund-prevention spend |
| UI / icons / store art | $0 | **$200–600** | **$500–1,500** | Capsule decides the click |
| Trailer edit | $0 | **$0–400** | **$200–800** | Can be in-house if footage is good |
| Localization | $0 | $0 | **$300–1,000** | After EN copy freeze |
| QA / playtest keys | $0 | Pizza + builds | **$100–400** | Don’t skip |
| Ads / influcencer | $0 | $0 | **TBD after wishlists** | Only if page converts |
| Buffer (20%) | — | — | **TBD** | Day-one patch, broken Deck, tax |
| **Running total** | **~$0 cash** | **TBD** | **TBD** | Update this row every Friday |

### Cash rules
1. Spend on **audio and store art** before more enemy types.
2. No ads until the Steam page has a trailer and 5 screenshots.
3. Co-op is a December *stretch*, not a November blocker.
4. Revisit this table on **26 Oct** (demo freeze) and **17 Nov** (keys).

---

## Store pitch (draft)

> **Gnome Guard** is a first-person snowball defense game about one bad night in the yard. Zombie gnomes want the tree. You have snowballs, a kick, and a lawn full of holiday misfits. Fifteen minutes to a Perfect Night — or until the lights go out.

**Tags (Steam draft):** Action · Casual · FPS · Arcade · Cute · Comedy · Singleplayer · Score Attack · Seasonal

**Do not promise in the first trailer:** co-op, ten maps, a novel.

---

## Tech snapshot

```
Assets/GnomeGuard/
  Scripts/     runtime loop (player, waves, HUD, tree FX)
  Editor/      one-time FBX import helper
Assets/POLYGON_Holiday_Gnomes_SourceFiles_v2/
  FBX + palettes used as the whole visual identity
```

Known ship blockers:

- Runtime loading uses the Unity **editor** asset database.
- World is built procedurally at Play, not authored as a scene of prefabs.
- Audio is generated in code.
- Pause is a banner, not a menu.

---

## Credits

- **Game** — InsidiousVillian / this repo
- **Art** — Synty Studios, POLYGON Holiday Gnomes (source files v2)
- **Engine** — Unity URP

Holiday gnome meshes are Synty’s. Don’t redistribute the pack as a pack. The **game** is the product.

---

## License / repo

Private repository. Source is for development of Gnome Guard, not for republishing the Synty kit.

---

*Living doc. Last dated 18 September 2026. Next ritual: friends-and-family playtest 20–21 Sep.*
