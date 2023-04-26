using System;
using System.Collections.Generic;

namespace FortRise;

public abstract class RiseModule 
{
    public abstract void Load();
    public abstract void Unload();


    public virtual void LoadContent() {}
    public virtual void Initialize() {}
}