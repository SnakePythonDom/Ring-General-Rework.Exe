namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Category for worker notes
    /// </summary>
    public enum NoteCategory
    {
        /// <summary>
        /// Booking ideas and creative suggestions
        /// </summary>
        BookingIdeas,

        /// <summary>
        /// Personal notes about the worker
        /// </summary>
        Personal,

        /// <summary>
        /// Injury-related notes
        /// </summary>
        Injury,

        /// <summary>
        /// Other/miscellaneous notes
        /// </summary>
        Other
    }

    /// <summary>
    /// Represents a note about a worker.
    /// Used by bookers to track ideas, observations, and important information.
    /// </summary>
    public class WorkerNote
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Worker ID (Foreign Key)
        /// </summary>
        public int WorkerId { get; set; }

        /// <summary>
        /// Note text content
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Note category
        /// </summary>
        public NoteCategory Category { get; set; } = NoteCategory.Other;

        /// <summary>
        /// When the note was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// When the note was last modified (null if never modified)
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// The worker this note is about
        /// </summary>
        public Worker? Worker { get; set; }

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Get created date formatted (French format)
        /// </summary>
        public string CreatedDateFormatted =>
            CreatedDate.ToString("dd/MM/yyyy HH:mm");

        /// <summary>
        /// Get modified date formatted (French format)
        /// </summary>
        public string? ModifiedDateFormatted =>
            ModifiedDate?.ToString("dd/MM/yyyy HH:mm");

        /// <summary>
        /// Has this note been modified?
        /// </summary>
        public bool WasModified => ModifiedDate.HasValue;

        /// <summary>
        /// Get category display name (French)
        /// </summary>
        public string CategoryDisplayName => Category switch
        {
            NoteCategory.BookingIdeas => "Idées de Booking",
            NoteCategory.Personal => "Personnel",
            NoteCategory.Injury => "Blessure",
            NoteCategory.Other => "Autre",
            _ => "Inconnu"
        };

        /// <summary>
        /// Get category color (hex) for UI
        /// </summary>
        public string CategoryColor => Category switch
        {
            NoteCategory.BookingIdeas => "#3b82f6",  // Blue
            NoteCategory.Personal => "#10b981",      // Green
            NoteCategory.Injury => "#ef4444",        // Red
            NoteCategory.Other => "#6b7280",         // Gray
            _ => "#6b7280"
        };

        /// <summary>
        /// Get category icon emoji
        /// </summary>
        public string CategoryIcon => Category switch
        {
            NoteCategory.BookingIdeas => "💡",
            NoteCategory.Personal => "👤",
            NoteCategory.Injury => "🏥",
            NoteCategory.Other => "📝",
            _ => "📝"
        };

        /// <summary>
        /// Get text preview (first 100 characters)
        /// </summary>
        public string TextPreview
        {
            get
            {
                if (string.IsNullOrEmpty(Text)) return string.Empty;
                return Text.Length > 100
                    ? Text.Substring(0, 97) + "..."
                    : Text;
            }
        }

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Update the note text and set modified date
        /// </summary>
        public void UpdateText(string newText)
        {
            if (newText != Text)
            {
                Text = newText;
                ModifiedDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Update the category and set modified date
        /// </summary>
        public void UpdateCategory(NoteCategory newCategory)
        {
            if (newCategory != Category)
            {
                Category = newCategory;
                ModifiedDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Validate the note
        /// </summary>
        public bool Validate()
        {
            return WorkerId > 0 &&
                   !string.IsNullOrWhiteSpace(Text) &&
                   Enum.IsDefined(typeof(NoteCategory), Category);
        }
    }
}
