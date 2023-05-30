using System;
using System.Collections.Generic;
using System.Text;

namespace StereoKit.HolographicRemoting.D3D11
{
	internal class ID3D11RenderTargetView
	{
		internal IntPtr _nativePointer;

        public ID3D11RenderTargetView(IntPtr nativePointer)
        {
            _nativePointer = nativePointer;
        }
    }
}
