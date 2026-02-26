using System;

namespace Neven.Axpo.Application.Services;

public interface IDateTimeProvider
{
    DateTime GetCurrentLocalTime();
}