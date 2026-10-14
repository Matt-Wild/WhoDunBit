# WhoDunBit — Game Design Document

**Purpose:** Define WhoDunBit's player experience, gameplay systems and content direction.  
**Version:** 0.3  
**Created:** 14 October 2026  
**Updated:** 14 October 2026  
**Status:** Pre-production design draft  
**Related documents:** [Technical Design Document](Technical%20Design%20Document.md)

This document defines WhoDunBit's player experience, gameplay systems and content direction. Proposed features are identified where relevant; unresolved rules are marked **TBD**. Numerical balance values are provisional and subject to playtesting.

## 1. Product vision

WhoDunBit is a multiplayer social deduction FPS in which humans must identify and eliminate supernatural enemies hiding among them. It combines the gunplay, suspicion and unpredictable social encounters of **Trouble in Terrorist Town (TTT)** with the role variety, bluffing, misleading evidence and information asymmetry of **Blood on the Clocktower (BOTC)**.

Monsters can participate in ordinary human activities and use conventional equipment before revealing supernatural powers. Their human collaborators, Minions, add hostile players without adding equally powerful supernatural combatants.

The defining deception mechanic is **Human Forms**: Monsters and Minions receive genuine Bystander role capabilities. A Monster claiming to be a Medic can actually heal people and use the Medic shop. Demonstrating a claimed role's basic abilities should not establish innocence.

| Property | Specification |
|---|---|
| Name | WhoDunBit; current preferred name, pending legal and commercial clearance |
| Genre | Multiplayer social deduction FPS |
| Initial platform | Windows PC |
| Intended distribution | Steam |
| Business model | Commercial standalone game |
| Initial hosting direction | Friend-hosted, host-authoritative listen server with a P2P-style experience |
| Longer-term hosting goal | Community-hosted dedicated servers |
| Foundational priority | Extensive community modding and host configurability |

WhoDunBit draws on these inspirations while using original code, assets, branding and implementations. Detailed art direction, setting, narrative, age rating, controller support and accessibility requirements are TBD.

## 2. Design pillars

1. **Easy to learn, difficult to master.** Players should quickly understand the basic objective. Role combinations and deduction create depth, while basic Bystander play remains enjoyable without advanced role knowledge.
2. **Information is a resource.** Deaths, identities and allegiances should be discovered when gameplay requires it, rather than unnecessarily disclosed by the interface.
3. **Believable deception.** Functional Human Forms prevent simple ability demonstrations from reliably exposing Monsters.
4. **Rare, dangerous Monsters.** Each Monster should feel powerful while remaining vulnerable to coordinated human opposition.
5. **Sensible defaults, extensive configuration.** A friend group should be able to start without setup work; advanced hosts should have substantial control.
6. **Minimal downtime.** Preparation and role allocation happen while players explore and interact with the map.
7. **Emergent interactions.** Deception, cooperation and unusual tactics should emerge from interacting systems, without bespoke rules for every scenario.
8. **Gameplay before unnecessary complexity.** Validate a fun core experience early and expand incrementally. Modular systems support that growth.

## 3. Core round experience

A typical round proceeds as follows.

1. Players join a lobby using the host's chosen rules, faction distribution and role availability.
2. Preparation begins. Players spawn around the map and immediately learn their faction.
3. Players explore, gather weapons and equipment, and interact with the environment.
4. Players receive or choose primary roles within their assigned factions.
5. Monsters and Minions then receive or choose functional Human Forms from remaining eligible Bystander capacity.
6. A final preparation period lets players read their full role information and organise equipment.
7. The active round begins. Humans investigate and fight hostile players; Monsters and Minions deceive and eliminate their opponents.
8. A victory condition ends the round.
9. The post-round summary reveals previously hidden information. Karma incidents are evaluated and victims can forgive attackers.
10. The next round begins.

Rounds feature first-person movement and firearms, randomised map spawns, map weapons and equipment, specialised abilities, shops, evidence, corpse interactions, transformations and player communication.

**TBD:** Exact victory conditions, time-expiry outcomes, round duration, maximum lobby size, starting loadouts, weapon distribution and release role roster. Victory rules must also address cases such as Minions surviving after all Monsters die.

## 4. Factions, allegiance and team knowledge

### 4.1 Four separate factions

Bystanders, Enforcers, Monsters and Minions are four distinct factions.

