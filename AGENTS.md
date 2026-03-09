\# Bird Cafe – Agent Contribution Guidelines



This document governs how AI agents and human developers should write, modify, and structure code for the \*\*Bird Cafe\*\* repository.



The repository is intentionally split between:



\- \*\*`BirdCafe.Shared`\*\*: the shared simulation/gameplay engine consumed by multiple front ends

\- \*\*`BirdCafe.Shared.Console`\*\* (`ConsoleApp~`): a thin console-based verification/playable UI



The most important rule in this repository is simple:



> \*\*Shared game rules live in `BirdCafe.Shared`. Presentation lives in the UI layer.\*\*



Do not duplicate logic across layers. Do not move business rules into the console app. Do not add Unity-specific dependencies to the shared library.



---



\## 1. Project architecture and boundaries



\## 1.1 `BirdCafe.Shared` is the canonical engine



\*\*Target:\*\* `.NET Standard 2.1`  

\*\*Unity constraint:\*\* must remain Unity-compatible and free of Unity engine references.



This project is the source of truth for:



\- game state

\- save-state shape

\- simulation flow

\- economy and ledger behavior

\- care and planning rules

\- reporting and game-over logic

\- UI-facing `ViewModels`



This project must remain \*\*plain C#\*\*, consumable by both:



\- the console app

\- the Unity client



\### Non-negotiable rules



\- \*\*No `UnityEngine` references\*\*

\- \*\*No MonoBehaviour / ScriptableObject dependencies\*\*

\- \*\*No scene-dependent logic\*\*

\- \*\*No UI rendering concerns\*\*

\- \*\*No console-specific formatting or terminal behavior\*\*



Also note:



\- the shared `.csproj` explicitly compiles only `Runtime/\*\*/\*.cs`

\- the shared asmdef is configured with `noEngineReferences: true`



That means \*\*new shared code must be placed under `Runtime/`\*\* or it will not be part of the shared library.



---



\## 1.2 `BirdCafe.Shared.Console` is a thin verification UI



\*\*Target:\*\* `.NET 6.0`



The console project exists to:



\- verify shared gameplay logic quickly

\- provide a playable CLI/reference implementation

\- exercise ViewModels and engine flows outside Unity



The console app must:



\- read data from shared `ViewModels`

\- trigger actions through the shared facade

\- render text and collect input

\- remain thin



The console app must \*\*not\*\*:



\- mutate bird stats directly

\- calculate prices, costs, or simulation outcomes

\- authoritatively change inventory or save-state

\- become the canonical implementation of game rules



---



\## 2. Core engine principles



\## 2.1 `BirdCafeGame` is the UI boundary



All UI-layer interaction must go through \*\*`BirdCafeGame`\*\*.



For console code, this means `BirdCafeGame.Instance`.  

For other UIs, the same facade principle applies.



`BirdCafeGame` is responsible for:



\- delegating work to the appropriate manager

\- exposing UI-safe methods for engine actions

\- returning screen-appropriate `ViewModels`

\- handling engine failures and surfacing them as UI-facing events/messages

\- coordinating screen transitions and popup-style events



\### Required contribution rule



If a new user-visible feature is added, update `BirdCafeGame` so the UI can access it through the facade.



Examples:



\- new care action

\- new planning action

\- new report screen

\- new popup or chat flow

\- new screen/view model

\- new summary data needed by the UI



\### Anti-patterns



Do \*\*not\*\* make UI code rely on:



\- `\_controller` internals

\- manager internals

\- public controller escape hatches

\- raw domain models when a screen-facing ViewModel should exist



Even though `BirdCafeGame` currently exposes `Controller`, treat that as an implementation leak, not the preferred UI integration path.



---



\## 2.2 Preserve the controller / manager architecture



`BirdCafeController` is the authoritative engine root.



It owns:



\- `CurrentState`

\- `CurrentPhase`

\- the manager set:

&nbsp; - `Meta`

&nbsp; - `Simulation`

&nbsp; - `Care`

&nbsp; - `Planning`

&nbsp; - `Reporting`



Use the existing split consistently:



\### `MetaManager`

Owns:

\- new game creation

\- loading/injecting save state

