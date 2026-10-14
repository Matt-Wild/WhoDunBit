# WhoDunBit — Technical Design Document

**Purpose:** Define WhoDunBit's technical requirements, system responsibilities and proposed architecture.  
**Version:** 0.5  
**Created:** 14 October 2026  
**Updated:** 14 October 2026  
**Status:** Pre-production technical design draft  
**Related documents:** [Game Design Document](Game%20Design%20Document.md)

This document defines technical requirements, system responsibilities and the proposed architecture for WhoDunBit. Requirements describe intended behaviour; architectural approaches remain proposals pending implementation and validation. Unresolved choices are marked **TBD**.

## 1. Technical overview

### 1.1 Target environment

| Topic | Specification / status |
|---|---|
| Engine direction | Unity 6.3 LTS |
| Reference Editor version | 6000.3.14f1; recorded in `WhoDunBit Project/ProjectSettings/ProjectVersion.txt` |
| Rendering baseline | URP 17.3.0 with the template's Forward+ renderer; see [Initial Project Configuration](Project%20Configuration.md) for applied settings |
| Language | C# |
| Initial target | Windows PC, intended Steam distribution |
| Initial multiplayer direction | Host-authoritative listen server providing a P2P-style experience |
| Future goal | Community-hosted dedicated servers without a complete gameplay rewrite |
| Content strategy | Modular and reusable systems, with extensive future modding |
| Networking framework | TBD |
| Mod packaging/runtime scripting | TBD |

### 1.2 Immediate development objective

Build a modular foundation for a host-authoritative multiplayer FPS with configurable factions, sequential role allocation, functional Human Forms and supernatural transformations. The first playable prototype should validate the core game while preserving practical extension points for future modding.

Development should proceed incrementally, with architectural decisions supported by actual gameplay needs. Reuse systems where this reduces complexity or duplication. Potential extraction of reusable Unity modules is a longer-term goal.

## 2. Technical design constraints

The architecture must support the following gameplay constraints.

1. **Separate faction from victory allegiance.** Four initial factions map to two standard sides, but future content may introduce other relationships.
2. **Separate human-role capabilities from hostile identity.** A Monster or Minion Human Form grants real Bystander abilities and equipment access without changing faction or primary role.
3. **Separate appearance from active physical/ability form.** Shapeshifter identity changes must not automatically trigger the restrictions of a monstrous combat transformation.
4. **Resolve primary roles before hostile Human Forms.** Both consume configured Bystander role capacities where applicable; Bystander primary assignments take priority in the sequence.
5. **Use authoritative state changes.** Clients request actions; the host decides assignments, purchases, damage, form changes and other gameplay outcomes.
6. **Limit information to entitled recipients.** Do not distribute hidden roles or survival state to every client merely because UI hides them.
7. **Always provide Monster teammate recognition.** Disguises and transformations must not invalidate it; it must not imply automatic through-wall location tracking.
8. **Prevent Monster-on-Monster negative effects.** Cover damage and other harmful effects, without accidentally granting equivalent protection to Minions.
9. **Keep Monster shop pools separate.** Human Form equipment and mutations use independent balances.
10. **Protect interactive preparation.** Prevent player damage, physical blocking and disruptive hostile effects while permitting ordinary preparation activity.
11. **Delay Karma finalisation.** Incident evaluation and forgiveness must not become a source of hidden-allegiance information during the round.
12. **Preserve responsive control.** Routine interactions must not be blocked solely by an unrelated animation. Deliberately committed actions need explicit restrictions and generally support cancellation.
13. **Separate cosmetic appearance from role authority.** Player-selected models and appearance preferences do not establish faction, role or verification status.

## 3. Conceptual system boundaries

**Proposed architecture:** Organise gameplay around the following responsibilities. Final assembly, package and class boundaries are TBD.

