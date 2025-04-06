using Esox.Models;

namespace Esox.Services;

public interface IProvider
{
    CommonMethodComputingModel? Model { get; }
}