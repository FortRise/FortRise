using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall.Editor;

public class patch_Actor : Actor
{
    private Vector2 ExportOrigin
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
    }

    private Vector2 ExportNode
    {
        [MonoModIgnore]
        get
        {
            throw new NotImplementedException();
        }
        [MonoModIgnore]
        set
        {
            throw new NotImplementedException();
        }
    }

    public patch_ActorData Data;

    public List<Vector2> Nodes;
    public Dictionary<string, string> Attributes;
    public bool AttributeMoused;

    public bool Invalid;

    public patch_Actor(Level level, Vector2 position, ActorData data) : base(level, position, data)
    {
    }

    public patch_Actor(Actor actorAt) : base(actorAt)
    {
    }

    [MonoModLinkTo("TowerFall.Editor.Actor", "System.Void .ctor(TowerFall.Editor.Level,Microsoft.Xna.Framework.Vector2,TowerFall.Editor.ActorData)")]
    [MonoModIgnore]
    public void this_ctor(Level level, Vector2 position, ActorData data) {}


    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(Level level, Vector2 position, patch_ActorData data)
    {
        Level = level;
        Data = data;
        Width = Data.DefaultWidth;
        Height = Data.DefaultHeight;
        Position = EnforceBounds(position);
        if (data.HasNode)
        {
            Nodes = [];
        }
        if (data.Attributes is not null)
        {
            Attributes = new (data.Attributes);
        }
        OnCreateMove();
    }

    [MonoModConstructor]
    [MonoModReplace]
    public void ctor(patch_Actor actor)
    {
        this_ctor(actor.Level, actor.Position, actor.Data);
        Width = actor.Width;
        Height = actor.Height;
        Node = actor.Node;
        Nodes = actor.Nodes;
        Attributes = actor.Attributes;
    }

    [MonoModConstructor]
    [MonoModReplace]       
    public void ctor(Level level, XmlElement xml, string name)
    {
        Level = level;
        Data = (patch_ActorData)ActorData.Data[name];

        if (Data.ResizeableX)
        {
            Width = xml.AttrInt("width", Data.DefaultWidth);
        }
        else
        {
            Width = Data.DefaultWidth;
        }

        if (Data.ResizeableY)
        {
            Height = xml.AttrInt("height", Data.DefaultHeight);
        }
        else
        {
            Height = Data.DefaultHeight;
        }

        ExportPosition = xml.Position();
        if (Data.HasNode)
        {
            var list = new List<Vector2>();
            foreach (XmlElement node in xml.GetElementsByTagName("node"))
            {
                var x = node.AttrInt("x");
                var y = node.AttrInt("y");

                list.Add(new Vector2(x, y) - ExportOrigin);
            }
            if (list.Count != 0)
            {
                Node = list[0];
            }

            Nodes = list;
        }

        if (Data.Attributes is {})
        {
            Attributes = [];
            foreach (XmlAttribute attr in xml.Attributes)
            {
                if (attr.Name is "x" or "y" or "width" or "height")
                {
                    continue;
                }

                Attributes.Add(attr.Name, attr.Value);
            }
        }
    }

    [MonoModReplace]
    public XmlElement ToXML(XmlDocument doc)
    {
        XmlElement xmlElement = doc.CreateElement(Data.Name);
        xmlElement.SetAttr("x", (int)ExportPosition.X);
        xmlElement.SetAttr("y", (int)ExportPosition.Y);

        if (Data.ResizeableX)
        {
            xmlElement.SetAttr("width", Width);
        }

        if (Data.ResizeableY)
        {
            xmlElement.SetAttr("height", Height);
        }

        if (Data.HasNode)
        {
            foreach (var node in Nodes)
            {
                XmlElement nodeElement = xmlElement.CreateChild("node");
                nodeElement.SetAttr("x", (node + ExportOrigin).X);
                nodeElement.SetAttr("y", (node + ExportOrigin).Y);
            }
        }
        
        if (Attributes is not null)
        {
            foreach (var (key, obj) in Attributes)
            {
                xmlElement.SetAttr(key, obj.ToString());
            }
        }

        return xmlElement;
    }

    [MonoModReplace]
    private void SingleRender(ActorsLayerUI ui, Vector2 at, float alpha)
    {
        if (!ui.Collidable)
        {
            DrawActor(this, at, alpha);
        }
        else
        {
            if (ui.MousedActor == this)
            {
                Draw.Rect(at.X - Width / 2 - 3f, at.Y - Height / 2 - 3f, Width + 6, Height + 6, Color.Yellow * 0.6f);
            }
            else
            {
                Draw.Rect(at.X - Width / 2 - 1f, at.Y - Height / 2 - 1f, Width + 2, Height + 2, Color.White * 0.4f);
            }

            DrawActor(this, at, alpha);
            if (ui.MousedActor == this)
            {
                Color color = ResizeMoused ? Color.Lime : Color.White;
                float scale = ResizeMoused ? 1.5f : 1f;
                if (Data.ResizeableX)
                {
                    if (Data.ResizeableY)
                    {
                        Draw.TextureCentered(TFGame.EditorAtlas["resizeXY"], at + new Vector2(Width / 2, Height / 2), color, scale, 0f);
                    }
                    else
                    {
                        Draw.TextureCentered(TFGame.EditorAtlas["resizeX"], at + new Vector2(Width / 2, 0f), color, scale, 0f);
                    }
                }
                else if (Data.ResizeableY)
                {
                    Draw.TextureCentered(TFGame.EditorAtlas["resizeY"], at + new Vector2(0f, Height / 2), color, scale, 0f);
                }

                if (Data.Attributes is { Count: > 0 })
                {
                    Color attrColor = AttributeMoused ? Color.Lime : Color.White;
                    Draw.TextureCentered(
                        TFGame.MenuAtlas["editor/attribute"], at + new Vector2(-8f, -(Height / 2)), attrColor, scale, 0f);
                }
            }

            if (Data.HasNode && ui.MousedActor == this)
            {
                Color color = NodeMoused ? Color.Lime : Color.White;
                float scale = NodeMoused ? 1.5f : 1f;
                Draw.TextureCentered(TFGame.EditorAtlas["node"], at + new Vector2(Width / 2, -(Height / 2)), color, scale, 0f);
            }
        }
    }

    [MonoModReplace]
    public patch_Actor SymmetryClone(SymmetryAgent.SymmetryType symmetry)
    {
        Vector2 position = Position;
        position.X = 160f + (160f - position.X);
        if (symmetry == SymmetryAgent.SymmetryType.LeftToRight)
        {
        }
        patch_Actor actor = new patch_Actor(Level, position, Data)
        {
            Width = Width,
            Height = Height
        };

        if (Data.HasNode)
        {
            actor.Nodes = [..Nodes];
            for (int i = 0; i < actor.Nodes.Count; i += 1)
            {
                var node = actor.Nodes[i];
                actor.Nodes[i] = new Vector2(160f + (160f - node.X), node.Y);
            }
        }

        return actor;
    }


    [MonoModReplace]
    public void Render(ActorsLayerUI ui, float alpha)
    {
        SingleRender(ui, Position, alpha);

        if (Data.AllowScreenWrap)
        {
            int horizontalFacing = 0;
            int verticalFacing = 0;
            if (Position.X + Width / 2 > 320f)
            {
                horizontalFacing = -1;
                SingleRender(ui, Position + Vector2.UnitX * -320f, alpha);
            }
            else if (Position.X - Width / 2 < 0f)
            {
                horizontalFacing = 1;
                SingleRender(ui, Position + Vector2.UnitX * 320f, alpha);
            }
            if (Position.Y + Height / 2 > 240f)
            {
                verticalFacing = -1;
                SingleRender(ui, Position + Vector2.UnitY * -240f, alpha);
            }
            else if (Position.Y - Height / 2 < 0f)
            {
                verticalFacing = 1;
                SingleRender(ui, Position + Vector2.UnitY * 240f, alpha);
            }
            if (horizontalFacing != 0 && verticalFacing != 0)
            {
                SingleRender(ui, Position + new Vector2(320 * horizontalFacing, 240 * verticalFacing), alpha);
            }
        }

        if (Data.HasNode)
        {
            if (Nodes is { Count: > 0 })
            {
                Vector2 previousNode = Position;
                foreach (var node in Nodes)
                {
                    int idx = Nodes.IndexOf(node);
                    Color color = NodeMoused ? Color.Lime : Color.White;
                    float scale = NodeMoused ? 1.5f : 1f;
                    if (idx == Nodes.Count - 1)
                    {
                        Draw.TextureCentered(
                            TFGame.EditorAtlas["node"], 
                            node + new Vector2(Width / 2, (float)(-(float)Height / 2)), color, scale, 0f);
                    }


                    DrawActor(this, node, alpha * 0.5f);
                    Draw.Line(previousNode, node, Color.Lime * alpha * 0.3f);
                    previousNode = node;
                }
            }
        }
    }

    [MonoModReplace]
    public MouseMode CheckPosition(Vector2 position)
    {
        Rectangle rectangle;
        if (!EditorBase.Shift && !EditorBase.Ctrl)
        {
            if (Data.ResizeableX)
            {
                if (Data.ResizeableY)
                {
                    rectangle = new Rectangle((int)(Position.X + Width / 2 - 4f), (int)(Position.Y + Height / 2 - 4f), 8, 8);
                }
                else
                {
                    rectangle = new Rectangle((int)(Position.X + Width / 2 - 4f), (int)Position.Y - 4, 8, 8);
                }

                if (CheckRect(rectangle, position))
                {
                    return MouseMode.Resize;
                }
            }
            else if (Data.ResizeableY)
            {
                rectangle = new Rectangle((int)(Position.X - 4f), (int)(Position.Y + Height / 2 - 4f), 8, 8);
                if (CheckRect(rectangle, position))
                {
                    return MouseMode.Resize;
                }
            }

            if (Data.HasNode)
            {
                rectangle = new Rectangle((int)(Position.X + Width / 2 - 4f), (int)(Position.Y - Height / 2 - 4f), 8, 8);
                if (CheckRect(rectangle, position))
                {
                    return MouseMode.Node;
                }


                if (Nodes is not null && Nodes.Count > 0)
                {
                    var lastNode = Nodes[^1];
                    rectangle = new Rectangle((int)(lastNode.X + Width / 2 - 4f), (int)(lastNode.Y - Height / 2 - 4f), 8, 8);
                    if (CheckRect(rectangle, position))
                    {
                        return MouseMode.NodeExtended;
                    }
                }
            }

            if (Data.Attributes is { Count: > 0 })
            {
                rectangle = new Rectangle((int)(Position.X - 8f), (int)(Position.Y - Height / 2 - 4f), 8, 8);
                if (CheckRect(rectangle, position))
                {
                    return MouseMode.Attribute;
                }
            }
        }

        rectangle = new Rectangle((int)(Position.X - Width / 2), (int)(Position.Y - Height / 2), Width, Height);
        MouseMode mouseMode;
        if (CheckRect(rectangle, position))
        {
            mouseMode = MouseMode.Main;
        }
        else
        {
            mouseMode = MouseMode.None;
        }
        return mouseMode;
    }

    [MonoModReplace]
    public void OnCreateMove()
    {
        if (Data.HasNode)
        {
            Vector2 node;
            if (Position.X != 160f)
            {
                node = EnforceBounds(new Vector2(160f + (160f - Position.X), Position.Y));
            }
            else
            {
                node = EnforceBounds(new Vector2(160f, 120f + (120f - Position.Y)));
            }

            if (Nodes.Count == 0)
            {
                Nodes.Add(node);
            }
            else 
            {
                Nodes[0] = node;
            }
        }
    }

    [MonoModIgnore]
    private extern bool CheckRect(Rectangle rect, Vector2 position);

    public enum MouseMode
    {
        None,
        Main,
        Resize,
        Node,
        Attribute,
        NodeExtended,
    }

    public extern void orig_Update(ActorsLayerUI ui);

    public void Update(ActorsLayerUI ui)
    {
        orig_Update(ui);

        if (Data.Validate is { } valid)
        {
            Invalid = !valid(this, Position);
        }
    }
}
