using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Prisma.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccentColor
{
    Purple = 0,
    Teal = 1,
    Blue = 2
}
