# Bird Cafe

Virtual Pet and Cafe Management Simulation (FBLA Introduction to Programming 2025–2026)

**Developers:** Morgan Kindle, Kevin Olalia, Joshua Tanczos  
**Project Topic:** Virtual Pet Simulation  
**Platform:** Unity 6 LTS (game UI) + C# shared simulation engine (.NET Standard 2.1)  
**Verification Builds:** Console UI (.NET 6) + NUnit tests (.NET 6)

Bird Cafe is a virtual pet game built around the idea that caring for birds should feel personal, meaningful, and consequential. The player is not simply running a cafe with interchangeable workers; the player is building a flock of birds with individual needs, moods, health states, rest schedules, food preferences, and long-term progression. The cafe structure provides a reason to make daily decisions, but the heart of the experience is still pet care: feeding birds, helping them recover, managing sickness risk, giving them rest, buying them food and enrichment items, unlocking cosmetic upgrades, and watching their condition change over time.

The project is designed to reflect the FBLA "Build a Virtual Pet" topic in a way that is both playful and measurable. It focuses on the required ideas of customization, care over time, reactions to treatment, and visible cost-of-care tracking, while also using the cafe simulation as the game’s earning system and feedback loop.

## Submission contents

* A comprehensive README (this file)
* Complete source code for the shared engine (`Runtime/`), console verifier (`ConsoleApp\~/`), and tests (`Tests/`)
* Acknowledgement of any templates, libraries, and third-party assets used (including licenses/permissions)
* AI-assisted code disclosure (see below)

## Judge quick start (recommended paths)

### Option A: Unity (visual experience)

1. Install **Unity 6 LTS** or newer.
2. Open this repository as a Unity project.
3. Press **Play**.

### Option B: Console (fast logic verification)

1. Install the **.NET 6 SDK** (or newer): https://dotnet.microsoft.com/download
2. From the repository root, run:

```bash
cd ConsoleApp\~
dotnet run
```

Console notes:

* The CLI includes a guided tutorial screen.
* On most screens, press **H** for contextual help.
* The chat popup key (**C**) is present as UI scaffolding and currently returns a mock response.
* The console version is useful for judges because it drives the same shared gameplay logic as the Unity front-end.

### Option C: Automated tests (deterministic verification)

From the repository root:

```bash
dotnet test Tests/BirdCafe.Shared.Tests/BirdCafe.Shared.Tests.csproj
```

## At a glance: Bird Cafe virtual pet game

Bird Cafe was intentionally designed so the birds feel like living pets first and productivity units second. The following systems reinforce that goal:

* **Named birds with persistent state**  
Each bird has an identity, species, age stage, level, experience, and multiple care-related stats that persist in save data.
* **Daily care responsibilities**  
The player must decide whether birds need food, play, veterinary care, or a rest day before they are ready to work again.
* **Emotional and physical reactions over time**  
Birds can become hungry, tired, stressed, sick, or unhappy if the player neglects them or overworks them.
* **Pet-oriented spending choices**  
The game tracks food costs, vet costs, toy purchases, costume/customization purchases, and other pet-store expenses through the same economy and ledger system that tracks business income.
* **Pet progression and collection**  
The flock can grow over time through additional bird purchases, and the player can unlock or equip pet-focused rewards such as costumes and egg-toy rewards.
* **Visible outcomes**  
The player can observe the impact of care decisions through daily reports, weekly summaries, bird dashboards, and the birds’ performance during the next day’s simulation.

## How the game works (the day loop)

Bird Cafe runs on a repeatable daily cycle:

1. **Start the day**

   * The player begins with a cafe and a starter bird.
   * On a new game, the player names the player profile and cafe before entering the simulation flow.
   * The current day includes a stored plan, current funds, current bird states, and the cafe’s popularity.