| Area | Intended responsibilities |
|---|---|
| Core gameplay | Player/entity state, health and damage, inventory, equipment, interactions, abilities, status effects and gameplay events |
| Networking | Sessions, connection lifecycle, transport integration, authority, replication and remote requests |
| Round management | Lobby state, spawning, preparation stages, timers, active round, victory evaluation and summaries |
| Factions and roles | Definitions, allegiances, primary roles, Human Forms, capacities, assignment, selection and faction knowledge |
| Transformations and identity | Physical/ability forms, restrictions, equipment compatibility, presented identity and disguise changes |
| Shops | Equipment and mutation definitions, inventories, independent point pools, purchase validation and Karma modifiers |
| Investigation | Corpses, evidence, supernatural detection, identity verification and information visibility |
| Karma | Incident ledger, provisional penalties, contextual adjustments, forgiveness, recovery and persistence |
| Modding | Content registration, settings, manifests, dependencies, extension APIs and compatibility validation |

Shared capabilities should support composition across roles. Candidate applications include damage modifiers for Werewolf resistance, regeneration for Vampires, identity presentation for disguises, item/status modification for proposed medical poisoning and capability restrictions for transformed forms. Implementation depends on the abilities selected for each role.

**TBD:** Dependencies, assembly definitions, folder layout, content authoring format, composition approach, event contracts and class-level state ownership.

## 4. Domain model and terminology

### 4.1 Independent concepts

The domain model must represent these concepts independently. Concrete types and storage are TBD.

| Concept | Meaning | Important distinction |
|---|---|---|
| Actual player identity | The real participant/entity behind an action | Must remain available to authoritative evaluation when appearance changes |
| Presented identity | Whom a player currently appears to be | Can change through Shapeshifter disguises |
| Cosmetic appearance | Selected model, skin and any supported adjustments/accessories | Independent of actual role and distinct from adopting another player's identity |
| Actual faction | Bystander, Enforcer, Monster or Minion initially | Must not be inferred from Human Form or presented appearance |
| Victory allegiance | The side whose victory the player supports | Distinct from faction for Minions and future extensions |
| Primary role | Faction-specific role such as Vampire or Detective | Does not get replaced by a disguise |
| Human Form / ordinary human role | Functional Bystander capabilities and shop | Shared human-role definitions may serve Bystanders and hostile Human Forms |
| Active physical/ability form | Current form and its available capabilities | A combat transformation can disable human-role abilities without altering the underlying assignment |
| Known faction information | Information a particular recipient is entitled to know | Distinct from authoritative truth and from current location |
| Supernatural characteristics | Properties potentially relevant to investigation | Proposed system; must not be equated automatically with faction |

For example, a player can have Monster faction, Monster-side allegiance, Vampire primary role, Medic Human Form and an active Vampire combat form. Their role assignment, available abilities and presented appearance are separate state concerns.

### 4.2 Content definitions and round state

Human-role definitions should be reusable across Bystanders and hostile Human Forms. A Medic's baseline abilities and shop should use the same definitions regardless of the player's allegiance.

**Proposed data model:** Separate reusable content definitions from per-round player assignments, current resources and active effects. Represent eligibility/capacity, visibility and form restrictions independently.

**TBD:** Identifiers, serialization, Unity asset types, object lifetimes, save schemas, content format and runtime composition framework.

## 5. Multiplayer and authority

### 5.1 Hosting model

The preferred initial model is a listen server. One player runs the authoritative simulation while participating in the game. Other clients connect to that host, providing a friend-hosted, P2P-style experience with central simulation authority.

Candidate connection mechanisms include Steam networking, relay services and direct connections. Transport selection is TBD.

Future dedicated hosting should reuse core gameplay. The proposed architecture separates simulation authority from the host's local player interface. Dedicated-server deployment design is TBD.

### 5.2 Authoritative responsibilities

The host controls:

- Faction, primary role and Human Form assignments, availability and selection outcomes.
- Preparation progression, timers, round state and victory conditions.
- Damage, health, equipment and inventory.
- Shop purchases, balances, mutations and transformations.
- Corpses and any revival outcomes.
- Hidden information and permitted disclosures.
- Karma incidents, evaluation and finalisation.

Clients submit actions or requests rather than directly changing authoritative state.

**Proposed request handling:** Validate requests against player identity, round stage, eligibility, resources and relevant gameplay restrictions. Selection and spending require authoritative conflict handling so concurrent requests cannot exceed capacity or balances. The messaging and concurrency model is TBD.

### 5.3 Framework and transport selection

The networking library and Steam integration are TBD. Selection criteria include host authority, control over replicated information, mod synchronisation, FPS performance, Steam connectivity, maintainability and dedicated-server support.

