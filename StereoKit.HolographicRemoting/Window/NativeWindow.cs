using System;
using static StereoKit.HolographicRemoting.Window.NativeWindowAPI;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;

namespace StereoKit.HolographicRemoting.Window
{
	public class NativeWindow
	{
		public string Title { get; private set; }
		public IntPtr HWND { get; private set; }
		public Size ClientSize { get; private set; }

		private WndProc wndProcDelegate;

		public NativeWindow(IntPtr hwnd)
		{
			HWND = hwnd;
		}

		public NativeWindow(string title, int width = 1280, int height = 720)
		{
			RegisterWindow();

			Title = title;
			ClientSize = new Size(width, height);

			WindowStyle style = 0;
			style =  WindowStyle.WS_CAPTION |
				WindowStyle.WS_SYSMENU |
				WindowStyle.WS_MINIMIZEBOX |
				WindowStyle.WS_CLIPSIBLINGS |
				WindowStyle.WS_BORDER |
				WindowStyle.WS_DLGFRAME |
				WindowStyle.WS_THICKFRAME |
				WindowStyle.WS_GROUP |
				WindowStyle.WS_TABSTOP |
				WindowStyle.WS_SIZEBOX;

			int x, y = 0;
			int winWidth, winHeight;


			if (ClientSize.Width > 0 && ClientSize.Height > 0)
			{
				var rect = new RECT
				{
					Right = ClientSize.Width,
					Bottom = ClientSize.Height
				};

				// Adjust according to window styles
				AdjustWindowRectEx( ref rect, (uint)style, false, (uint)WindowStyleEx.WS_EX_APPWINDOW);

				winWidth = rect.Right - rect.Left;
				winHeight = rect.Bottom - rect.Top;

				int screenWidth = GetSystemMetrics(SystemMetric.SM_CXSCREEN);
				int screenHeight = GetSystemMetrics(SystemMetric.SM_CYSCREEN);

				// Place the window in the middle of the screen.WS_EX_APPWINDOW
				x = (screenWidth - winWidth) / 2;
				y = (screenHeight - winHeight) / 2;
			}
			else
			{
				x = y = winWidth = winHeight = unchecked((int)0x80000000);
			}

			var module = GetModuleHandle(null);
			HWND = CreateWindowExW(
				(uint)WindowStyleEx.WS_EX_APPWINDOW,
                "MirrorWin",
				Title,
				(uint)style,
				x,
				y,
				winWidth,
				winHeight,
                IntPtr.Zero,
				IntPtr.Zero,
				module,
				IntPtr.Zero);


			if (HWND == IntPtr.Zero)
			{
				return;
			}

			ShowWindow(HWND, 0x00000001);
			RECT windowRect;
			GetClientRect(HWND,  out windowRect);
			ClientSize = new Size(windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
		}

		public Rectangle Bounds
		{
			get
			{
				GetWindowRect(HWND, out RECT rect);
				return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
			}
			set
			{
				bool res = SetWindowPos(HWND, IntPtr.Zero, value.X, value.Y, value.Width, value.Height, (uint)WindowPoseFlag.SWP_SHOWWINDOW);
			}
		}


		private void RegisterWindow()
		{
			var module = GetModuleHandle(null);
			wndProcDelegate = WndProc;
			WNDCLASSEX winClass = new WNDCLASSEX
			{
				cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
				style = (uint)(WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW |WindowClassStyles.CS_OWNDC),
				lpfnWndProc =  Marshal.GetFunctionPointerForDelegate(wndProcDelegate),
				hInstance = module,
				hCursor = LoadCursor(IntPtr.Zero, (int)IDC_STANDARD_CURSORS.IDC_ARROW),
				hbrBackground = IntPtr.Zero,
				lpszClassName = "MirrorWin",
				hIcon = IntPtr.Zero,
				hIconSm = IntPtr.Zero,

				
			};
			
			ushort atom = RegisterClassExW(ref  winClass);


		}

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
			if (msg == (uint)WindowsMessages.ACTIVATEAPP)
			{
				return DefWindowProcW(hWnd, msg, wParam, lParam);
			}

			switch (msg)
			{
				case (uint)WindowsMessages.KEYDOWN:
				case (uint)WindowsMessages.KEYUP:
				case (uint)WindowsMessages.SYSKEYDOWN:
				case (uint)WindowsMessages.SYSKEYUP:

				break;
				case (uint)WindowsMessages.DESTROY:
					PostQuitMessage(0);
					break;
			}

            return DefWindowProcW(hWnd, msg, wParam, lParam);

        }






    }
}
