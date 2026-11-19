using UnityEngine;

internal static class PowerQuest
{
    internal enum Progress { Waiting, Accepted, CarryingBattery, Repaired }
    internal static Progress Current { get; private set; }
    internal static bool ReturningToTown { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        Current = Progress.Waiting;
        ReturningToTown = false;
    }

    internal static void Accept()
    {
        if (Current == Progress.Waiting)
            Current = Progress.Accepted;
    }

    internal static void PickUpBattery()
    {
        if (Current < Progress.CarryingBattery)
            Current = Progress.CarryingBattery;
    }

    internal static void Repair()
    {
        if (Current == Progress.CarryingBattery)
            Current = Progress.Repaired;
    }

    internal static void BeginReturn() => ReturningToTown = true;
    internal static void FinishReturn() => ReturningToTown = false;
}
