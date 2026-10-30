using System;
using MonoMod;

namespace FortRise.Downgrades;

// Fix the speedrun timer desyncs by making it behave like legacy TimeSpan from .NET Framework
// Code Reference: https://github.com/EverestAPI/Everest/blob/cac41cca0309d83fd113f76d7895314502884268/NETCoreifier/Patches/TimeSpan.cs#L8
public static class TimeSpanLegacy 
{
    private const int MillisPerSecond = 1000, MillisPerMinute = MillisPerSecond * 60, MillisPerHour = MillisPerMinute * 60, MillisPerDay = MillisPerHour * 24;


    private static TimeSpan Interval(double value, int scale)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentException("TimeSpan does not accept floating point Not-a-Number values.");
        }

        var tmp = value * scale;

        var millis = tmp + (value >= 0 ? +0.5 : -0.5);

        if ((millis > long.MaxValue / TimeSpan.TicksPerMillisecond) || (millis < long.MinValue / TimeSpan.TicksPerMillisecond))
        {
            throw new OverflowException("TimeSpan overflowed because the duration is too long.");
        }

        return new TimeSpan((long)millis * TimeSpan.TicksPerMillisecond);
    }

    [MonoModLinkFrom("System.TimeSpan System.TimeSpan::FromMilliseconds(System.Double)")]
    public static TimeSpan FromMilliseconds(double val) => Interval(val, 1);

    [MonoModLinkFrom("System.TimeSpan System.TimeSpan::FromSeconds(System.Double)")]
    public static TimeSpan FromSeconds(double val) => Interval(val, MillisPerSecond);

    [MonoModLinkFrom("System.TimeSpan System.TimeSpan::FromMinutes(System.Double)")]
    public static TimeSpan FromMinutes(double val) => Interval(val, MillisPerMinute);

    [MonoModLinkFrom("System.TimeSpan System.TimeSpan::FromHours(System.Double)")]
    public static TimeSpan FromHours(double val) => Interval(val, MillisPerHour);

    [MonoModLinkFrom("System.TimeSpan System.TimeSpan::FromDays(System.Double)")]
    public static TimeSpan FromDays(double val) => Interval(val, MillisPerDay);
}
