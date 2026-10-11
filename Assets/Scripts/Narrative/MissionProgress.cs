namespace FrecuenciaCero
{
    // Session data without scene references. Completion is recorded on close.
    public sealed class MissionProgress
    {
        public bool SupervisorCompleted { get; private set; }
        public bool LogbookRead { get; private set; }
        public bool MapExamined { get; private set; }
        public bool AnomalyCompleted { get; private set; }
        public bool GeneratorExamined { get; private set; }
        public bool FuseCollected { get; private set; }
        public bool GeneratorRepaired { get; private set; }
        public bool GeneratorAnomalyCompleted { get; private set; }
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
            Objectives.Set("locate_generator", "Localizar el generador auxiliar");
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
            if (!SupervisorCompleted || GeneratorExamined || GeneratorRepaired) return false;
            GeneratorExamined = true;
            Objectives.Set(FuseCollected ? "install_fuse" : "find_fuse", FuseCollected ?
                "Instalar el fusible en el generador" : "Encontrar un fusible de repuesto");
            return true;
        }
        public bool CollectFuse()
        {
            if (!SupervisorCompleted || FuseCollected || GeneratorRepaired) return false;
            FuseCollected = true;
            Objectives.Set("install_fuse", "Instalar el fusible en el generador");
            return true;
        }
        public bool RepairGenerator()
        {
            if (!SupervisorCompleted || !GeneratorExamined || !HasFuse || GeneratorRepaired) return false;
            GeneratorRepaired = true;
            Objectives.Set("return_service_road", "Regresar al camino de servicio");
            return true;
        }
        public bool CompleteGeneratorAnomaly()
        {
            if (!GeneratorAnomalyReady) return false;
            GeneratorAnomalyCompleted = true;
            return true;
        }
    }
}
