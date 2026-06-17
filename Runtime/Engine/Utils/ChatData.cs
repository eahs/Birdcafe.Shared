using BirdCafe.Shared.ViewModels;
using System.Collections.Generic;
using System;

namespace BirdCafe.Shared.Engine.Utils
{
    /// <summary>
    /// Static data container for the Oracle's dialogue tree.
    /// Refactored to ensure no node exceeds 5 options.
    /// </summary>
    public static class ChatData
    {
        /// <summary>
        /// Root node key used to start or reset Oracle chat navigation.
        /// </summary>
        public const string ROOT_ID = "ROOT";
        private static readonly Dictionary<string, ChatMessage> _nodes = new Dictionary<string, ChatMessage>();
        private static readonly Random _rng = new Random();

        private static readonly List<string> _exitPhrases = new List<string>
        {
            "Maybe let's talk about something else later.",
            "I think I've heard enough for now.",
            "I need to get back to the cafe.",
            "Let's pause this conversation.",
            "I'm good for now, thanks!",
            "Got it. Catch you later.",
            "I have birds to feed, bye!",
            "Let's chat again some other time.",
            "That's all the info I need.",
            "Thanks, Oracle. See ya!"
        };

        static ChatData()
        {
            BuildDialogueTree();
        }

        /// <summary>
        /// Resolves a chat node by id, falling back to the root node when the key is unknown.
        /// </summary>
        /// <param name="id">Node id from a prior chat response option.</param>
        /// <returns>The matching node when found; otherwise the root tutorial/help node.</returns>
        public static ChatMessage GetNode(string id)
        {
            if (_nodes.ContainsKey(id)) return _nodes[id];
            return _nodes[ROOT_ID];
        }