| Faction | Standard victory allegiance | Intended function |
|---|---|---|
| Bystanders | Human side | Ordinary humans with functional roles; investigate, support and fight |
| Enforcers | Human side | Publicly trusted or verifiable specialists in investigation or protection |
| Monsters | Monster side | Hidden supernatural enemies with Human Forms and Monster capabilities |
| Minions | Monster side | Human collaborators with Human Forms, without inherent Monster transformations |

Faction identity and victory allegiance are different concepts. Future mods may introduce additional factions or more complex allegiance relationships.

### 4.2 Bystanders

Bystanders are generally the most numerous faction. They cannot inherently identify one another. They use ordinary firearms and equipment alongside role abilities that may provide support, investigation or defence. They must identify hostile players without indiscriminately killing allies.

### 4.3 Enforcers

Enforcers are specialised human roles, comparable to TTT Detectives, whose genuine allegiance is publicly known or verifiable. They have investigative or protective capabilities, specialised equipment and value as trusted coordination points. This makes them important targets.

A Shapeshifter may impersonate an Enforcer when host configuration permits it. Verification therefore needs to account for impersonation. **TBD:** The exact verification mechanic.

### 4.4 Monsters

Monsters are relatively few and individually powerful. Each has a Monster role and a functional Human Form, may use ordinary weapons while human, and has supernatural abilities and role-specific mutations. Transformation behaviour depends on the Monster role.

Monsters always know the identities of fellow Monsters, including through disguises and transformations. This rule is mandatory. Teammate recognition does not automatically reveal locations through walls.

Monster-on-Monster damage and negative ability effects are disabled, including relevant poison, incapacitation and harmful status effects. Monsters can harm and kill Minions.

### 4.5 Minions

Minions are human collaborators on the Monster side and form their own faction. Initially it contains only the primary role **Minion**. They have functional Human Forms and the corresponding equipment shops, no inherent Monster transformation, and no mutation shop by default.

Minions add deception and permit finer hostile-side balancing than adding another full Monster. They can die to Monster attacks.

The following knowledge relationships are intended to be independently configurable:

- Whether Minions know Monsters.
- Whether Monsters know Minions.
- Whether Minions know other Minions.

**TBD:** Defaults for these settings. Monster-to-Monster recognition remains mandatory.

These knowledge settings support uncertain alliances and social bluffing. For example, a Monster may be unable to verify a player's claim to be a Minion, while an innocent Sinner could exploit that uncertainty. A Minion could also sacrifice themselves to provide a Shapeshifter with a disguise. Such tactics emerge from existing mechanics.

## 5. Functional Human Forms

### 5.1 Meaning and scope

A **Human Form** grants a Monster or Minion the functional capabilities of a Bystander role, allowing them to perform that role convincingly while pursuing a different allegiance.

A Human Form provides the relevant abilities, passive capabilities, role interactions, role-specific equipment and equipment shop. It does not change actual faction, victory allegiance, Monster role, supernatural transformations or mutation selection.

A genuine Bystander Medic and a Vampire with a Medic Human Form can both perform genuine Medic activities. Their allegiance and additional supernatural capabilities differ.

Example combinations using the proposed role roster:

| Actual faction | Primary role | Human Form / ordinary human role |
|---|---|---|
| Bystander | Medic | Medic |
| Bystander | Civilian | Civilian |
| Monster | Vampire | Medic |
| Monster | Werewolf | Civilian |
| Monster | Shapeshifter | Investigator |
| Minion | Minion | Medic |

### 5.2 Shared capacity

Bystander primary roles and hostile Human Forms consume the same configured Bystander role capacity. Genuine Bystander assignments resolve first.

For example, with two Medic slots, a Bystander Medic occupies one and a Monster's Medic Human Form can occupy the other. Medic is then unavailable to additional Monsters or Minions. Availability updates as selections occur, and the host can configure capacities and restrictions.

**TBD:** Default capacities, duplicate policy, role-specific restrictions, insufficient-capacity fallback and visibility of capacity information. The two Medic slots above are an example configuration.

### 5.3 Possible combinations with supernatural abilities

**Proposed extension:** Mutations may interact with Human Form tools, such as allowing a Monster Medic to poison their own medical equipment. Other possibilities include sabotaging support equipment or corrupting evidence. Release scope for these interactions is TBD.