\- session-level setup

\- meta operations



\### `SimulationManager`

Owns:

\- workday simulation

\- customer generation

\- customer interactions

\- end-of-day cleanup

\- day result creation

\- deterministic daily simulation behavior



\### `CareManager`

Owns:

\- evening bird care actions

\- health/mood/energy/hunger/stress effects

\- care validation

\- care-related ledger or cost behavior



\### `PlanningManager`

Owns:

\- evening inventory planning

\- next-day staffing/rest selections

\- daily plan mutations

\- planning-phase validation



\### `ReportingManager`

Owns:

\- daily/weekly summaries

\- game-over checks

\- outcome aggregation

\- long-term report generation



\### Placement rule



When adding code, place it in the narrowest correct layer:



\- persistent data -> models

\- workflow/domain actions -> manager

\- engine orchestration / UI API -> `BirdCafeGame`

\- rendering shape -> ViewModel

\- terminal display -> console screen



---



\## 2.3 Respect the phase-based state machine



The game loop uses strict phase-gated progression.



Core phase flow:



`Meta -> DayLoop -> EveningLoop -> Reporting`



Manager actions must validate phase before mutating state.



Examples:



\- do not allow inventory planning during `DayLoop`

\- do not run simulation outside `DayLoop`

\- do not apply evening care at the wrong time

\- do not advance reporting from the wrong phase



If an action is invalid for the current phase, return a failure result rather than silently doing nothing.



Preferred pattern:



\- validate phase first

\- return `EngineResult.Failure("InvalidPhase", "...")`



Never “fix” a wrong-phase call by quietly forcing the phase unless that transition is explicitly part of the design.



---



\## 2.4 `EngineResult` is the standard command contract



Command-style manager operations should return `EngineResult`.



Use:



\- `EngineResult.Success(payload)` for success

\- `EngineResult.Failure(code, userMessage)` for failure



\### Guidelines



\- error codes should be stable and machine-readable

\- messages should be UI-friendly

\- failure should be explicit

\- avoid throwing exceptions for expected gameplay validation failures



Typical failure cases:



\- wrong phase

\- insufficient money

\- invalid bird id

\- invalid quantity

\- missing plan data

\- unavailable action

\- invalid save input



\### UI rule



UI layers should not own engine-validation policy.



Instead:



\- managers return `EngineResult`

\- `BirdCafeGame` interprets it

\- facade emits toast/help/chat/screen events or returns ViewModels as needed

\- UI renders the result



Do not spread validation logic across screens when the engine already knows the rule.



---



\## 2.5 `GameSave` is the single source of truth



All durable gameplay state belongs in `GameSave` or one of its owned child models.



If a value must persist:



\- across screens

\- across phases

\- across days

\- across weeks

\- across loads/saves



…it belongs in save-state, not in transient UI state.



\### Store durable data in models, not in:

\- static fields

\- console globals

\- manager-local caches

\- ad hoc UI memory

\- duplicated derived fields in multiple places



Use `GameSave` and owned structures for things like:



\- economy balance and ledger

\- current day / week state

\- planning data

\- bird states and flags

\- history and summaries

\- configuration relevant to the session



---



\## 2.6 Preserve deterministic simulation



The daily simulation must remain deterministic for a given day seed.



`SimulationManager.RunDaySimulation` initializes randomness from the current plan’s day seed. Preserve that behavior.



\### Required rules



\- use the existing seeded RNG flow

\- do not introduce `new Random()` inside the simulation loop

\- do not introduce `Guid.NewGuid()` for simulation randomness

\- do not use clock/time-based randomness for simulation outcomes

\- keep day results reproducible for the same seeded plan



This is important for:



\- testability

\- debugging

\- replay stability

\- verification between Unity and console consumers



\### Important nuance



`MetaManager.StartNewGame` currently creates the first day’s seed using `new Random()` when creating the plan. That is acceptable for initial session generation. The simulation itself must remain deterministic once a day seed exists.



---



\## 2.7 Preserve stat clamping and invariants



The design materials describe bird state using “1–100” style semantics, but the current domain code enforces \*\*0–100 clamping behavior\*\*.



Treat the code invariant as authoritative unless you are intentionally performing a repo-wide rules migration.



