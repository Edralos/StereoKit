using StereoKit;
using System.Diagnostics;
using System;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using Vortice.Mathematics;
using Veldrid.Sdl2;
using SDL2;

public class VorticeWindow
{
    public string Title { get; private set; }
    public Size ClientSize { get; private set; }

    public VorticeWindow(string title, IntPtr hwnd,  int width = 1280, int height = 720)
    {
        Title = title;
        ClientSize = new Size(width, height);
        _window = new Sdl2Window(title, 10, 10, 1080, 720, SDL_WindowFlags.Resizable, true) ;
        _window.Resizable = true;
    }


    private Sdl2Window _window;

    public bool Exists => _window.Exists;
    public IntPtr Handle => _window.Handle;


    public  Rectangle Bounds
    {
        get
        {
            var windowBounds = _window.Bounds;

            return Rectangle.FromLTRB(windowBounds.Left, windowBounds.Top, windowBounds.Right, windowBounds.Bottom);
        }
    }
}