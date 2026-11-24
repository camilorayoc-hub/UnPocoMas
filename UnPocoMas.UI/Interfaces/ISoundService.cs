namespace UnPocoMas.UI.Interfaces;

public interface ISoundService
{
    Task PlayClickAsync();

    Task PlaySuccessAsync();

    Task PlayFailAsync();
}