\### Current rule

Bird state mutations should remain clamped to valid bounds.



Examples already enforced in domain code:



\- `Hunger` -> clamp to `0..100`

\- `Mood` -> clamp to `0..100`

\- `Health` -> clamp to `0..100`

\- `Energy` -> clamp to `0..100`

\- `Stress` -> clamp to `0..100` with many operations only enforcing the lower bound directly



\### Required contribution rule



Whenever you add or change domain behavior affecting bird stats:



\- clamp values explicitly

\- do not trust UI input

\- do not allow silent overflow/underflow

\- prefer central domain helpers where possible

\- keep mutation logic in engine/model code, not UI code



If you introduce a new stat-like quantity, define and enforce its valid range clearly.



---



\## 3. Data, models, and ViewModels



\## 3.1 Models are for domain and persistence



Use `Runtime/Models` and related runtime domain files for persistent or authoritative state.



Examples include:



\- bird state

\- cafe state

\- economy and ledger state

\- inventory

\- day state

\- simulation results

\- summaries

\- configuration



Model code should be:



\- explicit

\- serializable-friendly

\- easy to inspect

\- stable across front ends



Do not turn models into mini-UI layers.



---



\## 3.2 ViewModels are for screen consumption



UIs should not receive live domain state directly when the intent is to render a screen or popup.



Instead:



1\. compute or collect the relevant domain information

2\. flatten it into a `ViewModel`

3\. expose that ViewModel via `BirdCafeGame`



\### ViewModel rules



\- make them UI-ready

\- prefer booleans/strings/flat properties over requiring UI-side logic reconstruction

\- use them for screens, reports, popups, chat, and summaries

\- keep them presentation-oriented

\- do not put business logic into them



Examples of good UI-ready properties:



\- `IsAffordable`

\- `WillRestTomorrow`

\- `StatusText`

\- `FormattedTime`

\- display names/messages ready for rendering



\### Important rule



If the UI needs data to render a feature, update the shared ViewModel contract instead of forcing the UI to re-derive the logic itself.



---



\## 3.3 Keep domain text and presentation compatibility in mind



Some shared content is presentation-facing, such as chat/tutorial/report text.



The repository already contains examples of rich-format-style content in shared code, while the console strips tags for terminal output.



When adding presentation-oriented text to shared data:



\- keep it understandable even when formatting is stripped

\- do not rely on Unity-only rendering behavior

\- avoid embedding UI logic in text content

\- prefer data structures over giant hard-coded UI branches



---



\## 4. Shared engine coding rules



\## 4.1 Put gameplay logic in the right place



Business logic belongs in `BirdCafe.Shared`.



This includes:



\- simulation math

\- popularity changes

\- sickness logic

\- starvation or decay logic

\- bird care effects

\- inventory purchasing rules

\- waste rules

\- end-of-day cleanup

\- report generation

\- phase transitions

\- game-over checks



Do not implement or duplicate those rules in:



\- `ConsoleApp~/Program.cs`

\- `ConsoleApp~/Screens/\*`

\- Unity front-end scripts

\- utility classes created for display-only purposes



---



\## 4.2 Keep mutations centralized



When a feature changes state, centralize the mutation in one authoritative place.



Good examples:



\- manager mutates `GameSave`

\- model helper clamps the changed stat

\- facade calls the manager and translates result for UI



Bad examples:



\- UI mutates state, then manager mutates it again

\- console pre-applies costs before the engine does

\- multiple screens each partially implement the same domain rule



---



\## 4.3 Keep finance and ledger behavior coherent



If a change affects money, inventory cost, or financial outcomes, keep the economy model coherent.



When introducing or modifying spend/earn flows:



\- update balance consistently

\- record ledger entries when that is part of the existing flow

\- keep purchase/sale reason codes meaningful

\- align inventory changes with financial changes

\- avoid “phantom” money changes without corresponding state changes



Typical examples:



\- feed/vet/play costs

\- inventory purchases

\- product sales

\- waste-related accounting

\- report totals



---



\## 4.4 Preserve seeded planning-to-simulation flow



The game relies on the current day plan to drive the simulation.



When changing planning or simulation features:



