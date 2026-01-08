namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Type of contract
    /// </summary>
    public enum ContractType
    {
        /// <summary>
        /// Exclusive contract - wrestler works only for this company
        /// </summary>
        Exclusive,

        /// <summary>
        /// Per-appearance contract - wrestler paid per show
        /// </summary>
        PerAppearance,

        /// <summary>
        /// Developmental contract - training/development deal
        /// </summary>
        Developmental
    }

    /// <summary>
    /// Status of the contract
    /// </summary>
    public enum ContractStatus
    {
        /// <summary>
        /// Active - Currently in effect
        /// </summary>
        Active,

        /// <summary>
        /// Expired - Contract ended naturally
        /// </summary>
        Expired,

        /// <summary>
        /// Terminated - Contract ended early (release, buyout)
        /// </summary>
        Terminated
    }

    /// <summary>
    /// Represents a worker's contract with the company.
    /// Tracks contract history including past and current contracts.
    /// </summary>
    public class ContractHistory
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
        /// Contract start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Contract end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Weekly salary amount
        /// </summary>
        public decimal WeeklySalary { get; set; }

        /// <summary>
        /// Signing bonus (one-time payment)
        /// </summary>
        public decimal SigningBonus { get; set; }

        /// <summary>
        /// Type of contract
        /// </summary>
        public ContractType ContractType { get; set; } = ContractType.Exclusive;

        /// <summary>
        /// Status of the contract
        /// </summary>
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        // ====================================================================
        // NAVIGATION PROPERTIES
        // ====================================================================

        /// <summary>
        /// The worker this contract belongs to
        /// </summary>
        public Worker? Worker { get; set; }

        // ====================================================================
        // HELPER PROPERTIES
        // ====================================================================

        /// <summary>
        /// Is this contract currently active?
        /// </summary>
        public bool IsActive => Status == ContractStatus.Active;

        /// <summary>
        /// Number of weeks remaining in contract
        /// </summary>
        public int WeeksRemaining
        {
            get
            {
                if (!IsActive) return 0;
                var timeSpan = EndDate - DateTime.Now;
                return timeSpan.Days > 0 ? (int)Math.Ceiling(timeSpan.TotalDays / 7.0) : 0;
            }
        }

        /// <summary>
        /// Number of days remaining in contract
        /// </summary>
        public int DaysRemaining
        {
            get
            {
                if (!IsActive) return 0;
                var timeSpan = EndDate - DateTime.Now;
                return timeSpan.Days > 0 ? timeSpan.Days : 0;
            }
        }

        /// <summary>
        /// Is the contract expiring soon? (< 30 days)
        /// </summary>
        public bool IsExpiringSoon => IsActive && DaysRemaining > 0 && DaysRemaining <= 30;

        /// <summary>
        /// Total contract duration in weeks
        /// </summary>
        public int DurationInWeeks
        {
            get
            {
                var timeSpan = EndDate - StartDate;
                return (int)Math.Ceiling(timeSpan.TotalDays / 7.0);
            }
        }

        /// <summary>
        /// Total contract value (weekly salary × weeks)
        /// </summary>
        public decimal TotalValue => WeeklySalary * DurationInWeeks;

        /// <summary>
        /// Total contract value including signing bonus
        /// </summary>
        public decimal TotalValueWithBonus => TotalValue + SigningBonus;

        /// <summary>
        /// Get start date formatted (French format)
        /// </summary>
        public string StartDateFormatted => StartDate.ToString("dd/MM/yyyy");

        /// <summary>
        /// Get end date formatted (French format)
        /// </summary>
        public string EndDateFormatted => EndDate.ToString("dd/MM/yyyy");

        /// <summary>
        /// Get contract type display name (French)
        /// </summary>
        public string ContractTypeDisplayName => ContractType switch
        {
            ContractType.Exclusive => "Exclusif",
            ContractType.PerAppearance => "Par Apparition",
            ContractType.Developmental => "Développement",
            _ => "Inconnu"
        };

        /// <summary>
        /// Get status display name (French)
        /// </summary>
        public string StatusDisplayName => Status switch
        {
            ContractStatus.Active => "Actif",
            ContractStatus.Expired => "Expiré",
            ContractStatus.Terminated => "Résilié",
            _ => "Inconnu"
        };

        /// <summary>
        /// Get status color (hex) for UI
        /// </summary>
        public string StatusColor => Status switch
        {
            ContractStatus.Active => IsExpiringSoon ? "#f59e0b" : "#10b981",  // Orange if expiring, Green if ok
            ContractStatus.Expired => "#6b7280",   // Gray
            ContractStatus.Terminated => "#ef4444", // Red
            _ => "#6b7280"
        };

        // ====================================================================
        // HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Expire the contract (mark as expired)
        /// </summary>
        public void Expire()
        {
            Status = ContractStatus.Expired;
        }

        /// <summary>
        /// Terminate the contract (early termination)
        /// </summary>
        public void Terminate()
        {
            Status = ContractStatus.Terminated;
        }

        /// <summary>
        /// Check if contract should be automatically expired
        /// </summary>
        public void CheckAndExpire()
        {
            if (IsActive && DateTime.Now > EndDate)
            {
                Expire();
            }
        }

        /// <summary>
        /// Validate the contract
        /// </summary>
        public bool Validate()
        {
            return WorkerId > 0 &&
                   StartDate < EndDate &&
                   WeeklySalary >= 0 &&
                   SigningBonus >= 0 &&
                   Enum.IsDefined(typeof(ContractType), ContractType) &&
                   Enum.IsDefined(typeof(ContractStatus), Status);
        }
    }
}
