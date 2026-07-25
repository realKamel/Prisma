namespace Prisma.Application.Abstractions.Services;

public interface IAppLocalizer
{
    string this[string name] { get; }
    string this[string name, params object[] arguments] { get; }
}