using System;
using Neven.Axpo.Application.Services;

namespace Neven.Axpo.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentLocalTime()
    {
        var gmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentLocalDateTimeUtc = DateTime.UtcNow;
        var gmtCurrentLocalDateTime = TimeZoneInfo.ConvertTimeFromUtc(currentLocalDateTimeUtc, gmtTimeZoneInfo);
        return gmtCurrentLocalDateTime;
    }
}