\- ensure the plan remains the input to the day loop

\- ensure next-day staffing and purchasing choices feed into simulation

\- ensure simulation results are captured and surfaced to reports/UI

\- avoid adding hidden side-channel state not represented in the plan/save models



---



\## 4.5 Avoid unnecessary architecture churn



This repository already has a clear educational architecture.



Do not introduce new layers, frameworks, or abstractions unless there is a compelling reason.



Prefer:



\- small helpers

\- explicit methods

\- straightforward manager logic

\- clear ViewModel mappers

\- well-named enums and models



Avoid:



\- speculative generic frameworks

\- overly abstract service layers

\- UI-driven domain behavior

\- complicated event buses for simple flows

\- magic strings when enums/constants are appropriate



---



\## 5. Console app contribution rules



\## 5.1 The console app must use the facade only



Console code should call:



\- `BirdCafeGame.Instance.Get...ViewModel()`

\- `BirdCafeGame.Instance.\[Action]()`

\- facade popup/chat/help methods

\- facade-driven navigation/screen transitions



Console code should \*\*not\*\*:



\- manipulate `BirdCafeGame.Instance.Controller` directly

\- call managers directly

\- mutate `GameSave` directly

\- reproduce engine-side validations



Even if a public escape hatch exists, do not build new console features around it.



---



\## 5.2 Console screens are presentation and input only



Files under `ConsoleApp~/Screens/` should:



\- clear/redraw screens

\- print ViewModel data

\- collect input

\- call shared facade methods

\- respond to screen transitions and popup hooks



They should not:



\- perform simulation math

\- calculate derived domain decisions that belong in shared code

\- become the only place a feature works



---



\## 5.3 Use existing event hooks instead of ad hoc behavior



The console app already wires the facade through global hooks in `Program.cs`.



Examples include:



\- `OnScreenChanged`

\- `OnToastMessage`

\- `OnHelpPopup`

\- `OnChatPopup`

\- `OnMoneyChanged`



When adding new user-visible error/info flows:



\- prefer using or extending facade events

\- do not scatter raw `Console.WriteLine` error handling across business actions

\- keep screen logic focused on rendering, input, and flow



---



\## 5.4 Keep console loops safe and understandable



Console screens should use a clean render/input pattern.



Guidelines:



\- separate rendering from blocking input where practical

\- avoid accidental infinite loops

\- keep navigation exits explicit

\- let successful facade actions drive state changes

\- avoid screen flicker caused by unnecessary redraws



Do not make console control flow the authoritative expression of game state progression. That belongs in the shared engine/facade.



---



\## 5.5 New screens require end-to-end updates



If you add a new navigable screen or popup-like surface, update all affected layers.



Typical touchpoints include:



\- `GameScreen`

\- `BirdCafeGame`

\- `ConsoleApp~/Program.cs`

\- relevant `Screens/\*.cs`

\- any supporting ViewModels



Do not add a new screen enum without also wiring the facade and console routing.



---



\## 6. Implementing changes safely



\## 6.1 Adding a new gameplay action



Example: new care action, planning action, or simulation-affecting player choice.



Expected workflow:



1\. Update or add domain enums/constants if needed.

2\. Update config/models if new persistent or tunable data is required.

3\. Implement the logic in the correct manager.

4\. Validate phase and other preconditions.

5\. Return `EngineResult`.

6\. Update `BirdCafeGame` to expose the action.

7\. Update or add ViewModel data if the UI must display it.

8\. Update console UI only as a thin rendering/input layer.

9\. Add tests.



---



\## 6.2 Adding a new screen or popup



Expected workflow:



1\. Define/update the relevant ViewModel.

2\. Expose it through `BirdCafeGame`.

3\. Update screen navigation enums or transitions if applicable.

4\. Wire popup/screen events if needed.

5\. Update `Program.cs` dispatch and the console screen implementation.

6\. Keep business rules in the shared layer.



---



\## 6.3 Adding or changing simulation behavior



Expected workflow:



1\. Put the rule in `SimulationManager` or the correct domain/model helper.

2\. Preserve deterministic seeded randomness.

3\. Update result and summary models only as needed.

4\. Surface new UI data through ViewModels.

