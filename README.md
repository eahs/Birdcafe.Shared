# \# Bird Cafe – Game Design Document

# 

# \## 1. Overview

# 

# \*\*Title:\*\* Bird Cafe

# \*\*Genre:\*\* Cozy pet-management + light business simulation

# \*\*Platform:\*\* Unity (initial focus on desktop; adaptable to others)

# 

# \*\*Core Concept:\*\*

# You run a small cafe staffed by birds. Each day, customers arrive and are served by your birds, buying Coffee, Baked Goods, and Themed Merch. After watching the day play out, you spend the evening caring for your birds and planning inventory and staffing for the next day. Happy, healthy, well-managed birds lead to better service, more customers, and higher profits. Neglect or poor planning can cause your cafe to struggle or shut down.

# 

# \### 1.1 Day Structure

# 

# \* \*\*Each day begins with a work day simulation.\*\*

# 

# &nbsp; \* The cafe opens, customers show up, birds serve them.

# &nbsp; \* Product sales and customer experiences are simulated.

# \* \*\*Evening is for care and planning.\*\*

# 

# &nbsp; \* You review the day’s performance.

# &nbsp; \* You care for birds (feed, play, rest, vet).

# &nbsp; \* You plan inventory purchases and which birds will work the next day.

# 

# \### 1.2 Special Rules for Day 1 and Weeks

# 

# \* \*\*Day 1:\*\*

# 

# &nbsp; \* Occurs on a \*\*Monday\*\*.

# &nbsp; \* The player starts with a \*\*preset amount of Coffee\*\* and watches the first simulation with no prior planning (tutorial-like).

# &nbsp; \* After the Day 1 simulation ends, the player gets their first evening phase and starts making real decisions.

# \* \*\*Days \& Weeks:\*\*

# 

# &nbsp; \* Days are numbered: \*\*Day 1, Day 2, Day 3, ...\*\*

# &nbsp; \* Each day also has a day-of-week label (e.g., Monday, Tuesday).

# &nbsp; \* \*\*Weeks are recorded as Sunday–Saturday\*\* for summary and reporting:

# 

# &nbsp;   \* Weekly summaries always cover Sunday through Saturday.

# &nbsp;   \* Day 1 (Monday) falls into its appropriate Sunday–Saturday week window.

# 

# ---

# 

# \## 2. Design Goals

# 

# \### 2.1 Gameplay Goals

# 

# \* Provide a \*\*relaxed, visually appealing\*\* experience with short, satisfying daily cycles.

# \* Keep the systems straightforward but deep enough to support multiple weeks of play:

# 

# &nbsp; \* Few product categories: Coffee, Baked Goods, Themed Merch.

# &nbsp; \* Clear connections between decisions and outcomes.

# \* Encourage players to think about:

# 

# &nbsp; \* Resource management (money, inventory).

# &nbsp; \* The trade-off between caring for birds and maximizing profit.

# 

# \### 2.2 Educational Goals

# 

# \* Illustrate the \*\*cost of pet care\*\*:

# 

# &nbsp; \* Food and basic supplies.

# &nbsp; \* Vet visits and health care.

# &nbsp; \* Toys and enrichment.

# &nbsp; \* Cosmetic and functional upgrades.

# \* Demonstrate \*\*budgeting and consequences\*\*:

# 

# &nbsp; \* Overspending on inventory or care can reduce profit.

# &nbsp; \* Underspending on care leads to unhealthy, unhappy birds, which harms the business over time.

# \* Reinforce that \*\*animals are responsibilities\*\*, not just decorations.

# 

# ---

# 

# \## 3. Target Audience

# 

# \* \*\*Age:\*\* Middle school and up, including families and casual players.

# \* \*\*Experience:\*\* Suitable for players with little or no management-game experience.

# \* \*\*Educational Use:\*\* Can support classroom discussions about:

# 

# &nbsp; \* Personal finance and budgeting.

# &nbsp; \* Planning and forecasting.

# &nbsp; \* Responsibility in pet ownership.

# 

# ---

# 

# \## 4. Core Loop \& Day Flow

# 

# \### 4.1 Daily Flow

# 

# Each in-game day follows this structure:

# 

# \*\*1. Work Day Simulation (Start of Day)\*\*

# The game automatically begins the day’s simulation:

# 

# \* The cafe opens for business.

# \* Customers arrive over time based on:

# 

# &nbsp; \* The cafe’s \*\*Popularity\*\*.

# &nbsp; \* A \*\*Popularity-to-Customer factor\*\* defined in the configuration.

# &nbsp; \* Some randomness to keep days varied.

# \* Each customer wants to buy something:

# 

# &nbsp; \* Coffee

# &nbsp; \* Baked Goods

# &nbsp; \* Themed Merch

# \* Birds serve customers:

# 

# &nbsp; \* Birds’ \*\*Productivity\*\*, \*\*Friendliness\*\*, and \*\*Reliability\*\* (all on 1–100 scales) determine:

# 

# &nbsp;   \* How quickly they can serve.

# &nbsp;   \* How many customers they can handle.

# &nbsp;   \* How often errors or slowdowns occur.

# &nbsp; \* Birds’ \*\*Mood\*\*, \*\*Health\*\*, \*\*Hunger\*\*, \*\*Energy\*\*, and \*\*Stress\*\* (also on 1–100 scales) influence these stats.

# \* The simulation resolves:

# 

# &nbsp; \* How many items are sold.

# &nbsp; \* How many customers leave unserved.

# &nbsp; \* How much income is earned.

# &nbsp; \* How much perishable inventory is wasted.

# &nbsp; \* Changes to cafe Popularity.

# &nbsp; \* Changes to each bird’s state (tiredness, hunger, mood, sickness chance).

# 

# The player primarily watches during this phase; it is a visual payoff for prior planning.

# 

# ---

# 

# \*\*2. Evening Phase (Care, Planning, and Budgeting)\*\*

# 

# After the work day simulation ends, the evening begins. The evening is the core decision-making time.

# 

# \*\*2.1 Review the Day\*\*

# 

# \* Summary of the day:

# 

# &nbsp; \* Customers arrived, served, and lost.

# &nbsp; \* Sales of Coffee, Baked Goods, and Themed Merch.

# &nbsp; \* Wasted Coffee and Baked Goods.

# &nbsp; \* Total income and expenses for the day.

# &nbsp; \* Net profit.

# &nbsp; \* Change in Popularity.

# \* Quick snapshot of each bird:

# 

# &nbsp; \* Mood, Health, Hunger, Energy, Stress before and after the day (or current state).

# 

# \*\*2.2 Care for Birds\*\*

# 

# The player selects care actions for each bird. Care actions adjust 1–100 attributes:

# 

# \* \*\*Feed:\*\*

# 

# &nbsp; \* Increases Hunger (toward 100, “full”).

# &nbsp; \* Can improve Health and Mood slightly, depending on food quality.

# \* \*\*Play:\*\*

# 

# &nbsp; \* Increases Mood and may reduce Stress.

# &nbsp; \* Uses some Energy.

# \* \*\*Rest:\*\*

# 

# &nbsp; \* The player can mark a bird as resting for the next day.

# &nbsp; \* This recovers Energy and can lower Stress.

# &nbsp; \* Resting birds do not work during the next day’s simulation.

# \* \*\*Vet Visit:\*\*

# 

# &nbsp; \* Used if a bird is sick or at risk.

# &nbsp; \* Has a monetary cost.

# &nbsp; \* Improves Health and can remove sickness status.

# &nbsp; \* The bird may need to rest afterward and not work next day.

# 

# Care decisions directly influence how birds will perform in the next simulation, as well as their likelihood of sickness and mood swings.

# 

# \*\*2.3 Plan and Budget for the Next Day\*\*

# 

# The player sets the \*\*Daily Plan\*\* for the next day:

# 

# \* Inventory planning:

# 

# &nbsp; \* Quantity of \*\*Coffee\*\* to purchase.

# &nbsp; \* Quantity of \*\*Baked Goods\*\* to purchase.

# &nbsp; \* Quantity of \*\*Themed Merch\*\* to purchase.

# \* Staffing:

# 

# &nbsp; \* Which birds will work tomorrow.

# &nbsp; \* Which birds will rest tomorrow.

# \* Budgeting:

# 

# &nbsp; \* An optional self-imposed \*\*daily budget limit\*\* that the player tries not to exceed (for learning purposes).

# \* Price awareness:

# 

# &nbsp; \* Each product type has a \*\*base sale price\*\* defined in the game configuration.

# &nbsp; \* The player can see these prices.

