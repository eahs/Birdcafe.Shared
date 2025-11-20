
using System;
using BirdCafe.Shared.Enums;
using BirdCafe.Shared.Engine.Managers;

namespace BirdCafe.Shared.Engine
{
    /// <summary>
    /// The authoritative core engine. 
    /// Manages the GameSave, Phase State Machine, and sub-managers.
    /// </summary>
    public class BirdCafeController
    {
        public GameSave CurrentState { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.Meta;

        // Managers
        public MetaManager Meta { get; private set; }
        public SimulationManager Simulation { get; private set; }
        public CareManager Care { get; private set; }
        public PlanningManager Planning { get; private set; }
        public ReportingManager Reporting { get; private set; }

        public BirdCafeController()
        {
            Meta = new MetaManager(this);
            Simulation = new SimulationManager(this);
            Care = new CareManager(this);
            Planning = new PlanningManager(this);
            Reporting = new ReportingManager(this);
        }

        // Internal Phase Transition Logic used by Managers
        internal void SetPhase(GamePhase newPhase)
        {
            CurrentPhase = newPhase;
        }
        
        internal void SetState(GameSave state)
        {
            CurrentState = state;
        }
    }
}