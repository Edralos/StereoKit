// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System;
using System.Diagnostics;
using StereoKit.HolographicRemoting;
using StereoKit.HolographicRemoting.Window;
using static StereoKit.HolographicRemoting.Window.NativeWindowAPI;

public class Application : IDisposable
{
    public const string WindowClassName = "VorticeWindow";


    protected D3D11GraphicsDevice _graphicsDevice;


    public Application(IntPtr hwnd,  bool headless = false)
    {
        Headless = headless;
        Current = this;

        PlatformConstruct(hwnd);
    }

    public static Application Current { get; private set; }

    public bool Headless { get; }

    //public MirroringWindow MainWindow { get; private set; }
    public NativeWindow MainWindow { get; private set; }

    public virtual void Dispose()
    {
        _graphicsDevice?.Dispose();
    }

    public virtual IntPtr Initialize(IntPtr deviceNative, IntPtr contextNative, IntPtr textureNative)
    {
        _graphicsDevice = new D3D11GraphicsDevice(MainWindow, new System.Drawing.Size(1440,936), deviceNative, contextNative, textureNative, Vortice.DXGI.Format.R8G8B8A8_UNorm_SRgb);
        return _graphicsDevice.BackBufferTexture.NativePointer;

    }

    private bool _exitRequested = false;

    public void Run()
    {
        while (!_exitRequested)
        {
            if (PeekMessage(out NativeMessage msg, default, 0, 0, (uint)PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE) != false)
            {
                _ = TranslateMessage(ref msg);
                _ = DispatchMessage(ref msg);
                if (msg.msg == 18U)
                {
                    _exitRequested = true;
                    break;
                }
            }

            Tick();
        }
    }

    public void Tick()
    {
        if (!_exitRequested)
        {
            if (PeekMessage(out NativeMessage msg, default, 0, 0, (uint)PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE) != false)
            {
                _ = TranslateMessage(ref msg);
                _ = DispatchMessage(ref msg);
                if (msg.msg == 18U)
                {
                    _exitRequested = true;
                    return;
                }
            }

            if (_graphicsDevice != null)
            {
                bool res = _graphicsDevice.DrawFrame(OnDraw);
                // MainWindow.Update();
            }
            else
            {
                OnDraw(MainWindow.Bounds.Width, MainWindow.Bounds.Height);
            }
        }
        
    }


    protected virtual void OnActivated()
    {
    }

    protected virtual void OnDeactivated()
    {
    }

    protected virtual void OnDraw(int width, int height)
    {
        _graphicsDevice.DeviceContext.Flush();
    }


    private unsafe void PlatformConstruct(IntPtr hwnd)
    {
       
        if (!Headless)
        {
            // Create main window.
            MainWindow = new NativeWindow("MIRROR", 1440, 936);
            // MainWindow =new MirroringWindow();
        }
    }

  


    
}