# &nbsp; \* Future versions of the game may allow the player to adjust sale prices for the next day, but for now price control is a planned extension, not a core mechanic.

# 

# When the player confirms the evening decisions, the Daily Plan is saved and will govern the next day’s work day simulation.

# 

# ---

# 

# \## 5. Systems Design

# 

# \### 5.1 Bird System

# 

# Each bird is central to the game and has:

# 

# \*\*Identity and Progression\*\*

# 

# \* Name (chosen by the player).

# \* Species (which may affect base stats or appearance).

# \* Age stage (e.g., hatchling, juvenile, adult).

# \* Level and experience, which may gradually increase stats through good care and work.

# 

# \*\*State Attributes (All on 1–100 Scales)\*\*

# 

# \* \*\*Mood:\*\*

# 

# &nbsp; \* 1 = extremely angry or sad, 100 = ecstatic.

# &nbsp; \* Influenced by care (feeding, play), workload, sickness, and stress.

# \* \*\*Health:\*\*

# 

# &nbsp; \* 1 = very sick, 100 = perfectly healthy.

# &nbsp; \* Affects likelihood of becoming or staying sick, and ability to work.

# \* \*\*Hunger:\*\*

# 

# &nbsp; \* 1 = starving, 100 = fully fed.

# &nbsp; \* Low Hunger increases sickness chance and reduces mood and performance.

# \* \*\*Energy:\*\*

# 

# &nbsp; \* 1 = exhausted, 100 = fully rested.

# &nbsp; \* Low Energy reduces productivity and may increase mistakes.

# \* \*\*Stress:\*\*

# 

# &nbsp; \* 1 = completely calm, 100 = extremely stressed.

# &nbsp; \* High Stress reduces mood and reliability, can lead to more frequent negative events.

# 

# \*\*Work Stats (Also on 1–100 Scales)\*\*

# 

# \* \*\*Productivity:\*\*

# 

# &nbsp; \* How many customers a bird can realistically serve in a day.

# \* \*\*Friendliness:\*\*

# 

# &nbsp; \* How much each successful interaction improves customer satisfaction and Popularity.

# \* \*\*Reliability:\*\*

# 

# &nbsp; \* How often the bird avoids mistakes, spills, and delays.

# 

# \*\*Customization \& Traits\*\*

# 

# \* Visual customization:

# 

# &nbsp; \* Primary feather color.

# &nbsp; \* Secondary feather color.

# &nbsp; \* Beak color.

# &nbsp; \* Costume.

# &nbsp; \* Accessories (e.g., hats, glasses, apron).

# \* Traits:

# 

# &nbsp; \* Permanent modifiers that shape play style, such as:

# 

# &nbsp;   \* “Fast Learner” (gains productivity more quickly).

# &nbsp;   \* “Shy” (less impact on popularity but gets stressed more slowly).

# &nbsp;   \* “Glutton” (loses Hunger faster but gains more Mood from feeding).

# 

# \*\*Sickness \& Availability\*\*

# 

# \* Birds can become sick, especially when:

# 

# &nbsp; \* Hunger is low.

# &nbsp; \* Energy is low.

# &nbsp; \* They are repeatedly overworked or not given rest.

# \* A severely sick bird may not be available to work the next day.

# \* Vet visits can cure sickness but cost money.

# 

# ---

# 

# \### 5.2 Products \& Inventory System

# 

# The cafe sells only three product categories:

# 

# \* \*\*Coffee\*\*

# \* \*\*Baked Goods\*\*

# \* \*\*Themed Merch\*\* (mugs, posters, t-shirts)

# 

# \*\*Perishability\*\*

# 

# \* \*\*Coffee\*\* and \*\*Baked Goods\*\* are \*\*perishable\*\*:

# 

# &nbsp; \* They are purchased for a specific day via the Daily Plan.

# &nbsp; \* Unsold units at the end of the day are \*\*wasted\*\* and removed from inventory.

# \* \*\*Themed Merch\*\* is \*\*non-perishable\*\*:

# 

# &nbsp; \* It remains in inventory across days until sold.

# &nbsp; \* It has a single standard price defined by the game configuration.

# 

# \*\*Pricing\*\*

# 

# \* The game configuration defines a \*\*base sale price\*\* for:

# 

# &nbsp; \* Coffee

# &nbsp; \* Baked Goods

# &nbsp; \* Themed Merch