Transport-specific integration should have a limited impact on reusable gameplay code. Abstraction should remain proportional to the prototype's needs.

**TBD:** Simulation tick rate, player limits, bandwidth budgets, prediction, interpolation, lag compensation, movement authority details, physics synchronisation and disconnect timeouts.

### 5.4 Failure and continuity policies

**TBD:** Host migration support, reconnection, late joining, disconnect handling, session discovery, lobby ownership, authentication and dedicated-server orchestration.

## 6. Hidden information and recipient knowledge

### 6.1 Disclosure boundary

Clients must receive only information they are entitled to know. For example, a Bystander client must not receive the full Monster roster by default. Hidden roles, allegiance and undiscovered deaths require recipient-specific disclosure.

Network payloads, roster displays and gameplay feedback must preserve the absence of a public killfeed and automatic live survival information.

**Trust boundary:** The listen-server host holds authoritative secrets and can potentially access information unavailable to ordinary clients. This limitation is accepted for the initial hosting model.

### 6.2 Knowledge relationships

| Relationship | Knowledge policy |
|---|---|
| Monster knows Monster | Mandatory, including recognition through disguises and transformations |
| Monster knows Monster location | Identity knowledge does not automatically disclose through-wall position |
| Minion knows Monsters | Intended independent host setting; default TBD |
| Monsters know Minions | Intended independent host setting; default TBD |
| Minion knows other Minions | Intended independent host setting; default TBD |
| Bystander knows other Bystanders | No inherent identification |
| Public recognition of Enforcers | Genuine allegiance known or verifiable; exact verification open |

**Proposed approach:** Maintain recipient knowledge separately from authoritative truth and use it when presenting identity and discovery information. Replication objects, visibility APIs and knowledge storage are TBD.

### 6.3 Information leakage review

Review role availability updates, selection feedback, apparent identity, death/spectator behaviour, corpse removal and Karma feedback for unintended disclosures. The selection UI must not cause an involuntary faction-specific interruption of movement.

**TBD:** Visibility of shared role capacities and permitted inferences; death/spectator channels; Enforcer verification under impersonation; voice/nameplate presentation; late-join disclosures; detailed information-security design.

## 7. Round lifecycle and allocation

### 7.1 Conceptual lifecycle

The round follows this sequence:

`Lobby → Initial preparation → Primary role allocation → Human Form allocation → Final preparation → Active round → Post-round evaluation/summary → Next round`

**TBD:** State-machine implementation and the ordering of summary presentation, contextual mitigation and forgiveness within the post-round stage.

At preparation start, players spawn into the map and learn their faction. Reveal primary roles when resolved, then hostile Human Forms after that stage. Final preparation gives everyone their complete role information before active play.

**TBD:** Exact victory conditions, timeout handling, simultaneous victory outcomes, empty-lobby/disconnect transitions and Minion survival after the last Monster dies.

### 7.2 Assignment and choice

Primary roles and hostile Human Forms are assigned randomly by default. Two independent host options enable player choice, both disabled by default. Primary choices remain within the assigned faction. Players who make no selection receive a random eligible assignment when the relevant timer expires.

1. Resolve primary roles for all factions.
2. Account for Bystander primary roles against shared Bystander capacities.
3. Resolve Monster and Minion Human Forms from remaining eligible capacity.
4. Prevent concurrent accepted choices from exceeding capacity.
5. Begin final preparation after both allocation stages resolve.

**Proposed allocation approach:** Validate each selection against authoritative availability at acceptance time. Competing requests must not overbook a role. Invalid requests and configurations with insufficient eligible capacity require explicit fallback policies.

**TBD:** Random distribution algorithm and weighting, contention policy, re-selection, capacity release after disconnects, duplicate defaults, exhausted pools, late joins and invalid-configuration fallback.

### 7.3 Preparation timing and interaction

**Provisional timing:** Initial 5 seconds; primary 5 seconds automatically or 15 with choice; Human Form 5 seconds automatically or 15 with choice; final 10 seconds. Totals are 25, 35 or 45 seconds depending on selection settings. All are configurable initial test values.

Preparation permits exploration, pickup, firing and appropriate map interactions. Player-on-player damage, physical blocking and disruptive hostile effects are disabled. Selection menus open voluntarily through a configurable hotkey.