5\. Add regression tests.



---



\## 6.4 Adding persistence-affecting state



If a new mechanic must survive day/week/session boundaries:



\- add it to `GameSave` or an owned child model

\- ensure it is initialized correctly in new games

\- ensure it is preserved across load paths

\- update reports/ViewModels if the UI depends on it



Do not hide durable state in manager fields or static caches.



---



\## 7. Testing expectations



Meaningful shared-logic changes should include or update tests.



Prioritize coverage for:



\- new game initialization

\- phase transitions

\- simulation determinism

\- care actions

\- planning and staffing behavior

\- inventory ordering

\- money and ledger effects

\- popularity changes

\- daily summaries

\- weekly summaries

\- game-over behavior

\- bug regressions



\### Regression rule



If you fix a bug in shared logic, add a test that would have caught it.



\### Layering rule



Prefer testing shared logic through managers/facade/domain state rather than relying on console-screen behavior as proof.



---



\## 8. Coding style and maintenance expectations



Follow the repository’s existing style unless there is a strong reason not to.



Prefer:



\- descriptive names

\- clear method responsibilities

\- explicit property-based models

\- small helpers for multi-step workflows

\- XML documentation on public API surface

\- straightforward control flow

\- enums/constants over magic strings



Avoid:



\- cryptic abstractions

\- duplicate logic across layers

\- hidden state transitions

\- catch-all “manager” classes that do everything

\- silent failures

\- UI-specific hacks in shared code



\### Comments

Write comments for:



\- non-obvious invariants

\- determinism requirements

\- phase assumptions

\- subtle financial/state synchronization

\- why a rule exists



Do not add comments that merely restate obvious syntax.



---



\## 9. File placement and project hygiene



\### Shared library

\- place shared source files under `Runtime/`

\- keep namespaces aligned with the shared project structure

\- do not add Unity engine dependencies



\### Console app

\- keep screen/UI code in `ConsoleApp~/`

\- do not move engine logic there



\### Tests

\- place test coverage in `Tests/BirdCafe.Shared.Tests/`

\- keep tests focused on shared behavior, not terminal cosmetics



---



\## 10. Build target expectations



\### `BirdCafe.Shared`

\- target: `netstandard2.1`

\- plain C#

\- Unity-safe

\- runtime code compiled from `Runtime/\*\*/\*.cs`



\### `BirdCafe.Shared.Console`

\- target: `net6.0`

\- reference UI / verification client

\- depends on the shared project



\### Tests

\- target: `net6.0`

\- NUnit-based verification of shared behavior



---



\## 11. Review checklist



Before submitting a contribution, verify all of the following:



\- Shared gameplay logic lives in `BirdCafe.Shared`.

\- New shared files are under `Runtime/`.

\- No Unity engine references were added to the shared library.

\- UI access goes through `BirdCafeGame`.

\- Managers validate phase before mutating state.

\- Command-style operations return `EngineResult`.

\- Simulation randomness remains deterministic for a given day seed.

\- Durable state lives in `GameSave` or owned models.

\- Bird/stat mutations remain clamped to valid ranges.

\- Finance, inventory, and ledger behavior remain coherent.

\- ViewModels expose UI needs without embedding domain logic.

\- Console changes remain thin and facade-driven.

\- Screen routing is updated if navigation changed.

\- Tests were added or updated for meaningful shared-logic changes.



---



\## 12. Priority order when rules conflict



If a requested change creates tension between goals, use this order:



1\. Keep `BirdCafe.Shared` as the single source of truth for gameplay logic.

2\. Preserve compatibility with both Unity and console consumers.

3\. Preserve deterministic, testable state progression.

4\. Keep UI layers thin and facade-driven.

5\. Prefer explicit, maintainable code over clever shortcuts.



---



\## 13. Final principle



Bird Cafe is a layered simulation project with educational value.



Contributions should prioritize:



\- clear architecture

\- explicit state transitions

\- safe state mutation

\- deterministic simulation

\- shared-engine correctness

\- thin UI layers

\- maintainability over hacks



When in doubt, put the rule in the shared engine, expose it through the facade, shape it with a ViewModel, and let the UI simply render it.