# \* These base prices are used during sales.

# \* The design anticipates that \*\*future versions\*\* may allow the player or advanced systems to adjust sale prices for the next day, but this is not required for the initial implementation.

# 

# ---

# 

# \### 5.3 Customers \& Popularity

# 

# \*\*Popularity (1–100)\*\*

# 

# \* A single value that represents how attractive the cafe is to customers.

# \* Affects:

# 

# &nbsp; \* The base number of customers that will arrive each day.

# &nbsp; \* The chance of special or high-value customers.

# \* Changes due to:

# 

# &nbsp; \* Good service (short waits, birds in good mood and health).

# &nbsp; \* Poor service (long waits, errors, unserved customers).

# &nbsp; \* Visible bird issues (very unhappy or sick birds may hurt reputation).

# 

# \*\*Popularity-to-Customer Factor\*\*

# 

# \* Defined in the Game Configuration:

# 

# &nbsp; \* A numeric factor that determines how much Popularity increases the number of customers.

# &nbsp; \* For example, higher Popularity multiplies with this factor to raise daily customer counts.

# \* Combined with:

# 

# &nbsp; \* A baseline customer count.

# &nbsp; \* A random variance to keep days unpredictable.

# 

# \*\*Customer Behavior\*\*

# 

# \* Customers arrive during the work day simulation.

# \* Each customer:

# 

# &nbsp; \* Has a preferred product type (Coffee, Baked Goods, or Themed Merch).

# &nbsp; \* Tries to purchase that product if it is in stock and a bird is able to serve them.

# \* Possible outcomes:

# 

# &nbsp; \* If served successfully:

# 

# &nbsp;   \* The appropriate product is sold.

# &nbsp;   \* Income is earned.

# &nbsp;   \* Popularity may increase based on bird performance.

# &nbsp; \* If not served in time, or if product is unavailable:

# 

# &nbsp;   \* The customer leaves.

# &nbsp;   \* Popularity may decrease.

# 

# Different abstract “customer types” may exist (regulars, super fans, critics, special guests), each with slightly different spending patterns and popularity effects.

# 

# ---

# 

# \### 5.4 Cost-of-Care \& Economy

# 

# The economy tracks:

# 

# \* \*\*Income:\*\*

# 

# &nbsp; \* Primarily from sales of Coffee, Baked Goods, and Themed Merch.

# \* \*\*Expenses:\*\*

# 

# &nbsp; \* Inventory purchases (all product categories).

# &nbsp; \* Bird food and supplies.

# &nbsp; \* Toys and enrichment activities.

# &nbsp; \* Vet visits and treatments.

# &nbsp; \* Cosmetic and functional upgrades for birds.

# 

# A \*\*ledger\*\* records each transaction, including:

# 

# \* Amount (positive for income, negative for expense).

# \* Reason and category.

# \* Related products or birds, if applicable.

# 

# \*\*Budgeting:\*\*

# 

# \* During the evening, the player plans the next day’s spending.

# \* The player may choose an internal daily budget limit for themselves.

# \* The game does not enforce savings goals, but the ledger and summaries make it easy to see how different spending choices affect profitability.

# 

# ---

# 

# \### 5.5 Weekly Summary \& Feedback

# 

# Every week (Sunday–Saturday), the game generates a Weekly Summary:

# 

# \* \*\*Financial Overview:\*\*

# 

# &nbsp; \* Total income.

# &nbsp; \* Total care-related expenses.

# &nbsp; \* Total inventory-related expenses.

# &nbsp; \* Net profit for the week.

# \* \*\*Bird Welfare:\*\*

# 

# &nbsp; \* Average Health (1–100).

# &nbsp; \* Average Mood (1–100).

# \* \*\*Business Health:\*\*

# 

# &nbsp; \* Total change in Popularity over the week.

# &nbsp; \* Average customers per day.

# \* \*\*Narrative Feedback:\*\*

# 

# &nbsp; \* A short paragraph describing the week.

# &nbsp; \* Highlights of major trends (e.g., “You wasted many baked goods” or “Your birds stayed mostly healthy and energized.”)

# 

# The weekly summary helps players see the longer-term impact of their care and budgeting decisions.

# 

# ---

# 

# \### 5.6 Progression \& Failure

# 

# \*\*Progression:\*\*

# 

# \* New birds can be introduced over time (e.g., new eggs).

