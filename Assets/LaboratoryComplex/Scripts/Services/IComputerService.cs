using System.Collections.Generic;

public interface IComputerService
{
    IReadOnlyList<ComputerDto> GetAll();

    void Connect(string ipAddress);

    void Disconnect(string ipAddress);
}