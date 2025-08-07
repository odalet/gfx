using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using ScanPlayerWpf.Rendering;

namespace ScanPlayerWpf.Models
{
    public sealed class SceneOptions : ObservableObject, ISceneOptions
    {
        private readonly HashSet<int> disabledHeads = new HashSet<int>();

        public SceneOptions()
        {
            showPlatform = true;
            showHeadReferences = true;
            showHeadFields = false;
            showReference = false;

            showJumps = true;
            showMarks = true;
            showPoints = true;
            showHulls = true;
        }

        public event EventHandler EnabledHeadsChanged;

        private bool showPlatform;
        public bool ShowPlatform
        {
            get => showPlatform;
            set => Set(ref showPlatform, value);
        }

        private bool showHeadReferences;
        public bool ShowHeadReferences
        {
            get => showHeadReferences;
            set => Set(ref showHeadReferences, value);
        }

        private bool showHeadFields;
        public bool ShowHeadFields
        {
            get => showHeadFields;
            set => Set(ref showHeadFields, value);
        }

        private bool showReference;
        public bool ShowReference
        {
            get => showReference;
            set => Set(ref showReference, value);
        }

        private bool showJumps;
        public bool ShowJumps
        {
            get => showJumps;
            set => Set(ref showJumps, value);
        }

        private bool showMarks;
        public bool ShowMarks
        {
            get => showMarks;
            set => Set(ref showMarks, value);
        }

        private bool showPoints;
        public bool ShowPoints
        {
            get => showPoints;
            set => Set(ref showPoints, value);
        }

        private bool showHulls;
        public bool ShowHulls
        {
            get => showHulls;
            set => Set(ref showHulls, value);
        }

        public bool IsHeadEnabled(int id) => !disabledHeads.Contains(id);
        
        public void EnableHead(int id, bool enable)
        {
            var changed = false;
            if (enable && disabledHeads.Contains(id))
                changed = disabledHeads.Remove(id);
            else if (!enable && !disabledHeads.Contains(id))
                changed = disabledHeads.Add(id);

            if (changed) EnabledHeadsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