**TBD:** Environmental damage, allowed preparation abilities, inventory carryover, ammunition restoration, resource spending, shop timing and transition feedback.

## 8. Human capabilities, transformations and effects

### 8.1 Human-role capability reuse

A Monster or Minion Human Form grants functional Bystander abilities, appropriate passives/interactions, equipment access and the corresponding shop. It does not alter faction, victory allegiance or Monster-role/mutation identity.

Common capabilities should support composition where useful. Proposed future mechanics, such as corrupted medical equipment, should fit existing ability/item systems where practical, limiting special cases for individual role combinations.

### 8.2 Form transitions

A monstrous combat form disables Human Form abilities. Its supernatural capabilities and item compatibility depend on the role/form and item. Ordinary Shapeshifter disguise changes preserve Human Form capabilities.

**Proposed form model:** Evaluate capability availability separately from role assignment and presentation. Preserve the Human Form assignment while suppressing its active capabilities in a monstrous form.

**TBD:** Behaviour of deployed equipment, ongoing passives, scheduled effects, resource regeneration and cooldowns across transitions. Weapon retention, reversibility, animation, timing, form health and transition validation also require specification.

### 8.3 Damage and hostile effects

Monster-to-Monster damage and negative ability effects are disabled, including relevant poison, incapacitation and harmful statuses. Monsters remain able to harm Minions. Preparation applies additional player-protection rules.

**Proposed effect validation:** Apply harm policies at authoritative effect application, including area effects, delayed effects and equipment-attributed damage. Ownership and attribution rules for indirect harm are TBD.

Other active-round friendly-fire rules, self-damage and environmental damage remain open unless constrained by the GDD. Karma penalties do not inherently mean an attack is mechanically disabled.

### 8.4 Responsive movement, weapons and actions

Walking is the default movement mode; sprinting is available, with no stamina consumption currently planned. Movement tuning should prioritise crisp response over realistic inertia. Jumping, climbing and vaulting remain under consideration.

Weapons should generally be controllable, with poor handling reserved for a deliberate weapon characteristic. Ordinary bullet damage must provide clear feedback without applying movement slowdown or partial incapacitation. Per-magazine tracking of spare ammunition is outside the current gameplay direction; detailed reload rules are TBD.

Opening a door must remain possible during reloading. **Proposed action model:** Validate concurrent actions against explicit gameplay restrictions rather than treating an active animation as a global interaction lock. Animation should represent action state; it should not create incidental restrictions on unrelated interactions.

Deliberate commitments may impose movement, view and interaction restrictions. Corpse consumption is a proposed example: stop movement and limit looking/interacting while consuming. Committed actions should generally support aborting. Exact restrictions, interruption events, cancellation costs and progress retention are TBD.

**Proposed authority handling:** The host validates gameplay outcomes and commitment restrictions. Local presentation and networking should preserve responsive feedback while respecting those decisions. Prediction, reconciliation, animation blending and exact input handling remain implementation choices.

### 8.5 Environmental and corpse physics

Most environmental props are intended to use physics, with impact responses that may include movement or destruction. Some weapons may launch dead players; effects that launch living players remain under consideration. Prop carrying and body movement are also undecided.

**Proposed physics approach:** Distinguish gameplay-relevant prop/corpse state from presentation where useful, and apply host authority to consequential interactions. Preparation protections must cover harmful or disruptive effects caused through props as well as direct attacks.

**TBD:** Physics and ragdoll replication, collision rules, destruction representation, ownership/attribution, impulse handling, active-object budgets and the effect of moved bodies on investigation. Living-player knockback requires a separate design decision from ordinary gun-hit feedback.

### 8.6 Character presentation and customisation

Default base models are humanoid. Model selection must be independent of role, and future mod support should make adding compatible player models straightforward. The bundled roster may include guard and civilian models, but those appearances do not confer trusted status.

**Proposed appearance model:** Keep model/skin selection, supported model adjustments and accessories separate from faction, role, known identity and active form. Optional preferences may select an Enforcer appearance or an ordinary human appearance used by Bystanders, human-looking Monsters and Minions. Preference resolution must preserve the intended information boundaries.

Role information and verification need UI that uses legitimate recipient knowledge rather than assuming a particular model identifies a faction. The exact UI remains TBD, including verification and presentation during Shapeshifter impersonation.

