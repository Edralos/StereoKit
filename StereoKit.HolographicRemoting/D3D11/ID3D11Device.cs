using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace StereoKit.HolographicRemoting.D3D11
{
	internal class ID3D11Device
	{
		protected IntPtr _nativePointer;

        public ID3D11Device(IntPtr nativePointer)
        {
            _nativePointer = nativePointer;
        }

        [DllImport("d3d11.dll")]
        private static extern uint D3D11GetDevice(IntPtr pResource, IntPtr pDesc, IntPtr ppRTView);

    }
}