## 6. Role assignment and player choice

### 6.1 Default allocation

Primary roles and hostile Human Forms are random by default. This reduces metagaming based on a player's known role preferences.

The host can independently enable primary role selection and Monster/Minion Human Form selection. Both options default to disabled. Enabling choice does not let a player choose their faction; primary role choice takes place inside the assigned faction.

| Primary role selection | Human Form selection | Behaviour |
|---|---|---|
| Disabled | Disabled | Both assigned randomly |
| Enabled | Disabled | Players choose primary roles; hostile Human Forms assigned afterwards |
| Disabled | Enabled | Primary roles assigned; Monsters and Minions choose Human Forms afterwards |
| Enabled | Enabled | Primary role choices resolve before hostile Human Form choices |

### 6.2 Mandatory sequence

Primary role allocation must finish before Human Form allocation begins.

In the primary stage, Bystanders, Enforcers, Monsters and Minions receive or select roles within their factions. Bystander assignments consume Bystander capacity. In the second stage, Monsters and Minions receive or select Human Forms from the remaining eligible capacity.

Players never have to open a selection menu. Unselected roles are randomly assigned when the relevant timer expires.

**TBD:** Handling of exhausted capacity, simultaneous requests, invalid selections, disconnects, incomplete choices, late joins and unusual host configurations. Automatic assignment requires an eligible role; fallback rules for impossible configurations remain to be designed.

## 7. Interactive preparation

### 7.1 Permitted activity and protections

Players spawn into the map and remain active during preparation while role allocation takes place.

They can move, run, explore, pick up weapons, gather equipment, fire weapons and interact with appropriate environmental objects. They cannot damage one another, physically block one another or apply disruptive hostile effects to other players.

**TBD:** Environmental damage, ability use, ammunition replenishment, other resource consumption, inventory persistence into the active round, shop access and detailed environmental restrictions.

### 7.2 Timing

The following provisional defaults are host-configurable.

| Preparation stage | Automatic allocation | Player selection enabled for that stage |
|---|---:|---:|
| Initial preparation | 5 seconds | 5 seconds |
| Primary role stage | 5 seconds | 15 seconds |
| Human Form stage | 5 seconds | 15 seconds |
| Final preparation | 10 seconds | 10 seconds |

This produces 25 seconds with both stages automatic, 35 seconds with one selection stage enabled, and 45 seconds with both enabled. Final preparation gives players time to read roles and organise equipment.

### 7.3 Progressive information and interface

Players learn their faction at preparation start, their primary role and relevant abilities after primary allocation, and their Human Form and related equipment/abilities after Human Form allocation. Players have complete role information during final preparation.

Selection menus open through a configurable hotkey. Players can ignore them and receive automatic assignments. The interface should support quick navigation and may use a discreet availability indicator.

Other players must not learn who is selecting what through the selection interface. Forcing only Monsters to stop moving for a menu could disclose their faction. Ordinary movement should continue unless the player chooses to interact with a menu.

**TBD:** Exact interface layout, transition feedback and timing of shop access.

## 8. Combat, transformations and identity

### 8.1 Ordinary combat

First-person firearms and map equipment underpin play across factions. Human-side players need to avoid indiscriminate friendly violence; Monsters have powerful supernatural options in addition to human cover.

**TBD:** Movement tuning, weapon catalogue, damage values, ammunition model, inventory limits, other friendly-fire rules and detailed health/death behaviour.

### 8.2 Human and monstrous forms

While human, Monsters can use functional Bystander abilities, ordinary weapons, Human Form equipment and shops, plus supernatural abilities permitted in human form.

Entering a monstrous form disables Human Form abilities and provides the supernatural form's capabilities. Equipment compatibility depends on the specific Monster and item.

Revealing supernatural power should involve a meaningful loss of deception. Reversibility depends on role: some transformations may normally be irreversible, while others may permit a return to human form.

**Proposed form designs:** A transformed Vampire could retain firearms and compatible equipment while gaining supernatural combat and movement. A Werewolf could gain melee power and durability while losing firearms.

**TBD:** Transformation times, cooldowns, exact reversibility, costs, equipment compatibility and finishing rules for resistant Monsters.

### 8.3 Shapeshifter distinction

