#region License
/* XNAFileDialog - Portable File Dialog for XNA Games
 *
 * Copyright (c) 2015 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region Using Statements
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod;
using MonoMod.Utils;
#endregion

[MonoModIfFlag("OS:Windows")]
public static class XNAFileDialog
{
	#region Public API

	public static GraphicsDevice GraphicsDevice;

	public static string Path;

	public static string StartDirectory;

	public static bool ShowDialogSynchronous(string title = null, string saveFile = null)
	{
		Path = null;

		// Create dialog resources
		XNAFileDialog_Init(
			CreateTextureDelegate,
			BufferDataDelegate,
			DrawPrimitivesDelegate,
			ReceivePathDelegate,
			StartDirectory,
			saveFile,
			title,
			GraphicsDevice.PresentationParameters.BackBufferWidth,
			GraphicsDevice.PresentationParameters.BackBufferHeight
		);

		// Store previous GL state
		Rectangle prevScissor = GraphicsDevice.ScissorRectangle;
		Texture prevTexture = GraphicsDevice.Textures[0];
		SamplerState prevSampler = GraphicsDevice.SamplerStates[0];
		BlendState prevBlend = GraphicsDevice.BlendState;
		DepthStencilState prevDepthStencil = GraphicsDevice.DepthStencilState;
		RasterizerState prevRasterizer = GraphicsDevice.RasterizerState;
		VertexBufferBinding[] prevVerts = GraphicsDevice.GetVertexBuffers();
		IndexBuffer prevIndex = GraphicsDevice.Indices;

		// Set new GL state
		GraphicsDevice.Textures[0] = texture;
		GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
		GraphicsDevice.BlendState = BlendState.NonPremultiplied;
		GraphicsDevice.DepthStencilState = DepthStencilState.None;
		GraphicsDevice.RasterizerState = RasterizerState.CullNone;

		// Time to block, yayyyyy
		pathSent = false;
		do
		{
			GraphicsDevice.Clear(Color.Black);
			XNAFileDialog_Update();
			XNAFileDialog_Render();
			GraphicsDevice.Present();
		} while (!pathSent);

		// Restore GL state
		GraphicsDevice.ScissorRectangle = prevScissor;
		GraphicsDevice.Textures[0] = prevTexture;
		GraphicsDevice.SamplerStates[0] = prevSampler;
		GraphicsDevice.BlendState = prevBlend;
		GraphicsDevice.DepthStencilState = prevDepthStencil;
		GraphicsDevice.RasterizerState = prevRasterizer;
		GraphicsDevice.SetVertexBuffers(prevVerts);
		GraphicsDevice.Indices = prevIndex;

		// Clean up. We out.
		texture.Dispose();
		texture = null;
		vertBuffer.Dispose();
		vertBuffer = null;
		vertBufferSize = 0;
		indexBuffer.Dispose();
		indexBuffer = null;
		indexBufferSize = 0;
		XNAFileDialog_Shutdown();
		return !String.IsNullOrEmpty(Path);
	}

	#endregion

	#region Texture Management Callback

	private static Texture2D texture;
	private static IntPtr CreateTexture(
		IntPtr bytes,
		int width,
		int height
	) {
		texture = new Texture2D(
			GraphicsDevice,
			width,
			height,
			false,
			SurfaceFormat.Color
		);
		byte[] pixels = new byte[width * height * 4];
		Marshal.Copy(bytes, pixels, 0, pixels.Length);
		texture.SetData(pixels);
		return new IntPtr(texture.GetHashCode());
	}

	#endregion

	#region Buffer Object Management Callback

	private static DynamicVertexBuffer vertBuffer;
	private static DynamicIndexBuffer indexBuffer;
	private static int vertBufferSize = 0;
	private static int indexBufferSize = 0;
	private static VertexDeclaration vertDecl = new VertexDeclaration(
		new VertexElement[]
		{
			new VertexElement(
				0,
				VertexElementFormat.Vector2,
				VertexElementUsage.Position,
				0
			),
			new VertexElement(
				16,
				VertexElementFormat.Color,
				VertexElementUsage.Color,
				0
			),
			new VertexElement(
				8,
				VertexElementFormat.Vector2,
				VertexElementUsage.TextureCoordinate,
				0
			)
		}
	);
	private static void BufferData(
		IntPtr vertexData,
		int vertexDataLen,
		IntPtr indexData,
		int indexDataLen
	) {
		if (vertexDataLen > vertBufferSize)
		{
			vertBufferSize = vertexDataLen;
			if (vertBuffer != null)
			{
				vertBuffer.Dispose();
			}
			vertBuffer = new DynamicVertexBuffer(
				GraphicsDevice,
				vertDecl,
				vertBufferSize / vertDecl.VertexStride,
				BufferUsage.WriteOnly
			);
		}
		if (indexDataLen > indexBufferSize)
		{
			indexBufferSize = indexDataLen;
			if (indexBuffer != null)
			{
				indexBuffer.Dispose();
			}
			indexBuffer = new DynamicIndexBuffer(
				GraphicsDevice,
				IndexElementSize.SixteenBits,
				indexBufferSize / 2,
				BufferUsage.WriteOnly
			);
		}
		byte[] vertices = new byte[vertBufferSize];
		short[] indices = new short[indexBufferSize / 2];
		Marshal.Copy(vertexData, vertices, 0, vertices.Length);
		Marshal.Copy(indexData, indices, 0, indices.Length);
		vertBuffer.SetData(0, vertices, 0, vertices.Length, 1, SetDataOptions.None);
		indexBuffer.SetData(indices, 0, indices.Length, SetDataOptions.None);
	}

	#endregion

	#region Internal Drawing Callback

	private static Rectangle scissor = new Rectangle();
	private static void DrawPrimitives(
		int scissorX,
		int scissorY,
		int scissorWidth,
		int scissorHeight,
		int vertexOffset,
		int vertexCount,
		int indexOffset,
		int elementCount
	) {
		scissor.X = scissorX;
		scissor.Y = scissorX;
		scissor.Width = scissorWidth;
		scissor.Height = scissorHeight;
		GraphicsDevice.ScissorRectangle = scissor;
		GraphicsDevice.SetVertexBuffer(vertBuffer);
		GraphicsDevice.Indices = indexBuffer;
		GraphicsDevice.DrawIndexedPrimitives(
			PrimitiveType.TriangleList,
			vertexOffset,
			0,
			vertexCount,
			indexOffset,
			elementCount / 3
		);
	}

	#endregion

	#region Internal Path Assignment Callback

	private static bool pathSent;
	private static void ReceivePath(IntPtr path)
	{
		// FIXME: UTF8?! -flibit
		Path = Marshal.PtrToStringAnsi(path);
		pathSent = true;
	}

	#endregion

	#region Native Interop

	private const string nativeLibName = "XNAFileDialog.dll";

	private delegate IntPtr XNAFileDialog_CreateTexture(
		IntPtr bytes,
		int width,
		int height
	);
	private static XNAFileDialog_CreateTexture CreateTextureDelegate = CreateTexture;

	private delegate void XNAFileDialog_BufferData(
		IntPtr vertexData,
		int vertexDataLen,
		IntPtr indexData,
		int indexDataLen
	);
	private static XNAFileDialog_BufferData BufferDataDelegate = BufferData;

	private delegate void XNAFileDialog_DrawPrimitives(
		int scissorX,
		int scissorY,
		int scissorWidth,
		int scissorHeight,
		int vertexOffset,
		int vertexCount,
		int indexOffset,
		int elementCount
	);
	private static XNAFileDialog_DrawPrimitives DrawPrimitivesDelegate = DrawPrimitives;

	private delegate void XNAFileDialog_ReceivePath(IntPtr path);
	private static XNAFileDialog_ReceivePath ReceivePathDelegate = ReceivePath;

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void XNAFileDialog_Init(
		XNAFileDialog_CreateTexture createTexture,
		XNAFileDialog_BufferData bufferData,
		XNAFileDialog_DrawPrimitives drawPrimitives,
		XNAFileDialog_ReceivePath receivePath,
		[MarshalAs(UnmanagedType.LPStr)]
			string startDirectory,
		[MarshalAs(UnmanagedType.LPStr)]
			string startFile,
		[MarshalAs(UnmanagedType.LPStr)]
			string windowTitle,
		int width,
		int height
	);

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void XNAFileDialog_Shutdown();

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void XNAFileDialog_Update();

	[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
	private static extern void XNAFileDialog_Render();

	#endregion
}