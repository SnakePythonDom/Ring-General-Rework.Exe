using RingGeneral.Core.Models;

namespace RingGeneral.Core.Interfaces;

public interface IValidator
{
    ValidationResult ValiderBooking(BookingPlan plan);
}
