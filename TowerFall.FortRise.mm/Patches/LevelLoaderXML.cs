using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall;

public class patch_LevelLoaderXML : LevelLoaderXML
{
    public patch_LevelLoaderXML(Session session) : base(session)
    {
    }

    public extern void orig_ctor(Session session);

    [MonoModConstructor]
    public void ctor(Session session) 
    {
        RiseCore.Events.Invoke_OnLevelLoaded();
        orig_ctor(session);
    }
}