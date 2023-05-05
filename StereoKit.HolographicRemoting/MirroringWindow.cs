using SDL2;
using System;
using System.Drawing;

namespace StereoKit.HolographicRemoting
{
    public class MirroringWindow
    {

        public IntPtr window;
        public Size ClientSize
        {
            get
            {
                int w, h;
                SDL.SDL_GetWindowSize(window, out w, out h) ;
                return new Size(w, h);
            }
        }
        public MirroringWindow()
        {
            window = SDL.SDL_CreateWindow("Window", 30, 30, 1280, 720, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            SDL.SDL_ShowWindow(window);
        }
        public void Update() 
        { 
        SDL.SDL_UpdateWindowSurface(window);
        }

        public Rectangle Bounds { get 
            {
                int l, t, r, b;
                SDL.SDL_GetWindowBordersSize(window, out t, out l, out b, out r);
                return Rectangle.FromLTRB(l, t, r, b);
            } }

    }
}