Monstrous forms replace the base model's presentation and allow more limited customisation. Shapeshifter disguises adopt an identity through their separate presentation rules. **TBD:** Which model, skin, adjustment and accessory properties a disguise copies, and how base appearance is restored after form or identity changes.

Model adjustments, hats/accessories and separate appearance preferences are proposed features. Free model selection and host/server cosmetic unlock shops are alternative access options under consideration. Cosmetic currency, entitlement storage, persistence and links to any other economy are undefined.

**TBD:** Model authoring/import workflow, rigs and animation compatibility, model validation, attachment points, collision/hitbox policy, network representation, content distribution and allowed Monster customisation. Arbitrary player appearance does not imply that model dimensions define gameplay collision.

### 8.7 Rendering, camera and feedback

The visual target is grounded, moderately detailed realism with a performance-conscious presentation. Default scenes should be readable; maps and Monster abilities may provide specific atmosphere or visibility effects. The initial project uses URP 17.3.0 with Forward+ rendering. The applied desktop settings and their trade-offs are recorded in [Initial Project Configuration](Project%20Configuration.md). Final quality tiers, asset budgets and target hardware remain TBD.

Camera sway and intrusive damage effects are likely to be omitted. Head bob and recoil are likely to be included, with head bob optional. Screen shake is likely to be reserved for meaningful gameplay or environmental events. These are presentation targets for playtesting; final effects, intensities and settings remain TBD.

## 9. Inventory, shops and mutation state

### 9.1 Shop structure

| Player | Shop state required by the design |
|---|---|
| Bystander | Ordinary role equipment shop |
| Enforcer | Specialised role equipment shop |
| Minion | Human Form equipment shop only by default |
| Monster | Human Form equipment shop and Monster-role mutation shop with independent point balances |

A Monster purchase in one shop cannot consume the other shop's points. **Provisional starting allocation:** Three points per shop before Karma modifiers. Final prices and starting balances are TBD.

Essential role functionality is available without purchase. Equipment and mutations enhance it. Item and mutation catalogues are TBD.

### 9.2 Purchase and inventory authority

Purchases, inventory, equipment, mutations and balances are host-authoritative.

**Proposed purchase handling:** Validate shop eligibility, availability, cost, balance and purchase restrictions before committing. Preserve separate balances when applying spending or future Karma modifiers. Repeated or concurrent requests must not produce unintended duplicate grants or overspending.

**TBD:** Inventory implementation, item slots, drop/pickup behaviour, purchase limits, refunds, shared items, mutation stacking, point regeneration, preparation purchases and compatibility with active forms.

### 9.3 Karma interaction

**TBD:** Whether low Karma affects both Monster pools, mutations alone or uses separate rules for each. Minions use their Human Form shop; their Karma eligibility depends on knowledge settings and penalty rules.

## 10. Corpses, disguises, investigation and revival

### 10.1 Corpse consumption and identity

Corpse consumption is host-authoritative. A completed consumption removes the dead player's body and grants the Shapeshifter that player's appearance/identity. The Shapeshifter can store multiple identities, switch among them and return to its original appearance at any time. Consumption does not require ownership of the kill. Enforcer impersonation is host-configurable.

The Shapeshifter keeps its own Human Form rather than acquiring the consumed victim's role capabilities. Ordinary disguise switching is distinct from monstrous combat transformation.

Consumption is intended to expose the Shapeshifter through a deliberate action commitment. The proposed movement/view/interaction restrictions and abort behaviour are described in section 8.4; exact consumption rules remain TBD.

**TBD:** Consumption duration, identity limit, interruption/cancellation rules, concurrent corpse interactions, identity UI and which aspects of clothing, names, voice or equipment a disguise copies.

### 10.2 Evidence

Corpses and evidence should support deduction through clues. Candidate systems include body inspection, cause of death, supernatural traces, combat evidence and missing-body investigation.

**Proposed:** Separate supernatural characteristics from actual faction to accommodate misleading Sinner evidence and concealment mutations. The signature model itself is not final.

**TBD:** Evidence types, schema, authoring, retention, visibility, corruption, certainty, replication policy and investigation interfaces.

### 10.3 Revivals