        private static void BuildDialogueTree()
        {
            // --- ROOT (5 Options MAX) ---
            Add(ROOT_ID,
                "<size=120%>Greetings, Bird Boss!</size>\nI am the <color=#00AA00>Oracle of the Aviary</color>. I know everything about running a Bird Cafe. What wisdom do you seek today?",
                "Tell me about Customizing my birds.", "Custom_Intro",
                "How do I take care of them?", "Care_Intro",
                "I want to make more money!", "Money_Intro",
                "How do I win this game?", "Strat_Intro",
                "Misc Mechanics / Lore", "Mech_Hub");

            // =================================================================================
            // BRANCH 1: CUSTOMIZATION
            // =================================================================================
            Add("Custom_Intro",
                "Making the cafe yours starts with your team! You can change names, species, and colors. It's not just about looks; it's about <color=#FF00FF>personality</color>.",
                "Does species matter?", "Custom_Species",
                "Tell me about names.", "Custom_Names",
                "Back to main topics.", ROOT_ID);

            // Refactored to link to Species Hub to save space
            Add("Custom_Species",
                "Absolutely! Different birds have different tendencies, and wil prefer different foods! Figuring out what those preferences are up to you though!",
                "Show me specific bird types...", "Species_Hub",
                "What about visuals?", "Custom_Visuals",
                "Do they have special skills?", "Custom_Skills",
                "Back to customization.", "Custom_Intro");

            Add("Species_Hub", "Select a species to learn more:",
                 "Budgie Info", "Lore_Budgie",
                 "Cockatiel Info", "Lore_Cockatiel",
                 "Lorikeet Info", "Lore_Lorikeet",
                 "Kingfisher Info", "Lore_Kingfisher",
                 "Back to Species", "Custom_Species");

            Add("Custom_Visuals",
                "You can buy and dress your birds up with costumes! These costumes can instantly gather more attention towards your birds.",
                "Do costumes affect popularity?", "Custom_Costumes",
                "What about hats?", "Custom_Accessory",
                "Back to species info.", "Custom_Species");

            Add("Custom_Costumes",
                "Directly? No. But a cool looking bird makes <i>you</i> happy, and a happy Boss makes better decisions!",
                "Fair point. Accessories?", "Custom_Accessory",
                "Okay, let's talk names.", "Custom_Names",
                "Back to visuals.", "Custom_Visuals");

            Add("Custom_Accessory",
                "Hats, glasses, aprons! Accessories are the ultimate flex. Some might even give small stat boosts in the future.",
                "How do I unlock them?", "Custom_Unlock",
                "Can I craft them?", "Custom_Craft",
                "Back to visuals.", "Custom_Visuals");

            Add("Custom_Unlock",
                "Costumes are buyable from Pete's Pet Store! Save up for one and you unlock it for all of your birds!",
                "Cool. What about skills?", "Custom_Skills",
                "Let's talk money.", "Money_Intro",
                "Back to accessories.", "Custom_Accessory");

            Add("Custom_Craft",
                "Not yet! But maybe a crafty bird could learn... for now, just buy them or unlock them.",
                "Okay, unlocking info?", "Custom_Unlock",
                "Back to accessories.", "Custom_Accessory");

            Add("Custom_Skills",
                "Birds have <color=#00AA00>Traits</color>. Some eat less, some work faster. Traits are permanent, so choose your eggs wisely!",
                "Can I change traits?", "Custom_TraitChange",
                "What is the best trait?", "Custom_BestTrait",
                "Back to species.", "Custom_Species");

            Add("Custom_TraitChange",
                "Nope! A bird is who they are. You must love them for their quirks, even if they are a <color=#FF0000>Glutton</color>.",
                "What does Glutton do?", "Custom_Glutton",
                "What is the best trait?", "Custom_BestTrait",
                "Back to skills.", "Custom_Skills");

            Add("Custom_Glutton",
                "A Glutton gets hungry 2x faster! But they get a huge mood boost from snacks. High maintenance, high reward.",
                "Interesting. Other traits?", "Custom_BestTrait",
                "Let's talk Care.", "Care_Intro",
                "Back to trait changing.", "Custom_TraitChange");

            Add("Custom_BestTrait",
                "There is no 'best'. <color=#00AA00>Fast Learner</color> is good for profit. <color=#00AA00>Friendly</color> is good for Popularity. Balance your team!",
                "I want a Friendly bird.", "Custom_Intro",
                "I want a Fast bird.", "Custom_Intro",
                "Back to skills.", "Custom_Skills");

            Add("Custom_Names",
                "Naming is powerful. <i>Sir Chirps</i> sounds professional. <i>Peanut</i> sounds cute. It sets the tone!",
                "Can I rename them?", "Custom_Rename",
                "Do customers see names?", "Custom_CustSeeName",
                "Back to Intro.", "Custom_Intro");

            Add("Custom_Rename",
                "Yes! You can rename your birds in the customize menu. Identity theft is not a crime for birds.",
                "Good to know.", "Custom_Names",
                "Back to names.", "Custom_Names");

            Add("Custom_CustSeeName",
                "They do! If a bird gives bad service, customers might leave a review mentioning <i>Peanut</i> by name. Yikes.",
                "I better train Peanut.", "Care_Intro",
                "I'll rename him.", "Custom_Rename",
                "Back to naming.", "Custom_Names");

            // =================================================================================
            // BRANCH 2: CARE
            // =================================================================================
            Add("Care_Intro",
                "Care is the heartbeat of the cafe. You have 4 main tools: <color=#00AA00>Feed, Play, Rest, and Vet</color>.",
                "Tell me about Food.", "Care_Feed",
                "Tell me about Health.", "Care_Health",
                "Back to Main Menu.", ROOT_ID);

            // Refactored: Moved deep mechanics to sub-menu to keep option count <= 5
            Add("Care_Feed",
                "Food fuels the flight! Hunger drops every day. If it hits 0, your bird takes <color=#FF0000>Health Damage</color> from starvation.",
                "Hunger/Starve Mechanics...", "Care_Food_Deep",
                "Does food cost money?", "Care_FoodCost",
                "Do they like specific food?", "Care_FoodPref",
                "Back to Care.", "Care_Intro");

            Add("Care_Food_Deep", "Details on Hunger Mechanics:",
                "Hunger Logic", "Care_Deep_Hunger",
                "Starvation Penalty", "Care_Deep_Starve",
                "Back to Feed", "Care_Feed");

            Add("Care_Deep_Hunger", "Hunger affects Mood. A hungry bird is a grumpy bird (-Mood/hr). Keep them fed!",
                "Okay", "Care_Feed", "Back", "Care_Feed");
            Add("Care_Deep_Starve", "Starvation kicks in at 0 Hunger. It causes massive HP loss. It's cruel. Don't do it.",
                "I won't", "Care_Feed", "Back", "Care_Feed");

            Add("Care_FoodCost",
                "Yes. Basic seeds are cheap ($5). Premium treats cost more but boost Mood too. Budget for it!",
                "What if I run out of money?", "Money_Bankrupt",
                "Tell me about Play.", "Care_Play",
                "Back to Food.", "Care_Feed");

            Add("Care_FoodPref",
                "Currently, they all eat standard cafe bird mix. It's nutritious and delicious.",
                "Easy enough.", "Care_Feed",
                "Back to Food.", "Care_Feed");

            Add("Care_Play",
                "All work and no play makes a bird <color=#0000FF>Sad</color>. Sad birds work slower and are rude to customers.",
                "How do I play?", "Care_HowPlay",
                "Does playing cost money?", "Care_PlayCost",
                "Back to Care.", "Care_Intro");

            Add("Care_HowPlay",
                "In the Evening menu, select 'Play'. It takes energy but boosts Mood massively. Use it on grumpy birds.",
                "It takes energy?", "Care_Energy",
                "Got it.", "Care_Play",
                "Back to Play info.", "Care_Play");

            Add("Care_PlayCost",
                "Usually, playing is free! It just costs time and Energy. It's the best way to fix a bad mood on a budget.",
                "That's good.", "Care_Play",
                "Let's talk Money.", "Money_Intro",
                "Back to Play info.", "Care_Play");

            Add("Care_Energy",
                "Energy is stamina. Working drains it. Playing drains it. Only <color=#00AA00>Rest</color> restores it fully.",
                "Tell me about Rest.", "Care_Rest",
                "What happens if Energy is low?", "Care_LowEnergy",
                "Back to Play.", "Care_HowPlay");

            Add("Care_Rest",
                "You can toggle a bird to 'Rest' for the next day. They won't work, but they will recover tons of Energy and de-stress.",
                "Can they work 7 days a week?", "Care_Overwork",
                "Do they eat while resting?", "Care_RestEat",
                "Back to Energy.", "Care_Energy");

            Add("Care_Overwork",
                "You monster! I mean... yes, but they will burn out, get sick, and hate you. Rotation is key.",
                "I'll be nice.", "Care_Rest",
                "I need profits though!", "Money_Intro",
                "Back to Rest.", "Care_Rest");

            Add("Care_RestEat",
                "Yes, resting birds still get hungry. Never stop feeding them!",
                "Understood.", "Care_Rest",
                "Back to Rest.", "Care_Rest");

            Add("Care_LowEnergy",
                "Low energy increases the chance of <color=#FF0000>Sickness</color>. Tired birds have weak immune systems.",
                "Tell me about Sickness.", "Care_Sick",
                "How do I fix Energy?", "Care_Rest",
                "Back to Energy.", "Care_Energy");

            Add("Care_Health",
                "Health is life. If it drops, use the Vet immediately. Low health leads to... well, let's not go there.",
                "How much is the Vet?", "Care_VetCost",
                "Can I cure them with food?", "Care_FoodCure",
                "Back to Care.", "Care_Intro");

            Add("Care_Sick",
                "Sickness is a status. A sick bird loses health daily and infects others. Quarantine them by Resting them!",
                "How do I cure it?", "Care_Vet",
                "Can they work while sick?", "Care_SickWork",
                "Back to Energy.", "Care_LowEnergy");

            Add("Care_SickWork", "Technically yes, unless they are Severely Sick. But they will infect others and make customers sad.", "Okay", "Care_Sick", "Back", "Care_Sick");
            Add("Care_FoodCure", "No. Food helps prevent sickness by keeping stats high, but only the Vet can cure an active illness.", "Okay", "Care_Health", "Back", "Care_Health");

            Add("Care_Vet",
                "Select 'Vet Visit' in the menu. It heals them and cures sickness instantly. Modern medicine is amazing.",
                "Is it expensive?", "Care_VetCost",
                "Back to Sickness.", "Care_Sick",
                "Back to Care.", "Care_Intro");

            Add("Care_VetCost",
                "It is pricey ($50+). This is why you need an Emergency Fund. Don't spend all your profit on merch!",
                "I'll save up.", "Strat_Fund",
                "Ouch.", "Care_Health",
                "Back to Vet.", "Care_Vet");

            // =================================================================================
            // BRANCH 3: MONEY
            // =================================================================================
            Add("Money_Intro",
                "Cash rules the cafe. You earn by selling Coffee, Baked Goods, and Merch. You lose money on Stock and Care.",
                "What sells best?", "Money_Products",
                "How do I stop losing money?", "Money_Loss",
                "Back to Main Menu.", ROOT_ID);

            Add("Money_Products",
                "Coffee is steady. Baked Goods are popular but spoil. <color=#00AA00>Merch</color> is pure profit but sells slowly.",
                "Tell me about Spoilage.", "Money_Spoil",
                "Should I buy tons of Merch?", "Money_MerchStrat",
                "Back to Money.", "Money_Intro");

            Add("Money_Spoil",
                "Coffee and Muffins go bad at midnight. If you buy 50 and sell 10, you wasted money on 40. Planning is everything!",
                "How do I predict sales?", "Strat_Reports",
                "That sounds hard.", "Money_Products",
                "Back to Products.", "Money_Products");

            Add("Money_MerchStrat",
                "Merch doesn't spoil! It sits on the shelf until sold. It's a safe investment if you have extra cash.",
                "I'll stock up.", "Money_Products",
                "Back to Products.", "Money_Products");

            Add("Money_Loss",
                "You lose money if: 1. You waste food (Inventory). 2. Your birds are sick (Vet bills). 3. You have no customers (Popularity).",
                "How to fix Popularity?", "Strat_Vibe",
                "How to fix Waste?", "Money_Spoil",
                "Back to Money.", "Money_Intro");

            Add("Money_Bankrupt",
                "If you can't afford food or coffee stock... it's Game Over. The cafe closes. Keep a buffer!",
                "How much buffer?", "Strat_Fund",
                "I'll be careful.", "Money_Intro",
                "Back to previous.", "Money_Intro");

            // =================================================================================
            // BRANCH 4: STRATEGY
            // =================================================================================
            Add("Strat_Intro",
                "Winning requires brain power. Three pillars: <color=#00AA00>Emergency Fund, Vibe Balance, and Data</color>.",
                "The Emergency Fund?", "Strat_Fund",
                "Balancing the Vibe?", "Strat_Vibe",
                "Using Data?", "Strat_Reports",
                "Back to Main Menu.", ROOT_ID);

            Add("Strat_Fund",
                "Keep at least $60 in the bank. That covers one Vet visit ($50) plus food ($5) and coffee stock ($5).",
                "That's a lot.", "Strat_Intro",
                "What if I spend it?", "Money_Bankrupt",
                "Back to Strategy.", "Strat_Intro");

            Add("Strat_Vibe",
                "If birds are Sad, they work slow. Customers wait too long and leave angry. Popularity drops. It's a death spiral!",
                "How to fix it?", "Care_Play",
                "What if birds are happy?", "Strat_Happy",
                "Back to Strategy.", "Strat_Intro");

            Add("Strat_Happy",
                "Happy birds get tips (maybe) and boost Popularity. High Popularity = More Customers = More Money.",
                "I want that.", "Strat_Vibe",
                "Back to Vibe.", "Strat_Vibe");

            Add("Strat_Reports",
                "Check the Evening Summary. If you see 'Wasted: 20', buy 20 less tomorrow. Adjust until waste is zero.",
                "Simple math.", "Strat_Intro",
                "Does history matter?", "Strat_History",
                "Back to Strategy.", "Strat_Intro");

            Add("Strat_History",
                "Yes! Look at the last 7 days. Is the trend going up? Buy more. Going down? Buy less.",
                "I feel smart.", "Strat_Reports",
                "Back to Reports.", "Strat_Reports");

            // =================================================================================
            // MECHANICS & LORE
            // =================================================================================
            Add("Mech_Hub", "Here are some loose ends about how the game world works.",
                "Time Mechanics?", "Mech_Time",
                "Saving the Game?", "Mech_Save",
                "Decorations?", "Mech_Decor",
                "More Topics...", "Mech_Hub2"); // Paging to keep options < 5

            Add("Mech_Hub2", "More mechanics info:",
                "Weeks & Milestones?", "Mech_Week",
                "Failure Conditions?", "Mech_Fail",
                "Lore & Responsibility?", "Lore_Intro",
                "Back to Hub 1", "Mech_Hub");

            Add("Mech_Time", "Time moves only when the Simulation runs. Take all the time you need to plan in the Evening.", "Okay", "Mech_Hub");
            Add("Mech_Save", "The game saves automatically in memory, but use 'Save Game' to keep it on disk.", "Okay", "Mech_Hub");
            Add("Mech_Decor", "Decorations unlock automatically. You don't place them, they just appear to make the cafe pretty.", "Okay", "Mech_Hub");
            Add("Mech_Week", "Weeks act as milestones. Surviving Week 1 is your first major achievement.", "Okay", "Mech_Hub2");
            Add("Mech_Fail", "Failure is part of learning. If you go bankrupt, try again with a cheaper strategy.", "Okay", "Mech_Hub2");

            Add("Lore_Intro",
                "Being a Bird Boss is about responsibility. These pixel birds depend on you entirely.",
                "It's just a game.", "Lore_Game",
                "I love them.", "Lore_Love",
                "Back to Main Menu.", ROOT_ID);

            Add("Lore_Game",
                "Is it? You practice budgeting, care, and planning here. Those are real life skills, Boss!",
                "True.", "Lore_Intro",
                "Back to Lore.", "Lore_Intro");

            Add("Lore_Love",
                "That's the spirit! A loved bird works hard. Treat them as partners, not employees.",
                "They are my friends.", "Lore_Intro",
                "Back to Lore.", "Lore_Intro");

            // --- BIRD SPECIFICS (Leaves) ---
            Add("Lore_Budgie", "Budgies are a common pet bird! They have average stats, and are low maintenance.", "Neat", "Species_Hub", "Back", "Species_Hub");
            Add("Lore_Cockatiel", "Cockatiels are known for their crests on their heads, making them very noticeable in crowds. They are sure to improve the vibes of your cafe!", "Neat", "Species_Hub", "Back", "Species_Hub");
            Add("Lore_Lorikeet", "Lorikeets have thousands of little hairs on their tongue! It might not help with your cafe, but they are also highly productive.", "Neat", "Species_Hub", "Back", "Species_Hub");
            Add("Lore_Kingfisher", "The pinnacle of bird evolution. Kingfishers have evolved to be incredibly energy efficient, even in your cafe!", "Neat", "Species_Hub", "Back", "Species_Hub");
        }