# \* Birds can level up and become more productive.

# \* The cafe can unlock:

# 

# &nbsp; \* New thematic merch or cosmetic items.

# &nbsp; \* Special visitors or small narrative events.

# 

# \*\*Failure Conditions:\*\*

# 

# \* If the player’s money balance becomes too low to:

# 

# &nbsp; \* Purchase minimal necessary inventory for the next day AND

# &nbsp; \* Afford basic care costs for the birds,

# \* Or if Popularity falls so low that almost no customers arrive:

# 

# &nbsp; \* The cafe effectively cannot operate.

# \* In such cases:

# 

# &nbsp; \* The game shows a clear summary of what led to the shutdown (spending patterns, care/neglect, popularity collapse).

# 

# ---

# 

# \## 6. User Interface \& Flow (High-Level)

# 

# \### 6.1 Work Day Simulation Screen

# 

# \* Visible elements:

# 

# &nbsp; \* Current day number and day-of-week.

# &nbsp; \* Current Popularity value.

# &nbsp; \* Brief display of birds currently working.

# &nbsp; \* Real-time counters for:

# 

# &nbsp;   \* Customers arrived.

# &nbsp;   \* Customers served.

# &nbsp;   \* Customers lost.

# &nbsp;   \* Coffee sold.

# &nbsp;   \* Baked Goods sold.

# &nbsp;   \* Themed Merch sold.

# &nbsp;   \* Current income for the day.

# \* The player mostly watches; this is the payoff for prior evening decisions.

# 

# \### 6.2 Evening Screen

# 

# \* \*\*Day Summary Panel:\*\*

# 

# &nbsp; \* Profit or loss for the day.

# &nbsp; \* Items sold and wasted.

# &nbsp; \* Change in Popularity.

# \* \*\*Bird Care Panel:\*\*

# 

# &nbsp; \* List of birds.

# &nbsp; \* Current values for Mood, Health, Hunger, Energy, Stress (1–100).

# &nbsp; \* Buttons or controls for:

# 

# &nbsp;   \* Feed, Play, Rest, Vet Visit.

# &nbsp; \* Clear display of the cost and expected attribute changes for each action.

# \* \*\*Next-Day Plan Panel:\*\*

# 

# &nbsp; \* Controls to set purchase quantities for:

# 

# &nbsp;   \* Coffee

# &nbsp;   \* Baked Goods

# &nbsp;   \* Themed Merch

# &nbsp; \* Controls to mark which birds will work and which will rest tomorrow.

# &nbsp; \* Optional field or indicator for a self-imposed daily budget target.

# \* \*\*Money \& Overview Panel:\*\*

# 

# &nbsp; \* Current money balance.

# &nbsp; \* High-level breakdown of today’s expenses (care vs inventory).

# &nbsp; \* Simple indication if the player is trending toward profit or loss.

# 

# \### 6.3 Weekly Summary Screen

# 

# \* Presented at the end of each Saturday (or when a week’s days are complete).

# \* Shows:

# 

# &nbsp; \* Financial totals and net profit.

# &nbsp; \* Bird welfare averages.

# &nbsp; \* Popularity trend.

# &nbsp; \* A short narrative summary.

# 

# ---

# 

# \## 7. Data Model / Object Definitions

# 

# This section lists all objects that need structured data, and their properties, in plain language. Attribute scales of \*\*1–100\*\* are used where specified.

# 

# \### 7.1 Global / Meta Objects

# 

# \#### Object: Game Save

# 

# Represents a complete snapshot of the current game.

# 

# \* Save Identifier

# \* Schema Version (for migration if data structures change in the future)

# \* Current Day Number (Day 1, Day 2, etc.)

# \* Current Day-of-Week Name (Monday, Tuesday, etc.)

# \* Current Week Number

# \* Flag: Day 1 Intro/Tutorial Completed (yes/no)

# \* Player Profile (linked object)

# \* Player Preferences (linked object)

# \* Cafe State (linked object)

# \* Economy State (linked object)

# \* List of Bird objects owned by the player

# \* Story State (linked object)

# \* List of Day Summaries for past days

# \* List of Weekly Summaries for past weeks

# 

# ---

# 

# \#### Object: Player Profile

# 

# Identifies and describes the player.

# 

# \* Player Identifier

# \* Player Display Name

# \* Creation Date and Time of the profile