A Shapeshifter's identity changes preserve its functional Human Form abilities. It retains its assigned Human Form when adopting a disguise; consuming a victim does not grant their role abilities.

True identity, presented identity and active ability/physical form must remain distinct concepts.

## 9. Role concepts

The roster below is proposed. Individual abilities remain under development, with the Shapeshifter's core identity-theft mechanics defined in section 9.3. Final launch roster is TBD.

### 9.1 Bystander concepts

| Role | Intended niche | Candidate mechanics / uncertainty |
|---|---|---|
| Civilian | Simple, approachable role | Minimal special abilities; exact baseline unspecified |
| Sinner | Innocent source of misleading supernatural evidence | May confuse detection, produce suspicious clues or bluff as a Minion; exact signature mechanics open |
| Medic | Support | Healing, medical supplies and support abilities; genuine capabilities also granted by its Human Form |
| Investigator | Evidence gathering | Ordinary Bystander investigation; distinction from Enforcer Detective unresolved |

### 9.2 Enforcer concepts

| Role | Intended niche | Candidate mechanics / uncertainty |
|---|---|---|
| Detective | Investigation | Body inspection, supernatural traces and evidence analysis; exact tools open |
| Sentry | Protection and area control | Defensive equipment, deployables and protecting others; exact tools open |

Investigation should generally supply clues rather than automatic, guaranteed answers.

### 9.3 Monster concepts

**Werewolf — proposed combat specialist.** Potential mechanics include a powerful transformation, strong melee attacks, durability, regeneration and restricted or disabled firearms while transformed. One candidate mechanic lets ordinary bullets incapacitate the Werewolf while specialised methods finish it permanently. Exact abilities and finishing methods are TBD.

**Vampire — proposed stealth, assassination and versatility specialist.** Potential mechanics include attacks while human, isolated-target hunting, regeneration, supernatural senses and a transformed combat form that may retain firearms. Exact abilities remain open.

**Shapeshifter — deception and identity theft specialist. Core mechanics:**

- Can consume dead player bodies, regardless of who killed them.
- Consumption removes the corpse from the environment and grants the victim's appearance or identity.
- Can store multiple acquired identities and switch among them.
- Can return to its original appearance at any time.
- Can impersonate Enforcers if host configuration allows it.
- Does not ordinarily receive a powerful conventional monstrous combat form and relies comparatively heavily on mutations for combat enhancement.
- Retains its own functional Human Form; consuming a victim does not automatically grant their actual role abilities.

Consumption removes evidence and provides disguises, but creates an exposure risk while being performed. **TBD:** Consumption duration, exact identity limits, disguise presentation details and the default Enforcer-impersonation setting.

### 9.4 Minion role

The Minion faction initially has one primary role, Minion. Its Human Form supplies Bystander gameplay capabilities. Additional Minion roles may be introduced later.

## 10. Equipment and mutation economy

### 10.1 Shop access

Every player has an appropriate shop. Defining role abilities are available without purchases; equipment and upgrades provide additional advantages. For the proposed roster, this includes basic Detective investigation, Medic medical functionality, Werewolf transformation and Shapeshifter identity theft. Exact baseline ability specifications are TBD.

| Player type | Equipment shop | Mutation shop |
|---|---|---|
| Bystander | Associated with ordinary role | None specified |
| Enforcer | Specialised role equipment | None specified |
| Minion | Human Form equipment shop | No, by default |
| Monster | Human Form equipment shop | Monster-role mutation shop with a separate pool |

Monsters have two independent point pools. Spending Human Form equipment points does not spend mutation points, and vice versa. Minions receive only their Human Form shop by default.

**Provisional starting allocation:** Three points per shop, subject to balancing and Karma.

### 10.2 Candidate purchases

**Proposed:** Bystander shops may include bandages, flashlights, ammunition, protection, medical supplies and role equipment; some may also appear in maps. Enforcer shops may provide evidence tools, investigation utilities, defensive devices and specialised upgrades. Mutation examples include enhanced senses, regeneration, movement, transformations, special attacks and role-specific utilities.

**TBD:** Item catalogue, prices, starting pools, resource regeneration, buying during preparation and how Karma affects Monsters' separate pools.

## 11. Investigation and hidden information

The interface has no public killfeed or live scoreboard that automatically reveals who remains alive. A basic roster may display participants without survival information.

