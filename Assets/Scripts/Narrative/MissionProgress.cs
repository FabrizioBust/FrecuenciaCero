namespace FrecuenciaCero
{
    // Session data without scene references. Completion is recorded on close.
    public sealed class MissionProgress
    {
        public bool SupervisorCompleted { get; private set; }
        public bool TreeFallStarted { get; private set; }
        public bool TreeFallen { get; private set; }
        public bool TreeReactionCompleted { get; private set; }
        public bool WreckDiscovered { get; private set; }
        public bool GeneratorAvailable => WreckDiscovered;
        public bool LogbookRead { get; private set; }
        public bool MapExamined { get; private set; }
        public bool AnomalyCompleted { get; private set; }
        public bool GeneratorExamined { get; private set; }
        public bool FuseCollected { get; private set; }
        public bool ToolboxExamined { get; private set; }
        public bool ToolboxOpened { get; private set; }
        public bool EntryLampFailureStarted { get; private set; }
        public bool GeneratorRepaired { get; private set; }
        public bool GeneratorAnomalyCompleted { get; private set; }
        public SurvivalProgress Survival { get; } = new SurvivalProgress();
        public ChapterProgress Chapters { get; } = new ChapterProgress();
        public bool ForestAvailable => GeneratorRepaired && GeneratorAnomalyCompleted;
        public bool EncounterStarted { get; private set; }
        public bool EncounterCompleted { get; private set; }
        public bool NoiseHudUnlocked { get; private set; }
        public bool EscapedEscarbador { get; private set; }
        public bool LevelOneCompleted { get; private set; }
        public bool HasFuse { get { return FuseCollected && !GeneratorRepaired; } }
        public bool GeneratorAnomalyReady { get { return GeneratorRepaired && !GeneratorAnomalyCompleted; } }
        public ObjectiveState Objectives { get; private set; }
        public MissionProgress() { Objectives = new ObjectiveState("investigate", "Investigar el retén"); }
        public int ObjectiveRevision { get { return Objectives.Revision; } }
        // Legacy documents remain optional; their old event is not part of this mission.
        public bool AnomalyReady { get { return false; } }
        public string CurrentObjective { get { return Objectives.Description; } }
        public void CompleteSupervisor()
        {
            if (SupervisorCompleted) return;
            SupervisorCompleted = true;
        }
        public bool TryBeginTreeFall()
        {
            if (!SupervisorCompleted || TreeFallStarted) return false;
            TreeFallStarted = true;
            return true;
        }
        public bool CompleteTreeFall()
        {
            if (!TreeFallStarted || TreeFallen) return false;
            TreeFallen = true;
            Objectives.Set("investigate_noise", "Investigar el ruido del exterior");
            return true;
        }
        public void CompleteTreeReaction() { if (TreeFallen) TreeReactionCompleted = true; }
        public bool CompleteWreckDiscovery()
        {
            if (!TreeFallen || WreckDiscovered) return false;
            WreckDiscovered = true;
            TreeReactionCompleted = true;
            Objectives.Set("locate_generator", "Localizar el generador auxiliar");
            return true;
        }
        public void ReadLogbook() { LogbookRead = true; }
        public void ExamineMap() { MapExamined = true; }
        public void CompleteAnomaly()
        {
            if (!AnomalyReady) return;
            AnomalyCompleted = true;
            Objectives.Set("locate_generator", "Localizar el generador auxiliar");
        }

        public bool ExamineGenerator()
        {
            if (!GeneratorAvailable || GeneratorExamined || GeneratorRepaired) return false;
            GeneratorExamined = true;
            Objectives.Set(FuseCollected ? "install_fuse" : "find_fuse", FuseCollected ?
                "Instalar el fusible en el generador" : "Encontrar un fusible de repuesto");
            return true;
        }
        public bool CollectFuse()
        {
            if (!GeneratorAvailable || FuseCollected || GeneratorRepaired) return false;
            FuseCollected = true;
            Objectives.Set("install_fuse", "Instalar el fusible en el generador");
            return true;
        }
        public bool ExamineToolbox()
        {
            if (!GeneratorAvailable || ToolboxExamined || FuseCollected) return false;
            ToolboxExamined = true;
            return true;
        }
        public bool OpenToolbox()
        {
            if (!GeneratorAvailable || !ToolboxExamined || ToolboxOpened || FuseCollected) return false;
            ToolboxOpened = true;
            return true;
        }
        public bool TryBeginEntryLampFailure()
        {
            if (EntryLampFailureStarted || GeneratorRepaired) return false;
            EntryLampFailureStarted = true;
            return true;
        }
        public bool RepairGenerator()
        {
            if (!GeneratorAvailable || !GeneratorExamined || !HasFuse || GeneratorRepaired) return false;
            GeneratorRepaired = true;
            Objectives.Set("return_service_road", "Regresar al camino de servicio");
            return true;
        }
        public bool CompleteGeneratorAnomaly()
        {
            if (!GeneratorAnomalyReady) return false;
            GeneratorAnomalyCompleted = true;
            Objectives.Set("forest_camp", "Seguir el sendero sur hacia el campamento forestal");
            return true;
        }
        public bool BeginEncounter()
        {
            if (EncounterStarted) return false;
            EncounterStarted = true;
            return true;
        }
        public void FinishEncounter()
        {
            EncounterStarted = EncounterCompleted = NoiseHudUnlocked = true;
            Objectives.Set("listen_roots", "Permanecer quieto y esperar a que la criatura se aleje");
        }
        public void UnlockNoiseHud() => NoiseHudUnlocked = true;
        public void CompleteEvasion()
        {
            if (!EncounterCompleted || EscapedEscarbador) return;
            EscapedEscarbador = true;
            Objectives.Set("roots_exit", "Avanzar con cuidado hacia la salida del claro");
        }
        public void CompleteLevelOne()
        {
            if (!EscapedEscarbador || LevelOneCompleted) return;
            LevelOneCompleted = true;
            Objectives.Set("level_one_complete", "El Sendero del Retén — completado");
            UnityEngine.PlayerPrefs.SetInt("FrecuenciaCero.Progress.Level1Completed", 1);
            UnityEngine.PlayerPrefs.Save();
        }
        public void StartChapterTwo()
        {
            if (Chapters.StartTwo()) Objectives.Set("cabin_trail", "Seguir el sendero hacia la cabaña del guardabosques");
        }
        public void DiscoverCabin()
        {
            Chapters.ExtinguishWindow();
            Objectives.Set("investigate_cabin", "Investigar la cabaña del guardabosques");
        }
        public void EnterCabin()
        {
            Chapters.EnterCabin();
            Objectives.Set("cabin_threshold", "Explorar la cabaña del guardabosques");
        }
    }
}