Revival is a candidate mechanic with implications for Karma mitigation and post-round reporting. If included, revival outcomes must be host-authoritative.

**TBD:** Release scope, eligible roles/items, legal targets, costs, timing, failure conditions, allegiance after revival and interactions with consumed corpses.

## 11. Karma processing and persistence

### 11.1 Incident-based evaluation

Track individual attacker-victim incidents and hold penalties provisional until round evaluation. Live feedback must not disclose hidden role or allegiance information.

Relevant context may include damage, deaths, revivals, known allegiance, form state, self-defence, automatic mitigation and victim forgiveness.

**Proposed incident model:** Retain action-time facts and legitimate attacker knowledge so later revelations do not retroactively change what the attacker knew. Associate mitigation with its original incident. The event schema is TBD.

### 11.2 Eligibility and policy

The GDD defines the faction interaction matrix. Human-on-human harm normally attracts penalties; human attacks on the Monster side do not. Monsters normally neither lose nor recover Karma. Minion penalties and recovery eligibility depend on legitimately known allies and the final rules.

Karma does not ordinarily alter role assignment or impose automatic bans. One or two minor mistakes should generally not change starting equipment points. Exact amounts remain open.

### 11.3 Mitigation, forgiveness and recovery

Victims can neutralise penalties tied to their own attacker-victim interactions. Forgiveness cannot grant more Karma than would exist without the incident. Proposed revival-related mitigation reduces a specific incident penalty and cannot award general bonus Karma.

**Proposed contextual rules:** Partial relief when an attacker later revives the ally they killed; reduced penalties for a Minion attacking an openly transformed Monster to preserve cover. Exact algorithms are TBD.

Finalise totals after mitigation and forgiveness. Recovery towards baseline applies only in eligible rounds where the player could meaningfully lose Karma; winning is not required. Recovery criteria should exclude trivial or artificial participation.

**TBD:** Processing order, aggregation, duplicate-incident handling, damage/kill overlap, self-defence assessment, forgiveness timeout, eligibility thresholds, recovery rates and UI timing. Baseline 1,000 and the GDD shop thresholds are provisional.

### 11.4 Persistence

**Proposed persistence stages:** Session-specific storage, host/community persistence and dedicated-server persistence. Global account-wide Karma is outside current requirements.

**TBD:** Identity keys, storage mechanism, ownership, migrations, tamper resistance, resets and synchronisation.

## 12. Host configuration and scaling

Configuration should provide straightforward presets with advanced settings. Intended controls cover population, faction counts, roles/capacities, choice stages, preparation timing, equipment/mutations, Monster strength/scaling, Karma, transformations, win rules and enabled mods. Minion knowledge relationships and Shapeshifter Enforcer impersonation have separate settings.

**Proposed scaling:** Role-specific adjustments to health, regeneration, damage resistance, cooldowns, form power or mutation effectiveness. Scaling should avoid surprising attribute changes during combat where practical.

**Proposed configuration handling:** Validate role capacity and eligibility before allocation. Support registration of mod-defined settings through common configuration interfaces. The policy for rejecting, correcting or applying fallbacks to invalid settings is TBD.

**TBD:** Configuration schema, save format, presets, sharing, versioning, permitted in-round changes, default capacities, population presets and scaling formulas. Monster-to-Monster recognition and protection remain mandatory base-game rules.

## 13. Modding and extensibility

### 13.1 Long-term content scope

Modding is a foundational priority. Community content should extend the game without modifying its source. Intended extension areas include maps, weapons, equipment, player models, roles across all factions, mutations, transformations, abilities, factions, rules, victory conditions and presets. Initial release scope is TBD. Custom player models require a defined compatibility and import workflow as described in section 8.6.

Common content should use reusable functionality and data where practical, with custom code for mechanics existing systems cannot express. Early architecture should preserve useful extension points; the full distribution and runtime ecosystem can follow core gameplay validation.

### 13.2 Mod configuration

Mods should be able to register settings such as cooldowns, availability, health, transformation restrictions, equipment compatibility, shop inventories and mutation costs.

**TBD:** Registration API, content IDs, discovery, load order, authoring tools, runtime extensibility and compatibility guarantees.

### 13.3 Distribution and security

**Candidate distribution features:** Steam Workshop, manifests, dependencies, required mod versions, asset/content loading, host/client compatibility checks and version validation.

