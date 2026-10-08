namespace FortRise;

public interface IUnregisterable<T>
where T : class
{
    void Unload(T entry);
}
