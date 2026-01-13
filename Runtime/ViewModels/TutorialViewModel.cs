
using System;
using System.Collections.Generic;

namespace BirdCafe.Shared.ViewModels
{
    [Serializable]
    public class TutorialViewModel
    {
        public string Title { get; set; } = "Your First Day at the Bird Cafe";
        public List<TutorialStep> Steps { get; set; } = new List<TutorialStep>();
    }

    [Serializable]
    public class TutorialStep
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}