**TBD:** Packaging format, runtime scripting, managed-code support, dependency resolver, mod synchronisation and trust model. A data-driven-first rollout is one option. Support for arbitrary managed code requires an explicit trust and execution policy.

### 13.4 Proprietary game and potential SDK

The main game remains proprietary. A proposed public modding SDK would expose supported extension points without requiring access to the full game source. A permissive licence such as MIT is under consideration for the SDK.

**TBD:** SDK scope, licence and release timing. The main game has no selected public licence.

## 14. Repository and development practices

The intended repository setup is private Git hosting with proprietary source, a Unity `.gitignore` and a tracked Unity project version. Collaborator permissions and contribution ownership are TBD before additional developers join.

Unity 6.3 LTS / 6000.3.14f1 is the reference Editor version. The project is in `WhoDunBit Project/`, with the Editor version recorded in its `ProjectSettings/ProjectVersion.txt`. A Unity `.gitignore` at the project root excludes generated files while retaining source assets, metadata, package manifests/lockfile and project settings. Engine upgrades should be justified against compatibility and project needs.

Architecture and package choices should be evaluated against the current codebase. Network library, scene structure, build pipeline and test tooling are TBD.

The GDD defines gameplay behaviour. Changes affecting authority, state, visibility or system interactions should be reflected in the TDD, with decision dates and rationale recorded.

Design documentation is maintained in the repository's `Documentation/` directory alongside the codebase. See the [documentation index](README.md) for the GDD, TDD and design-status conventions.

## 15. Development phases

The following phased roadmap is proposed. Milestone scope and dates are TBD. Architecture and modding considerations apply throughout, with fuller mod infrastructure following validation of the core game.

| Stage | Candidate work |
|---|---|
| 1. Technical foundations | Inspect/bootstrap Unity structure; choose networking; establish useful module boundaries; basic sessions, player representation, movement and shooting |
| 2. Round framework | Lobbies, spawning, preparation stages, faction/role assignment, lifecycle and agreed victory rules |
| 3. Human roles | Reusable role definitions, functional abilities, equipment shops, capacities, Human Form allocation and selection UI |
| 4. Monsters | Monster definitions, supernatural abilities, transformations/restrictions, mutations, teammate knowledge and harm protection |
| 5. Deception and investigation | Corpses, evidence, tools, Shapeshifter disguises, presented identity and visibility |
| 6. Balance and Karma | Population presets, scaling investigation, economy, incidents, forgiveness and selected persistence scope |
| 7. Modding infrastructure | Public definitions/registration, examples, compatibility, SDK documentation and distribution tooling |
| 8. Release preparation | Steam integration, interface polish, performance, multiplayer reliability, security review, playtesting and commercial preparation |

Core gameplay validation takes priority over speculative features. Prototype-specific rules should be recorded with their intended scope and reviewed before becoming production defaults.

## 16. Validation strategy

The proposed validation strategy combines focused system tests with multiplayer playtesting. Test tooling and numerical acceptance targets are TBD.

| Area | Behaviour worth validating once implemented |
|---|---|
| Allocation | Primary roles finish before Human Forms; shared capacities cannot be exceeded by competing requests; eligible unselected players receive assignments; impossible configurations follow an agreed policy |
| Human Forms | Genuine and hostile versions provide the intended same baseline human functionality and shop access without changing allegiance |
| Preparation | Exploration and firing work; player harm, disruptive effects and physical blocking are prevented; menus open voluntarily |
| Identity/forms | Disguise switching preserves the assigned Human Form; a combat transformation suppresses its abilities; a stolen identity does not grant the victim's role |
| Monster knowledge | Monsters recognise teammates through transformations/disguises without receiving unintended position information |
| Harm rules | Monster protection covers each supported negative-effect path; Minions remain vulnerable to Monsters |
| Shops | Spending one Monster balance leaves the other unchanged; invalid/concurrent requests cannot overspend or double grant |
| Information | Unentitled clients do not receive hidden rosters or automatic survival information; role UI and Karma feedback respect disclosure rules |
| Corpses | Valid consumption removes the body and grants the correct disguise; ownership of the kill does not restrict consumption |
| Karma | Action-time knowledge informs evaluation; forgiveness applies only to the victim's incidents; mitigation cannot generate bonus Karma; ineligible rounds do not recover Karma |
| Round outcomes | Agreed victory, time-expiry and disconnect policies resolve consistently and reveal information only at permitted times |
| Modding, when scoped | Selected compatibility rules catch unsupported content/settings rather than silently assuming identical host/client content |
| Responsiveness | Walking/sprinting feel crisp; door use remains available during reloads; routine interactions are not blocked solely by animations |
| Action commitments | Supported commitments enforce their defined restrictions and abort rules consistently without unintended retained locks |
| Physical interactions | Prop movement/destruction and corpse impulses behave consistently; preparation protections survive indirect physical effects |
| Character presentation | Model choices do not establish roles; appearance preferences, form replacements and disguises preserve the intended identity and knowledge rules |
| Camera and readability | Ordinary hits remain clear without movement penalties or intrusive obscuring effects; head bob can be disabled if included |