# \* Starting Funds value used for this profile

# 

# ---

# 

# \#### Object: Player Preferences

# 

# Player-specific settings.

# 

# \* Tutorials Enabled (yes/no)

# \* Tooltips/Hints Enabled (yes/no)

# \* Text or UI Scale (for readability)

# \* Animations Enabled (yes/no)

# \* Preferred Language or Locale Code

# 

# ---

# 

# \#### Object: Game Configuration

# 

# Tunable game-wide values and base settings.

# 

# \* Base Number of Customers per Day at neutral Popularity

# \* Popularity-to-Customer Factor

# 

# &nbsp; \* A numeric factor that determines how strongly Popularity affects daily customer count.

# \* Default Starting Inventory for Day 1

# 

# &nbsp; \* For example, initial Coffee quantity so the player can watch the first day.

# \* Baseline Sickness Chance per Bird per Day

# \* Multipliers that adjust sickness chance based on:

# 

# &nbsp; \* Low Hunger

# &nbsp; \* Low Energy

# \* Base Sale Price for Coffee

# \* Base Sale Price for Baked Goods

# \* Base Sale Price for Themed Merch

# \* Baseline cost values for:

# 

# &nbsp; \* Bird food and basic supplies

# &nbsp; \* Toys and enrichment activities

# &nbsp; \* Vet visits and treatments

# &nbsp; \* Upgrades and customizations

# \* Definition of Week Boundaries (Sunday–Saturday) for summary aggregation

# 

# ---

# 

# \### 7.2 Cafe \& Economy Objects

# 

# \#### Object: Cafe

# 

# Current state of the cafe business.

# 

# \* Cafe Name

# \* Current Popularity (1–100)

# \* Inventory State (linked object)

# \* List of Unlocked Product Variants, if any (e.g., special blends or themed baked goods)

# \* List of Unlocked Decorations or Themed Elements (visual flavor, even if not tracked as a numeric decor level)

# 

# ---

# 

# \#### Object: Inventory State

# 

# Tracks product stocks.

# 

# \* Coffee Inventory Entry (linked object)

# \* Baked Goods Inventory Entry (linked object)

# \* Themed Merch Inventory Entry (linked object)

# 

# ---

# 

# \#### Object: Product Inventory Entry

# 

# Data for a single product category.

# 

# \* Product Type (Coffee, Baked Goods, Themed Merch)

# \* Quantity on Hand (current stock)

# \* Current Sale Price per Unit (often equal to the base sale price, unless adjusted in a future extension)

# \* Average Cost Basis per Unit (what the player effectively paid, used for analysis)

# \* Perishable Flag (true for Coffee and Baked Goods, false for Themed Merch)

# \* Quantity Purchased for the Current Day (for tracking waste)

# \* Quantity Left Over from Previous Days (relevant mainly for Themed Merch; often zero for perishables)

# 

# ---

# 

# \#### Object: Economy State

# 

# Overall financial status of the game.

# 

# \* Current Money Balance

# \* List of Ledger Entries representing all transactions

# 

# ---

# 

# \#### Object: Ledger Entry

# 

# One financial transaction.

# 

# \* Ledger Entry Identifier

# \* Date and Time of Transaction

# \* Amount (positive for income, negative for expense)

# \* Reason (e.g., product sale, inventory purchase, food purchase, vet bill, toy purchase, upgrade purchase, refund, other)

# \* Expense Category (e.g., food and supplies, vet care, toys and activities, upgrades and customization, inventory for Coffee, inventory for Baked Goods, inventory for Themed Merch, miscellaneous)

# \* Related Product Type, if applicable

# \* Related Bird Identifier, if applicable (for bird-specific costs like vet care)

# \* Short Description explaining the transaction in plain language

# 

# ---

# 

# \### 7.3 Bird \& Care Objects

# 

# \#### Object: Bird

# 

# Represents one bird in the game.

# 

# \*\*Identity \& Progression\*\*

# 

# \* Bird Identifier

# \* Name chosen by the player

# \* Species Identifier

# \* Age Stage (e.g., hatchling, juvenile, adult)

# \* Level (progression level)

# \* Experience Points accumulated

# 

# \*\*State Attributes (1–100)\*\*

# 

# \* Mood (1–100; 1 = very unhappy, 100 = ecstatic)

# \* Health (1–100; 1 = very sick, 100 = perfectly healthy)