2. **Workday simulation**

   * Customers arrive based on cafe popularity.
   * Birds on duty process orders for **Coffee**, **Baked Goods**, and **Themed Merch**.
   * Birds spend energy while working, and the simulation records sales, lost customers, popularity shifts, inventory use, and financial results.
3. **Evening review**

   * The player reviews the completed day.
   * The game presents summary information about customers, products sold, revenue, expenses, popularity change, and bird outcomes.
4. **Evening care**

   * The player makes pet-care decisions that directly affect the birds’ next-day condition:

     * **Feed**
     * **Play Time**
     * **Vet Visit**
     * **Rest** (scheduled through next-day planning)
   * Each action is validated against the current game state and available funds.
5. **Pet store and next-day planning**

   * The player can visit **Pete’s Pet Store** to buy additional birds, food, toys, costumes, and special egg toys.
   * The player sets which birds will **Work** and which will **Rest**.
   * The player reviews inventory and budget before confirming the next day.
6. **Reporting**

   * The game generates **daily** and **weekly** reports summarizing revenue, expenses, net profit, service performance, popularity, and bird well-being.

## FBLA topic alignment (Virtual Pet Simulation)

Bird Cafe was built to align closely with the 2025–2026 FBLA Introduction to Programming topic. The required topic elements are addressed as follows.

### 1\. Customization

* The player creates and names the cafe at the beginning of the game.
* The game begins with a starter bird and supports a growing flock over time.
* Birds carry persistent identity and progression information such as **name**, **species**, **age stage**, **level**, and **experience**.
* The pet-store system includes **costumes** and cosmetic unlocks such as the **Cafe Bandana**, **Royal Cape**, and special egg-toy rewards.

### 2\. Pet care features

* **Feed** raises hunger toward a healthier state and represents recurring care responsibility.
* **Play Time** improves mood and supports emotional care.
* **Vet Visit** restores health and addresses sickness risk.
* **Rest** is handled through planning by marking birds off-duty so they can recover energy and reduce long-term risk.
* The care dashboard gives the player a direct way to review bird condition before making decisions.

### 3\. Reactions based on care level

* Bird state changes are not cosmetic only. The shared engine tracks **health**, **hunger**, **mood**, **energy**, **stress**, **trust**, and sickness-related conditions.
* Birds lose hunger and mood over time, lose energy while serving customers, and face higher sickness risk when hunger or energy is low.
* Neglect, overwork, or poor planning therefore creates meaningful pet-like consequences:

  * lower health
  * lower mood
  * higher sickness risk
  * weaker next-day performance
  * worse business outcomes

### 4\. Cost of care and financial responsibility

* Every meaningful care decision is tied to the economy system.
* The game tracks:

  * **food and supply costs**
  * **vet visits / health care**
  * **toy and enrichment purchases**
  * **costume/customization purchases**
  * **overall in-game currency and spending**
* Baseline care costs configured in `Runtime/Models/Meta/GameConfig.cs`:

  * Feed: **$5.00**
  * Vet Visit: **$50.00**
  * Play Time: **$0.00**
* Baseline product sale prices configured in the same shared configuration:

  * Coffee: **$3.00**
  * Baked Goods: **$4.50**
  * Themed Merch: **$15.00**

### 5\. Running total of care-related expenses

* Care actions and pet-store purchases flow through the shared economy and ledger logic.
* This gives the player a continuing record of what has been spent on birds, not just what has been earned from the cafe.
* The game is intentionally structured so the player must balance **care spending** and **business planning** rather than treating pet care as an afterthought.

### 6\. Growth and development over time

* Birds are persistent entities rather than temporary round-based units.
* The flock can expand through pet-store bird purchases.
* Birds can level and accumulate experience.
* Bird state evolves over time according to the player’s decisions, especially through repeated patterns of feeding, rest, work assignment, and recovery.
* Cosmetic progression also exists through costume and reward unlocks.

### 7\. Reporting and analysis