Playtesting should assess whether beginners understand the objective, Monsters feel powerful but answerable, role demonstrations preserve believable deception, preparation limits downtime and investigation creates useful uncertainty.

## 17. Open technical decisions and dependencies

| Area | Unresolved decisions / required input |
|---|---|
| Unity setup | Further project/module layout and package choices; initial project and configuration are established |
| Networking | Library, Steam transport/relay, replication model, movement synchronisation, tick rate, prediction and lag handling |
| Sessions | Population limits, authentication/discovery, reconnects, late joins, host migration and disconnect policies |
| Dedicated hosting | Server architecture, builds, discovery, administration and deployment |
| Data and architecture | Definition format, IDs, runtime state model, assemblies/modules, serialization and events |
| Allocation | Random algorithm, contention policy, invalid capacity handling, re-selection and late/disconnected players |
| Information security | Recipient visibility details, Enforcer verification, survival/spectator information and capacity disclosures |
| Movement and actions | Movement/sprint tuning; jumping/climbing/vaulting; concurrent-action model; commitment restrictions; abort controls, costs and progress |
| Combat and forms | Weapon/recoil tuning, reload rules, item compatibility, effect attribution, ongoing effects during transformation, finishing/resistance rules and transition behaviour |
| Physics | Prop/corpse replication, collision, destruction, impulses, budgets, carrying/body movement and possible living-player knockback |
| Economy | Item/mutation schemas, prices, balances, concurrency behaviour, regeneration and Karma-to-pool mapping |
| Corpses and revival | Consumption conflicts/cancellation, disguise channels, identity limits and revival mechanics/availability |
| Karma | Formulas, context/knowledge records, aggregation, self-defence, finalisation, forgiveness window and storage |
| Configuration | Schema, validation/fallbacks, shareable presets, scaling and mod-defined settings |
| Modding | API boundaries, packaging, assets, scripting, Workshop, dependencies, synchronisation, security and version policy |
| Nonfunctional targets | Frame rate, latency, hardware, bandwidth, performance budgets and profiling criteria |
| Presentation/input | Final quality tiers, UI framework, asset budgets, camera tuning/settings, audio/voice, input bindings and wider accessibility implementation |
| Character appearance | Custom-model workflow; rigs/animations; adjustments/accessories; appearance preferences; hitbox policy; verification UI; disguise copying and Monster customisation |
| Cosmetic access | Free selection versus host/server unlock shops; currency, entitlements, persistence and release scope |
| Build/release | CI, test tooling, Steam SDK integration, release process and distribution details |

Gameplay dependencies, including victory rules, role abilities and preparation inventory policy, require specification in the GDD before their implementation is finalised.

## 18. Revision history

| Date | Revision |
|---|---|
| 14 October 2026 | 0.1 — Initial technical design baseline. |
| 14 October 2026 | 0.2 — Revised structure and terminology for a conventional TDD; requirements, architecture proposals and outstanding decisions retained. |
| 14 October 2026 | 0.3 — Applied the repository documentation header standard and updated documentation location. |
| 14 October 2026 | 0.4 — Added responsive action, physics and presentation requirements; documented proposed commitment handling, character customisation and their open implementation decisions. |
| 14 October 2026 | 0.5 — Recorded the created Unity project, URP/Forward+ baseline and Git exclusions; linked the complete initial configuration record. |