Missing people, undiscovered deaths, removed bodies, impersonation, false claims and uncertainty about events are part of play. Interface disclosures should support those experiences.

**Proposed investigative interactions:** Body inspection, causes of death, traces of combat, supernatural activity, missing bodies and suspicious behaviour. Evidence should generally support deduction rather than supply guaranteed answers.

**Proposed supernatural signatures:** Ordinary humans might lack a signature, Sinners might appear unusual, Monsters might be detectable, and mutations might conceal or alter signatures. The system and its reliability are not final.

**TBD:** Evidence generated, persistence, investigative accuracy, verification, communication channels, spectator/dead-player communication and information accessible after death. Voice-chat features and implementation are also TBD.

## 12. Karma

### 12.1 Purpose and consequences

Karma discourages habitual indiscriminate violence while tolerating mistakes and unusual tactical decisions in casual play. Repeated unjustified harm gradually reduces starting shop points. One or two minor incidents should normally have no gameplay effect.

Karma recovers through eligible responsible play, does not depend on winning, and should avoid rewarding extremely short rounds or artificial participation. Karma does not ordinarily alter role assignment or cause automatic bans.

**Provisional baseline and thresholds:**

| Karma | Illustrative starting shop points |
|---|---:|
| 750–1,000 | 3 |
| 500–749 | 2 |
| 250–499 | 1 |
| 0–249 | 0 |

The proposed baseline is 1,000. Penalty sizes, thresholds, recovery rates and the affected Monster point pools are TBD.

### 12.2 Faction interactions

The intended default penalty rules are:

| Attacker | Victim | Penalty direction |
|---|---|---|
| Bystander or Enforcer | Bystander or Enforcer | Yes, subject to context and forgiveness |
| Bystander or Enforcer | Monster or Minion | No |
| Monster | Bystander or Enforcer | No |
| Monster | Monster | Harm disabled |
| Monster | Minion | No |
| Minion | Monster | Yes, if the Monster's allegiance is known to the attacker |
| Minion | Minion | Potentially, if allegiance is known; exact rule open |

Karma considers actual interactions and the information legitimately available to the attacker. A Minion who is not told Monster identities should not be penalised merely for unknowingly attacking one.

**Eligibility:** Bystanders and Enforcers normally face penalties and can recover. Monsters normally qualify for neither penalties nor recovery. Minion eligibility depends on knowledge and opportunities for punishable actions. Players cannot recover Karma during rounds where they cannot meaningfully lose it.

### 12.3 Incident evaluation and mitigation

Karma is evaluated from recorded attacker-victim incidents. Relevant context may include damage, death, revival, legitimately known allegiance, transformation state, self-defence and forgiveness. Penalties remain provisional through round evaluation to avoid leaking hidden allegiance through immediate Karma feedback.

**Proposed mitigation:** If a Minion kills a known allied Monster to preserve cover and later revives it, the revival could reduce that specific penalty. Attacking an openly transformed allied Monster might receive a smaller penalty than attacking it while disguised. Mitigation formulas and revival mechanics are TBD. Revival-related mitigation can only reduce an existing incident penalty; it cannot award general bonus Karma.

### 12.4 Victim forgiveness

At round end, a victim may forgive attackers who harmed or killed them. Forgiveness neutralises the penalty for that attacker-victim interaction. Players cannot forgive harm to other victims and cannot award Karma beyond the score that would have existed without the incident.

**TBD:** Forgiveness window, user interface, self-defence evaluation, mitigation amounts and finalisation timing.

### 12.5 Persistence and shops

**Proposed persistence stages:** Session-specific Karma, followed by host/community persistence and dedicated-server persistence. Scope and scheduling are TBD. Global account-wide Karma is outside current requirements.

**TBD:** Whether low Karma reduces both Monster pools, mutation points alone or applies distinct rules. A low score carried into a Monster round may affect starting resources even though Monster play does not normally change that score.

## 13. Population and balance direction

The intended three-player default is one Monster and two Bystanders, with no Enforcer.

| Players | Intended default composition |
|---|---|
| 3 | 1 Monster, 2 Bystanders; no Enforcer by default |
| 4 | Normally 1 Monster; remaining faction allocation unresolved |
| 5 | Normally 1 Monster; remaining faction allocation unresolved |
| Larger groups | Distribution unresolved |