# \* Hunger (1–100; 1 = starving, 100 = fully fed)

# \* Energy (1–100; 1 = exhausted, 100 = fully rested)

# \* Stress (1–100; 1 = calm, 100 = extremely stressed)

# 

# \*\*Work Stats (1–100)\*\*

# 

# \* Productivity (1–100; how many customers they can effectively serve)

# \* Friendliness (1–100; how positive their interactions are for Popularity)

# \* Reliability (1–100; how rarely they make mistakes)

# 

# \*\*Flags \& Work Assignment\*\*

# 

# \* Currently Sick Flag (yes/no)

# \* Severely Sick Flag (yes/no) – cannot or should not work when true

# \* Assigned Day Off for Next Day (yes/no)

# \* Worked During Last Simulation Day (yes/no)

# 

# \*\*Customization\*\*

# 

# \* Primary Feather Color

# \* Secondary Feather Color

# \* Beak Color

# \* Costume Identifier

# \* List of Accessory Identifiers (hats, glasses, apron, etc.)

# 

# \*\*Traits \& Equipment\*\*

# 

# \* List of Bird Traits (linked objects)

# \* List of Equipped Items (linked objects)

# 

# ---

# 

# \#### Object: Bird Trait

# 

# A permanent characteristic of a bird.

# 

# \* Trait Type (e.g., Fast Learner, Shy, Glutton, Friendly, Stoic, Clumsy)

# \* Trait Description (what it means in plain language)

# \* Gameplay Impact Description (how it affects stats or behavior, in general terms)

# 

# ---

# 

# \#### Object: Equipped Item

# 

# An item used or worn by a bird.

# 

# \* Item Identifier

# \* Display Name

# \* Item Category (e.g., purely cosmetic, productivity booster, friendliness booster, reliability booster, mixed)

# \* Productivity Bonus (how much it improves productivity, if relevant)

# \* Friendliness Bonus (how much it improves friendliness, if relevant)

# \* Reliability Bonus (how much it improves reliability, if relevant)

# \* Special Conditions (text description, such as “only active when bird is healthy” or “only during special events”)

# 

# ---

# 

# \#### Object: Care Action Template

# 

# Defines a type of care action the player can perform in the evening.

# 

# \* Care Action Identifier (e.g., feed basic food, play with toy, vet visit)

# \* Display Name

# \* Monetary Cost to perform the action

# \* List of Attribute Changes applied when used, such as:

# 

# &nbsp; \* Hunger increase

# &nbsp; \* Mood increase

# &nbsp; \* Health change

# &nbsp; \* Energy change

# &nbsp; \* Stress change

# \* Any Daily Use Limit or other constraints (e.g., can only use once per day per bird)

# 

# ---

# 

# \#### Object: Care Action Record (Optional)

# 

# Records a specific care action performed on a bird.

# 

# \* Day Number

# \* Bird Identifier

# \* Care Action Identifier (which template was used)

# \* Money Spent for this action

# \* Summary of Attribute Changes applied to the bird (for history and analytics)

# 

# ---

# 

# \### 7.4 Time, Planning \& Simulation Objects

# 

# \#### Object: Day State

# 

# Tracks the current day’s status.

# 

# \* Day Number

# \* Day-of-Week Name

# \* Week Number

# \* Flag indicating whether the work day simulation has been completed

# \* Reference to the Daily Plan that governs this day

# 

# ---

# 

# \#### Object: Daily Plan

# 

# Represents decisions made during the evening for the next day.

# 

# \* Day Number this plan applies to

# \* Planned Coffee Purchase Quantity for the next day

# \* Planned Baked Goods Purchase Quantity for the next day

# \* Planned Themed Merch Purchase Quantity for the next day

# \* Optional Daily Budget Limit (a self-chosen spending cap)

# \* List of Bird Identifiers assigned to work the next day

# \* List of Bird Identifiers assigned to rest the next day

# \* Optional Notes or Tags describing the plan strategy (for flavor or debugging)

# 

# ---

# 

# \#### Object: Day Simulation Result

# 

# Stores the outcome of the day’s work simulation.

# 

# \*\*High-Level Day Data\*\*

# 

# \* Day Number

# \* Total Customers Who Arrived

# \* Total Customers Served

# \* Total Customers Lost (unserved)

# 

# \*\*Product Sales \& Waste\*\*