* Daily and weekly reports provide a clear record of performance over time.
* Reports summarize profit/loss, customer volume, inventory usage, popularity, and bird outcomes.
* This supports both gameplay reflection and judge review by making the cause-and-effect relationship visible.

## Core pet systems

### Bird identity and progression

Each bird is represented as persistent shared-library data rather than temporary UI state. A bird includes:

* a unique identifier
* display name
* species identifier
* age stage
* level
* experience points

This makes birds feel like individual pets that continue from day to day rather than disposable workers.

### Bird wellness stats

The shared engine models pet care through a set of ongoing stats:

* **Mood** — emotional well-being
* **Health** — physical condition and ability to keep working
* **Hunger** — feeding state
* **Energy** — readiness to work or play
* **Stress** — overload / pressure level

In addition, gameplay performance and relationship systems use values such as:

* **Productivity**
* **Friendliness**
* **Reliability**
* **Trust**
* friendship-based bonuses where applicable

### Decay, recovery, and sickness

Bird care matters because the system does not keep birds in a static "healthy forever" state.

* Hunger decays each day.
* Mood decays each day.
* Energy is spent while serving customers.
* Rest restores additional energy.
* Low hunger and low energy increase sickness risk.
* Severe neglect can lead to direct health damage.

That combination is what makes the game read as a real virtual pet system rather than only a menu of cosmetic buttons.

## Pet store, enrichment, and collection systems

A major part of making Bird Cafe feel pet-centric is the evening **Pete’s Pet Store** loop.

### Buy additional birds

The player can expand the flock by purchasing additional birds from the pet store. This reinforces the idea that the game is about building and caring for a collection of birds over time, not only operating a cafe.

### Buy food and care supplies

The store includes bird food options such as:

* **Fruit Medley**
* **Nutri Pellets**

This supports the rubric’s emphasis on ongoing food and supply costs.

### Buy toys and enrichment items

The store includes toy purchases such as:

* **Feather Wand**
* **Bell Orb**

These purchases help the project feel more like a pet game because spending can directly support enrichment, not only business inventory.

### Buy costumes and cosmetic upgrades

The store also includes costume unlocks such as:

* **Cafe Bandana**
* **Royal Cape**

These help strengthen the customization side of the virtual pet prompt.

### Special egg toys and rewards

The player can buy **Special Egg Toys**, then open them for deterministic rewards. Existing reward examples include:

* **Starlight Spinner**
* **Golden Vest**

This adds a pet-collection and reward component that helps the game feel playful and growth-oriented.

## User experience and judge-facing usability

The rubric values a program that is easy to navigate and easy to understand. This project supports that goal in several ways:

* A **Unity** front-end provides the visual experience.
* A **console reference implementation** is included so judges can quickly verify logic without needing the full visual scene setup.
* The console build includes:

  * a **guided tutorial**
  * contextual **help** through the **H** key
  * a scaffolded **chat/Q\&A** entry point through the **C** key
* The shared library exposes dedicated `ViewModel` types so both UIs can present clean, task-specific screens without duplicating business logic.

## Input validation and edge cases

The engine is designed to prevent invalid or inconsistent state:

* **Funds checks**  
Care actions and pet-store purchases validate the current balance before allowing spending.
* **Phase checks**  
Many actions are only valid in the correct part of the day loop. For example, pet-store operations are restricted to the evening loop.
* **Quantity checks**  
Purchases validate that quantity is positive and that the target item is valid.
* **Stat bounds**  
Bird stats are clamped to safe ranges so values remain meaningful and do not drift into invalid states.
* **Determinism**  
Daily simulation and special egg rewards use persisted or seeded randomness so results are reproducible and testable.

## Architecture overview

Bird Cafe is built with a clean separation between game rules and presentation.

### Shared simulation engine (`BirdCafe.Shared`)

The shared engine is a Unity-compatible **.NET Standard 2.1** library that contains:

* Core models for birds, cafe state, economy, pet-store inventory, planning, simulation, and reports
* Deterministic simulation logic (seeded day-by-day for repeatable outcomes)
* Managers for care, planning, simulation, pet-store behavior, and reporting
* View models designed for UI consumption in both Unity and the console UI

Key entry points:

* `Runtime/Engine/BirdCafeController.cs` — engine controller and manager coordinator
* `Runtime/Engine/BirdCafeGame.cs` — higher-level facade that exposes UI-friendly operations and events
* `Runtime/GameSave.cs` — serializable state container used throughout the engine

### Console reference implementation (judge-friendly)

`ConsoleApp\~` is a lightweight UI over the same shared engine. It is included to let judges quickly verify logic without opening Unity.

### Unity front-end

The Unity project provides the visual interface. Unity scripts should depend on `BirdCafe.Shared` for rules and state, not re-implement simulation logic.

## Repository layout

```text
BirdCafe/
├── Runtime/                         # BirdCafe.Shared (simulation engine)
│   ├── Engine/                      # Controller + managers + helpers
│   ├── Enums/                       # Game and economy enums
│   ├── Models/                      # Birds, economy, planning, simulation summaries
│   └── ViewModels/                  # UI-ready view models
├── ConsoleApp\~/                     # Console UI for fast evaluation
├── Tests/                           # NUnit tests for core logic
└── Assets/                          # Unity project assets (scenes, prefabs, UI, scripts)
```

## How the shared library supports modular, readable code

The FBLA rubric places value on logical modularity, and the project is structured to make that clear.

* `BirdCafe.Shared` contains the core gameplay logic and persistent data.
* `BirdCafe.Shared.Console` is a thin verification UI built on top of the shared engine.
* The shared engine itself is split into focused managers:

  * **MetaManager**
  * **SimulationManager**
  * **CareManager**
  * **PlanningManager**
  * **PetStoreManager**
  * **ReportingManager**
* UI layers are expected to go through `BirdCafeGame` rather than re-implementing rules directly.

This separation makes it easier to test the game, maintain the code, and demonstrate that the logic is organized into purpose-driven modules instead of one large block.

## Build and run notes

* No internet connection is required to run the Unity project, console project, or tests.
* `BirdCafe.Shared` targets **.NET Standard 2.1** for Unity compatibility.
* The console app and tests target **.NET 6.0**.

## Documentation notes for judges

This README is intentionally more detailed than a typical game README because the FBLA rubric explicitly evaluates documentation quality. It is meant to make the following points easy to verify:

* how to run the project
* how the project connects to the assigned virtual pet topic
* which systems are pet-related versus business-related
* how the cost-of-care system works
* where the major code boundaries and modules are
* which third-party resources and generated assets were used

## Third-party assets, libraries, and tools

### Runtime / code libraries

* Unity 6.4 LTS — game runtime and UI framework
* .NET SDK (6+) — console application and tests
* NUnit — automated testing framework

### Unity UI / asset packages

* Cute Kawaii GUI Pack (Ricimi, Unity Asset Store) — UI layout and widgets

## AI-assisted development disclosure

AI tools were used to accelerate repetitive code and early drafts of data models and scaffolding. All code was reviewed, debugged, and integrated by the developers to ensure correctness and alignment with FBLA requirements.

* Tools referenced during development: ChatGPT and Google Gemini
* Scope of assistance: boilerplate generation, draft structures, and iterative refactoring support
* Human responsibility: final design decisions, implementation integration, testing, and submission quality

### Generated Asset Images

