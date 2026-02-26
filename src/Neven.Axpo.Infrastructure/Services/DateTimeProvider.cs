using System;
using Neven.Axpo.Application.Services;

namespace Neven.Axpo.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentLocalTime()
    {
        return DateTime.Now;
    }
}