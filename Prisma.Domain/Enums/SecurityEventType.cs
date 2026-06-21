using System.Text.Json.Serialization;

namespace Prisma.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SecurityEventType { TabSwitch, CopyPasteAttempt }

