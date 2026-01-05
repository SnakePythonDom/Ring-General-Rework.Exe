using RingGeneral.Core.Models;

namespace RingGeneral.Core.Simulation;

public sealed class WeeklyFinanceTick
{
    private readonly FinanceEngine _engine;

    public WeeklyFinanceTick(FinanceEngine engine)
    {
        _engine = engine;
    }

    public FinanceTickResult Executer(WeeklyFinanceContext context)
    {
        return _engine.CalculerFinancesHebdo(context);
    }
}
