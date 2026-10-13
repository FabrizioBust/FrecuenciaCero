using System;
using UnityEngine;
using UnityEngine.Events;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent, RequireComponent(typeof(NarrativeUI))]
    public sealed class NarrativeDirector : MonoBehaviour
    {
        [SerializeField] private LeoMovement movement = null;
        [SerializeField] private bool allowInteriorEvents = false;
        [SerializeField] private DialogueSequence supervisorConversation = null;
        [SerializeField] private DialogueSequence radioNoResponse = null;
        [SerializeField] private DialogueSequence radioAnomaly = null;
        [SerializeField] private DocumentDefinition logbook = null;
        [SerializeField] private DocumentDefinition map = null;
        [Header("Misión del generador")]
        [SerializeField] private bool allowGeneratorEvents = false;
        [SerializeField] private DialogueSequence generatorAnomaly = null;
        [SerializeField, Min(.1f)] private float generatorAnomalyDelay = 2f;
        [SerializeField, Min(0)] private float charactersPerSecond = 32f;
        [Tooltip("Conectar futuros efectos de audio a los identificadores de cada línea.")]
        [SerializeField] private UnityEvent<string> onSoundCue = new UnityEvent<string>();
        private DialogueSequence sequence;
        private int lineIndex, openedFrame;
        private float lineStartedAt;
        private bool revealLine, movementWasEnabled, ownsMovementLock;
        private Action onComplete;
        public DocumentDefinition Document { get; private set; }
        public string ContextPrompt { get; private set; }
        public bool IsOpen => sequence != null || Document != null;
        public DialogueLine CurrentLine => sequence != null ? sequence.lines[lineIndex] : null;
        public string VisibleText
        {
            get
            {
                string text = CurrentLine?.text ?? string.Empty;
                int count = RevealedCharacters(text.Length);
                return count >= text.Length ? text : text.Substring(0, count);
            }
        }
        public bool IsLineComplete => CurrentLine != null && RevealedCharacters((CurrentLine.text ?? "").Length) >= (CurrentLine.text ?? "").Length;

        public void SetPrompt(string prompt) { ContextPrompt = IsOpen ? null : prompt; }

        public void OpenTarget(RetenInteractionKind kind)
        {
            if (IsOpen || RetenTravel.IsLoading) return;
            MissionProgress progress = RetenMission.Session;
            switch (kind)
            {
                case RetenInteractionKind.Radio:
                    if (progress.SupervisorCompleted) OpenDialogue(radioNoResponse, null);
                    else OpenDialogue(supervisorConversation, progress.CompleteSupervisor);
                    break;
                case RetenInteractionKind.Logbook: OpenDocument(logbook, progress.ReadLogbook); break;
                case RetenInteractionKind.Map: OpenDocument(map, progress.ExamineMap); break;
            }
        }

        public bool TryStartAnomaly()
        {
            if (movement != null && movement.IsBlocked) return false;
            if (allowGeneratorEvents && !IsOpen && !RetenTravel.IsLoading &&
                RetenMission.Session.GeneratorAnomalyReady && RetenMission.GeneratorStartedAt >= 0f &&
                Time.time - RetenMission.GeneratorStartedAt >= generatorAnomalyDelay)
                return OpenDialogue(generatorAnomaly, () => RetenMission.Session.CompleteGeneratorAnomaly());
            if (!allowInteriorEvents || IsOpen || RetenTravel.IsLoading || !RetenMission.Session.AnomalyReady) return false;
            return OpenDialogue(radioAnomaly, RetenMission.Session.CompleteAnomaly);
        }

        public bool OpenDialogue(DialogueSequence dialogue, Action completed)
        {
            if (IsOpen || dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0) return false;
            foreach (DialogueLine line in dialogue.lines)
                if (line == null) { Debug.LogError("El diálogo contiene una línea vacía.", dialogue); return false; }
            sequence = dialogue;
            lineIndex = 0;
            onComplete = completed;
            BeginModal();
            BeginLine();
            return true;
        }

        public bool OpenDocument(DocumentDefinition document, Action completed)
        {
            if (IsOpen || document == null) return false;
            Document = document;
            onComplete = completed;
            BeginModal();
            GetComponent<NarrativeUI>().ResetDocumentScroll();
            return true;
        }

        public void HandleModalInput(bool advance, bool closeDocument)
        {
            if (!IsOpen || Time.frameCount <= openedFrame) return;
            if (Document != null)
            {
                if (closeDocument) CompleteModal();
                return;
            }
            if (!advance) return;
            if (!IsLineComplete) { revealLine = true; return; }
            if (lineIndex + 1 >= sequence.lines.Length) { CompleteModal(); return; }
            lineIndex++;
            BeginLine();
        }

        private int RevealedCharacters(int length) => revealLine || charactersPerSecond <= 0 ? length :
            Mathf.Clamp(Mathf.FloorToInt((Time.unscaledTime - lineStartedAt) * charactersPerSecond), 0, length);

        private void BeginLine()
        {
            revealLine = false;
            lineStartedAt = Time.unscaledTime;
            if (!string.IsNullOrEmpty(CurrentLine.soundCue)) onSoundCue.Invoke(CurrentLine.soundCue);
        }
        private void BeginModal()
        {
            openedFrame = Time.frameCount;
            ContextPrompt = null;
            if (movement == null) return;
            movementWasEnabled = movement.enabled;
            ownsMovementLock = true;
            movement.enabled = false;
            Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
            if (body != null) body.linearVelocity = Vector2.zero;
        }
        private void ReleaseMovement()
        {
            if (ownsMovementLock && movement != null) movement.enabled = movementWasEnabled;
            ownsMovementLock = false;
        }
        private void CompleteModal()
        {
            Action completion = onComplete;
            sequence = null;
            Document = null;
            onComplete = null;
            ReleaseMovement();
            completion?.Invoke();
        }
        private void OnDisable()
        {
            // An interrupted UI does not mark unfinished content as completed.
            sequence = null; Document = null; onComplete = null; ContextPrompt = null;
            ReleaseMovement();
        }
    }
}
