using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace FortRise;

public class EffectResource 
{
    public Effect Shader => shader;
    public string PassName => passName;
    private Effect shader;
    private EffectPass effectPass;

    private string passName;

    internal EffectResource() 
    {

    }

    internal void Init(Effect shader, string passName) 
    {
        this.shader = shader;
        this.passName = passName;
        this.effectPass = shader.Techniques[0].Passes[passName];
    }

    /// <summary>
    /// Swap the current pass from an <see cref="Microsoft.Xna.Framework.Graphics.Effect"/> .
    /// </summary>
    /// <param name="passName">A name of the pass</param>
    public void SwapPass(string passName) 
    {
        this.passName = passName;
        effectPass = shader.Techniques[0].Passes[passName];
    }

    /// <summary>
    /// Apply the <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>. Recommend to use this inside of an immediate <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch"/>.
    /// </summary>
    public virtual void Apply() 
    {
        effectPass.Apply();
    }
}

public class MiscEffectResource : EffectResource
{
    public Vector4 uColor = Vector4.One;
    public Texture2D uTexture0;
    public Texture2D uTexture1;
    public Texture2D uTexture2;
    
    /// <summary>
    /// Apply the <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>. Recommend to use this inside of an immediate <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch"/>.
    /// </summary>
    public override void Apply()
    {
        Shader.Parameters["uColor"].SetValue(uColor);
        Shader.Parameters["uDeltaTime"].SetValue(Engine.TimeMult);
        if (uTexture0 != null) 
        {
            Engine.Instance.GraphicsDevice.Textures[0] = uTexture0;
            Engine.Instance.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Shader.Parameters["uTextureSize0"].SetValue(
                new Vector2(uTexture0.Width, uTexture0.Height)
            );
        }
        if (uTexture1 != null) 
        {
            Engine.Instance.GraphicsDevice.Textures[1] = uTexture1;
            Engine.Instance.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            Shader.Parameters["uTextureSize1"].SetValue(
                new Vector2(uTexture1.Width, uTexture1.Height)
            );
        }
        if (uTexture2 != null) 
        {
            Engine.Instance.GraphicsDevice.Textures[2] = uTexture2;
            Engine.Instance.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
            Shader.Parameters["uTextureSize2"].SetValue(
                new Vector2(uTexture2.Width, uTexture2.Height)
            );
        }
        base.Apply();
    }

    /// <summary>
    /// Override the texture slot 0 from the <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>. Note that this texture could be a result of a rendered target from a 
    /// <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch"/>  
    /// </summary>
    /// <param name="texture">A <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> to use</param>
    /// <returns>This instance</returns>
    public MiscEffectResource UseTexture0(Texture2D texture) 
    {
        uTexture0 = texture;
        return this;
    }

    /// <summary>
    /// Override the texture slot 0 from the <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>. Note that this texture could be used by TowerFall's
    /// lighting layer. 
    /// </summary>
    /// <param name="texture">A <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> to use</param>
    /// <returns>This instance</returns>
    public MiscEffectResource UseTexture1(Texture2D texture) 
    {
        uTexture1 = texture;
        return this;
    }

    /// <summary>
    /// Override the texture slot 0 from the <see cref="Microsoft.Xna.Framework.Graphics.Effect"/>.  
    /// </summary>
    /// <param name="texture">A <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> to use</param>
    /// <returns>This instance</returns>
    public MiscEffectResource UseTexture2(Texture2D texture) 
    {
        uTexture2 = texture;
        return this;
    }
}