|filename|source|
|-|-|
|barista-1.png|Spritesheet generated by Ludo.ai|
|barista-2.png|Spritesheet generated by Ludo.ai|
|bird-cafe-logo-anim.png|Spritesheet generated by Ludo.ai|
|bird-cafe-logo.png|Image generated by ChatGPT 5.4|
|bird-friends.png|Image generated by ChatGPT 5.4|
|blue-budgie.png|Image generated by ChatGPT 5.4|
|Blurred office items on transparent background.png|Image generated by ChatGPT 5.4|
|cafe-background-counter.png|Image generated by ChatGPT 5.4|
|cafe-background.png|Image generated by ChatGPT 5.4|
|cafe-countertop.png|Image generated by ChatGPT 5.4|
|cafe-register.png|Image generated by ChatGPT 5.4|
|Cafe.png|Image generated by ChatGPT 5.4|
|cafe\_background.png|Image generated by ChatGPT 5.4|
|care-background.png|Image generated by ChatGPT 5.4|
|cocka-cockatiel.png|Image generated by ChatGPT 5.4|
|Cozy afternoon workspace by the window (1).png|Image generated by ChatGPT 5.4|
|Cozy coffee shop workspace warmth.png|Image generated by ChatGPT 5.4|
|dwarf-kingfisher.png|Image generated by ChatGPT 5.4|
|EastonAreaSchoolDistrictPrimaryThumbnailImage.png|Image generated by ChatGPT 5.4|
|FBLA-1.png|Image generated by ChatGPT 5.4|
|flower-parallax-foreground.png|Image generated by ChatGPT 5.4|
|hub-after-hours.png|Image generated by ChatGPT 5.4|
|kindpng\_7793376.png|Image generated by ChatGPT 5.4|
|lorikeet-bird.png|Image generated by ChatGPT 5.4|
|lounge\_background.png|Image generated by ChatGPT 5.4|
|Lush garden with vibrant flowers.png|Image generated by ChatGPT 5.4|
|manager\_background.png|Image generated by ChatGPT 5.4|
|petshop-background.png|Image generated by ChatGPT 5.4|
|petshop-salesman-1.png|Spritesheet generated by Ludo.ai|
|petshop-salesman-2.png|Spritesheet generated by Ludo.ai|
|petshop-salesman-idle.png|Spritesheet generated by Ludo.ai|
|petshop-salesman.png|Image generated by ChatGPT 5.4|
|planning-background.png|Image generated by ChatGPT 5.4|
|simulation\_frame.png|Image generated by ChatGPT 5.4|
|Steaming coffee in a ceramic mug.png|Image generated by ChatGPT 5.4|
|yellow-bird-chirping.png|Spritesheet generated by Ludo.ai|
|budgie/base/act\_accept\_treat.png|Spritesheet generated by Ludo.ai|
|budgie/base/act\_chirp\_sing.png|Spritesheet generated by Ludo.ai|
|budgie/base/act\_gift\_received.png|Spritesheet generated by Ludo.ai|
|budgie/base/emo\_angry.png|Spritesheet generated by Ludo.ai|
|budgie/base/emo\_curious.png|Spritesheet generated by Ludo.ai|
|budgie/base/emo\_excited.png|Spritesheet generated by Ludo.ai|
|budgie/base/emo\_love.png|Spritesheet generated by Ludo.ai|
|budgie/base/emo\_proud.png|Spritesheet generated by Ludo.ai|
|budgie/base/emo\_sad.png|Spritesheet generated by Ludo.ai|
|budgie/base/emo\_surprised.png|Spritesheet generated by Ludo.ai|
|budgie/base/idle\_happy.png|Spritesheet generated by Ludo.ai|
|budgie/base/idle\_look.png|Spritesheet generated by Ludo.ai|
|budgie/base/idle\_neutral.png|Spritesheet generated by Ludo.ai|
|budgie/base/idle\_shift.png|Spritesheet generated by Ludo.ai|
|budgie/base/idle\_sleep.png|Spritesheet generated by Ludo.ai|
|budgie/base/idle\_sleepy.png|Spritesheet generated by Ludo.ai|
|canary/base/act\_accept\_treat.png|Spritesheet generated by Ludo.ai|
|canary/base/act\_chirp\_sing.png|Spritesheet generated by Ludo.ai|
|canary/base/act\_gift\_received.png|Spritesheet generated by Ludo.ai|
|canary/base/emo\_angry.png|Spritesheet generated by Ludo.ai|
|canary/base/emo\_curious.png|Spritesheet generated by Ludo.ai|
|canary/base/emo\_excited.png|Spritesheet generated by Ludo.ai|
|canary/base/emo\_love.png|Spritesheet generated by Ludo.ai|
|canary/base/emo\_proud.png|Spritesheet generated by Ludo.ai|
|canary/base/emo\_sad.png|Spritesheet generated by Ludo.ai|
|canary/base/emo\_surprised.png|Spritesheet generated by Ludo.ai|
|canary/base/idle\_happy.png|Spritesheet generated by Ludo.ai|
|canary/base/idle\_look.png|Spritesheet generated by Ludo.ai|
|canary/base/idle\_neutral.png|Spritesheet generated by Ludo.ai|
|canary/base/idle\_shift.png|Spritesheet generated by Ludo.ai|
|canary/base/idle\_sleep.png|Spritesheet generated by Ludo.ai|
|canary/base/idle\_sleepy.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/act\_accept\_treat.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/act\_chirp\_sing.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/act\_gift\_received.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/emo\_angry.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/emo\_curious.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/emo\_excited.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/emo\_love.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/emo\_proud.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/emo\_sad.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/emo\_surprised.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/idle\_happy.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/idle\_look.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/idle\_neutral.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/idle\_shift.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/idle\_sleep.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_Bandana/idle\_sleepy.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/act\_accept\_treat.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/act\_chirp\_sing.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/act\_gift\_received.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/emo\_angry.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/emo\_curious.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/emo\_excited.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/emo\_love.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/emo\_proud.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/emo\_sad.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/emo\_surprised.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/idle\_happy.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/idle\_look.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/idle\_neutral.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/idle\_shift.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/idle\_sleep.png|Spritesheet generated by Ludo.ai|
|canary/Costume\_RoyalCape/idle\_sleepy.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/act\_accept\_treat.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/act\_chirp\_sing.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/act\_gift\_received.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/emo\_angry.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/emo\_curious.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/emo\_excited.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/emo\_love.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/emo\_proud.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/emo\_sad.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/emo\_surprised.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/idle\_happy.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/idle\_look.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/idle\_neutral.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/idle\_shift.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/idle\_sleep.png|Spritesheet generated by Ludo.ai|
|cockatiel/base/idle\_sleepy.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/act\_accept\_treat.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/act\_chirp\_sing.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/act\_gift\_received.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/emo\_angry.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/emo\_curious.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/emo\_excited.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/emo\_love.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/emo\_proud.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/emo\_sad.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/emo\_surprised.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/idle\_happy.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/idle\_look.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/idle\_neutral.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/idle\_shift.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/idle\_sleep.png|Spritesheet generated by Ludo.ai|
|kingfisher/base/idle\_sleepy.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/act\_accept\_treat.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/act\_chirp\_sing.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/act\_gift\_received.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/emo\_angry.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/emo\_curious.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/emo\_excited.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/emo\_love.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/emo\_proud.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/emo\_sad.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/emo\_surprised.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/idle\_happy.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/idle\_look.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/idle\_neutral.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/idle\_shift.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/idle\_sleep.png|Spritesheet generated by Ludo.ai|
|lorikeet/base/idle\_sleepy.png|Spritesheet generated by Ludo.ai|

## License

This project is provided under the **MIT License**. See `LICENSE` in the repository.

## Troubleshooting

* Unity compilation errors: verify that Unity is using a compatible scripting runtime and that the `Runtime/` assembly definition is present.
* `dotnet` not found: install the .NET SDK and restart your terminal.
* Tests failing unexpectedly: run `dotnet test` from the repository root so project references resolve correctly.

