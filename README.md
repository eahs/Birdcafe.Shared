
# Bird Cafe
Virtual Pet and Cafe Management Simulation (FBLA Introduction to Programming 2025–2026)

**Developers:** Morgan Kindle, Kevin Olalia, Joshua Tanczos  
**Project Topic:** Virtual Pet Simulation  
**Platform:** Unity 2022.3 LTS (game UI) + C# shared simulation engine (.NET Standard 2.1)  
**Verification Builds:** Console UI (.NET 6) + NUnit tests (.NET 6)

Bird Cafe is a virtual pet experience framed as a small cafe business. Players care for a team of bird “employees” while running a simple daily operation: plan staffing, serve customers, review financial results, and keep birds healthy and happy. The game is designed to highlight tradeoffs between the **cost of care** (food, enrichment, veterinary care) and **business performance** (revenue, popularity, inventory, and net profit).

## Submission contents
- A comprehensive README (this file)
- Complete source code for the shared engine (`Runtime/`), console verifier (`ConsoleApp~/`), and tests (`Tests/`)
- Acknowledgement of any templates, libraries, and third-party assets used (including licenses/permissions)
- AI-assisted code disclosure (see below)

## Judge quick start (recommended paths)

### Option A: Unity (visual experience)
1. Install **Unity 2022.3 LTS** or newer (Unity Hub is recommended).
2. Open this repository as a Unity project.
3. Open the main menu scene (typically under `Assets/Scenes/`).
4. Press **Play**.

### Option B: Console (fast logic verification)
1. Install the **.NET 6 SDK** (or newer): https://dotnet.microsoft.com/download
2. From the repository root, run:

```bash
cd ConsoleApp~
dotnet run

```

Console notes:

-   The CLI includes a guided tutorial screen.    
-   On most screens, press **H** for contextual help.    
-   The chat popup key (**C**) is present as UI scaffolding and returns a mock response.
    

### Option C: Automated tests (deterministic verification)

From the repository root:

```bash
dotnet test Tests/BirdCafe.Shared.Tests/BirdCafe.Shared.Tests.csproj

```

## How the game works (the day loop)

Bird Cafe runs on a repeatable daily cycle:

1.  **Morning planning**
    
    -   Assign birds to **Work** or **Rest** for the day.        
    -   Review current inventory and recent performance.
        
2.  **Workday simulation**
    
    -   Customers arrive based on cafe popularity.        
    -   Birds on duty process orders (Coffee, Baked Goods, Themed Merch).        
    -   Results produce revenue, inventory consumption, and performance outcomes.
        
3.  **Evening care**
    
    -   Care actions improve bird well-being and reduce risk:        
        -   **Feed**            
        -   **Play Time**            
        -   **Vet Visit**            
    -   Each action is validated against available funds.
        
4.  **Reporting**
    
    -   The game generates **daily** and **weekly** reports summarizing revenue, expenses, net profit, service performance, and bird outcomes.
        

## FBLA topic alignment (Virtual Pet Simulation)

Bird Cafe implements the required topic elements as follows:

### Pet customization

-   Players create and name their cafe and begin with a starter bird.
    

### Care features

-   **Feed:** improves hunger and supports health    
-   **Play Time:** improves mood    
-   **Vet Visit:** restores health and addresses sickness risk    
-   **Rest:** handled through the planning phase by scheduling birds off-duty    

### Reactions and behaviors

-   Bird stats (health, hunger, mood, energy, and sickness risk) change based on care and workload.
    
-   Neglect and overwork produce measurable consequences that impact both bird well-being and cafe performance.
    

### Cost of care and spending tracking

-   Every care action is recorded as an expense in the financial ledger.
    
-   Baseline care costs are configured in `Runtime/Models/Meta/GameConfig.cs`:
    
    -   Feed: $5.00        
    -   Vet Visit: $50.00        
    -   Play Time: $0.00
        

### Earning and saving

-   Revenue is generated during the workday simulation through customer purchases.
    
-   Base product prices are configured in `Runtime/Models/Meta/GameConfig.cs`:
    
    -   Coffee: $3.00        
    -   Baked Goods: $4.50        
    -   Themed Merch: $15.00        

### Reporting

-   Daily and weekly reports provide a clear record of performance over time (profit/loss, customer volume, inventory usage, and bird outcomes).
    

## Architecture overview

Bird Cafe is built with a clean separation between game rules and presentation.

### Shared simulation engine (`BirdCafe.Shared`)

The shared engine is a Unity-compatible **.NET Standard 2.1** library that contains:

-   Core models (birds, cafe, economy, inventory, planning, and reports)    
-   Deterministic simulation logic (seeded day-by-day for repeatable outcomes)    
-   Managers for care, planning, and simulation    
-   View models designed for UI consumption (Unity UI and the console UI)
    

Key entry points:

-   `Runtime/Engine/BirdCafeController.cs` — engine controller and manager coordinator    
-   `Runtime/Engine/BirdCafeGame.cs` — higher-level facade that exposes UI-friendly operations and events    
-   `Runtime/GameSave.cs` — serializable state container used throughout the engine    

### Console reference implementation (judge-friendly)

`ConsoleApp~` is a lightweight UI over the same shared engine. It is included to let judges quickly verify logic without opening Unity.

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
├── ConsoleApp~/                     # Console UI for fast evaluation
├── Tests/                           # NUnit tests for core logic
└── Assets/                          # Unity project assets (scenes, prefabs, UI, scripts)

```

## Build and run notes

-   No internet connection is required to run the Unity project, console project, or tests.    
-   `BirdCafe.Shared` targets **.NET Standard 2.1** for Unity compatibility.    
-   The console app and tests target **.NET 6.0**.
    

## Input validation and edge cases

The engine is designed to prevent invalid or inconsistent state:

-   **Funds checks:** care actions validate the current balance before allowing spending.    
-   **Stat bounds:** bird stats are clamped to safe ranges to avoid invalid values.    
-   **Determinism:** daily simulation uses a seeded RNG so results are reproducible and testable.
    

## Third-party assets, libraries, and tools

### Runtime / code libraries

-   Unity 2022.3 LTS — game runtime and UI framework    
-   .NET SDK (6+) — console application and tests    
-   NUnit — automated testing framework
    

### Unity UI / asset packages

-   Cute Kawaii GUI Pack (Ricimi, Unity Asset Store) — UI layout and widgets    
-   Easy Tooltip (Ahmed Benlakhdhar) — tooltip/popup UX components
    

## AI-assisted development disclosure

AI tools were used to accelerate repetitive code and early drafts of data models and scaffolding. All code was reviewed, debugged, and integrated by the developers to ensure correctness and alignment with FBLA requirements.

-   Tools referenced during development: ChatGPT and Google Gemini    
-   Scope of assistance: boilerplate generation, draft structures, and iterative refactoring support    
-   Human responsibility: final design decisions, implementation integration, testing, and submission quality    

## License

This project is provided under the **MIT License**. See `LICENSE` in the repository.

## Troubleshooting

-   Unity compilation errors: verify that Unity is using a compatible scripting runtime and that the `Runtime/` assembly definition is present.
    
-   `dotnet` not found: install the .NET SDK and restart your terminal.
    
-   Tests failing unexpectedly: run `dotnet test` from the repository root so project references resolve correctly.