        /// <summary>
        /// Helper to add a node with 4 standard options (Deep, Deep, Back, Exit).
        /// </summary>
        private static void Add(string id, string text, string opt1, string target1, string opt2, string target2, string backText, string backTarget)
        {
            var msg = new ChatMessage
            {
                StateId = id,
                OracleText = text,
                Options = new List<ChatResponseOption>
                {
                    new ChatResponseOption { ResponseText = opt1, NextStateId = target1 },
                    new ChatResponseOption { ResponseText = opt2, NextStateId = target2 },
                    new ChatResponseOption { ResponseText = backText, NextStateId = backTarget },
                    new ChatResponseOption { ResponseText = GetRandomExitPhrase(), NextStateId = ROOT_ID, IsExit = true }
                }
            };
            _nodes[id] = msg;
        }

        /// <summary>
        /// Helper to add a node with arbitrary options (params).
        /// Format: Text, OptionText, TargetId, OptionText, TargetId...
        /// Automatically adds Exit button ONLY if options &lt; 5 and it's not ROOT.
        /// </summary>
        private static void Add(string id, string text, params string[] args)
        {
            var msg = new ChatMessage
            {
                StateId = id,
                OracleText = text,
                Options = new List<ChatResponseOption>()
            };

            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 < args.Length)
                {
                    msg.Options.Add(new ChatResponseOption
                    {
                        ResponseText = args[i],
                        NextStateId = args[i + 1],
                        IsExit = false
                    });
                }
            }

            // Add a dedicated exit button to every node except ROOT if there's room
            if (id != ROOT_ID && msg.Options.Count < 5)
            {
                msg.Options.Add(new ChatResponseOption
                {
                    ResponseText = GetRandomExitPhrase(),
                    NextStateId = ROOT_ID,
                    IsExit = true
                });
            }

            _nodes[id] = msg;
        }

        private static string GetRandomExitPhrase()
        {
            return _exitPhrases[_rng.Next(_exitPhrases.Count)];
        }
    }
}
