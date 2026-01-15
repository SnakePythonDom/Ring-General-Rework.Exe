namespace RingGeneral.Core.Models
{
    public enum InteractionType
    {
        Talk,
        Alliance,
        Rivalry,
        GimmickEdit,
        Negotiate
    }

    public class InteractionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public InteractionType Type { get; set; }
        public int MoraleChange { get; set; }
        public int RelationChange { get; set; }

        // Potential attribute changes
        public Dictionary<string, int> AttributeChanges { get; set; } = new();

        public static InteractionResult FromSuccess(string message, InteractionType type)
        {
            return new InteractionResult { Success = true, Message = message, Type = type };
        }

        public static InteractionResult FromFailure(string message, InteractionType type)
        {
            return new InteractionResult { Success = false, Message = message, Type = type };
        }
    }
}