Minions provide an intermediate balancing option between one and two Monsters. A group might face one Monster and one Minion where two full Monsters would be too powerful. The appropriate lobby sizes are TBD.

**Proposed future scaling:** Role-specific adjustments to health, regeneration, damage resistance, cooldowns, transformation strength or mutations. Werewolves might need more durability scaling than Shapeshifters, whose deception can become more valuable in large groups. Avoid surprising stat changes during combat where practical.

Standard presets receive the primary balancing effort. Perfect balance is not expected for every unusual custom configuration. Maximum population, exact presets and scaling formulas remain open.

## 14. Post-round experience

The post-round summary reveals hidden information and lets the group compare their interpretation with actual events. Karma evaluation and victim forgiveness occur before final scores settle.

**Proposed summary content:** Actual factions, roles, Human Forms, deaths, kills, damage, transformations, corpse consumption, revivals, investigation events and Karma changes. A full event timeline may be added later. Exact presentation and which events are included remain open.

## 15. Host configuration and community content

Host configuration should provide simple defaults, advanced controls and eventual mod-defined settings. Controls should cover lobby size, faction distribution, role availability and capacities, primary role choice, Human Form choice, preparation timers, Monster strength/scaling, equipment, mutations, Karma, transformation rules, victory conditions and enabled mods. Minion knowledge and Shapeshifter Enforcer impersonation have their own settings.

Monster teammate recognition and protection from other Monsters remain mandatory base-game rules.

**Proposed:** Shareable configuration presets. Their format and distribution are open.

**Long-term modding ambition:** Community maps, weapons, equipment, roles across all four factions, mutations, transformations, abilities, factions, rules, victory conditions and configuration presets. Modders should be able to add content without modifying the proprietary game source and register relevant settings such as cooldowns, health, shop contents or compatibility.

Steam Workshop, mod packaging, custom scripting, dependencies and compatibility are addressed in the TDD. Initial modding scope and release timing are TBD.

## 16. Branding and commercial direction

**Current preference:** WhoDunBit, a playful variation of “whodunnit” whose “bit” suggests a supernatural attack. The desired tone is playful, catchy, slightly dark and suited to chaotic multiplayer.

**Proposed tagline:** “Everyone's innocent. Until they bite.”

The intended product is a proprietary, commercially viable standalone Steam game. The name requires clearance before release. Price, release date, monetisation beyond the standalone commercial model, detailed audience targets and marketing plan are not specified.

## 17. Open game design decisions

This register collects unresolved decisions; entries do not imply an agreed solution.

| Area | Open decisions |
|---|---|
| Core match | Maximum players; full faction presets; round duration; exact victory and timeout rules; Minion-only survival outcomes |
| Weapons and movement | Starting loadouts; map distribution; catalogue; inventory/ammunition rules; movement and damage tuning |
| Roles | Initial roster; precise abilities; Bystander Investigator versus Enforcer Detective; Enforcer verification |
| Allocation | Default capacities; duplicate policy; insufficient-capacity fallbacks; invalid or simultaneous choices; late joins/disconnects; visibility of capacity information |
| Preparation | Ammunition replenishment; retained items; ability and shop access; environmental damage/interactions; phase feedback |
| Transformations | Reversibility by role; timing/cooldowns; equipment compatibility; permanent finishing; Shapeshifter identity limits and consumption duration |
| Economy | Starting points; prices; regeneration; shop timing; Karma effects on two Monster pools |
| Investigation | Evidence model; supernatural signatures; accuracy; identity verification; information available after death |
| Communication | Channels, voice implementation, spectator rules and dead-player restrictions |
| Karma | Penalty amounts; thresholds/recovery; Minion rules; self-defence; mitigation; forgiveness timing; persistence |
| Scaling | Role-specific formulas, faction-composition inputs and when scaling is applied |
| Presentation | Detailed visual style, narrative, audio direction, accessibility and input support |
| Release | Final scope, milestones, pricing and schedule |

## 18. Revision history

| Date | Revision |
|---|---|
| 14 October 2026 | 0.1 — Initial design baseline. |
| 14 October 2026 | 0.2 — Revised structure and terminology for a conventional GDD; gameplay rules and outstanding decisions retained. |
| 14 October 2026 | 0.3 — Applied the repository documentation header standard. |