# 

# \* Coffee Sold (number of units)

# \* Coffee Wasted (end-of-day unsold units)

# \* Baked Goods Sold

# \* Baked Goods Wasted

# \* Themed Merch Sold (no waste, since non-perishable)

# 

# \*\*Financial Outcome\*\*

# 

# \* Total Income from sales for the day

# \* Total Expenses for the day (inventory plus care actions taken earlier that affect the day)

# \* Net Profit for the day

# \* Popularity Change (positive or negative value)

# 

# \*\*Bird-Specific Outcomes\*\*

# 

# For each bird that exists:

# 

# \* Bird Identifier

# \* Worked Today (yes/no)

# \* Number of Customers Served by this bird (if worked)

# \* Mood at Start of Day (1–100)

# \* Mood at End of Day (1–100)

# \* Health at Start of Day (1–100)

# \* Health at End of Day (1–100)

# \* Hunger at Start of Day (1–100)

# \* Hunger at End of Day (1–100)

# \* Energy at Start of Day (1–100)

# \* Energy at End of Day (1–100)

# \* Became Sick During the Day (yes/no)

# \* Recovered from Sickness During the Day (yes/no)

# 

# \*\*Events\*\*

# 

# \* List of Game Event Records generated during the day (see below)

# 

# ---

# 

# \#### Object: Day Summary

# 

# Compact day-level record used for history and weekly summaries.

# 

# \* Day Number

# \* Day-of-Week Name

# \* Week Number

# \* Reference to the Day Simulation Result

# \* Total Care Expenses for the Day (sum from ledger)

# \* Total Inventory Expenses for the Day (sum from ledger)

# \* Net Profit for the Day

# 

# ---

# 

# \#### Object: Weekly Summary

# 

# Aggregated report from Sunday to Saturday.

# 

# \* Week Number

# \* First Day Number included in this week

# \* Last Day Number included in this week

# 

# \*\*Financial Totals\*\*

# 

# \* Total Income during the week

# \* Total Care-Related Expenses (food, vet, toys, upgrades)

# \* Total Inventory-Related Expenses (Coffee, Baked Goods, Themed Merch purchases)

# \* Net Profit for the week

# 

# \*\*Bird Welfare\*\*

# 

# \* Average Bird Health across all birds and days this week (1–100)

# \* Average Bird Mood across all birds and days this week (1–100)

# 

# \*\*Business Health\*\*

# 

# \* Total Popularity Change during the week

# \* Average Number of Customers per Day for the week

# 

# \*\*Narrative Summary\*\*

# 

# \* One or more sentences describing the week in plain language

# \* Optional bullet list of notable highlights (e.g., “Bird X was sick for 3 days,” “Large waste of baked goods on Thursday”)

# 

# ---

# 

# \### 7.5 Customers \& Story Objects

# 

# \#### Object: Customer Type Definition

# 

# Defines behavior of a category of customers.

# 

# \* Customer Type Name (e.g., Regular, Curious Visitor, Super Fan, Critic, Special Guest)

# \* Typical Spending Pattern (average number of items they buy)

# \* Typical Positive Popularity Impact when satisfied

# \* Typical Negative Popularity Impact when dissatisfied

# \* Special Behavior Notes (e.g., may give feedback, may appear at higher popularity only)

# 

# \*(You may not need to store each individual customer; counts by type and day are often enough.)\*

# 

# ---

# 

# \#### Object: Story State

# 

# Tracks narrative progress and special events.

# 

# \* Flag indicating whether the opening “mysterious egg” story has been shown

# \* Flag indicating whether the Day 1 tutorial is complete

# \* List of Triggered Story Event Identifiers

# \* Flags for Special Narrative Milestones (e.g., “featured in local paper”, “reached major popularity milestone”)

# 

# ---

# 

# \#### Object: Game Event Record

# 

# Generic event log entry for significant in-game occurrences.

# 

# \* Event Identifier

# \* Event Type description (e.g., day started, day ended, bird got sick, bird recovered, product sold, product wasted, customer served, popularity changed, game over)

# \* Date and Time of the Event

# \* Day Number associated with the event

# \* Week Number associated with the event

# \* Related Bird Identifier, if applicable

# \* Related Product Type, if applicable

# \* Money Amount associated with the event, if applicable

# \* Short Text Description or Payload giving additional detail



