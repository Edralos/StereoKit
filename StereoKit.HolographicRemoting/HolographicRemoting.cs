using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StereoKit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace StereoKit.HolographicRemoting
{
	public class HolographicRemoting : IStepper
	{
		static readonly string remotingExtName = "XR_MSFT_holographic_remoting";
		static readonly string mirroringExtName = "XR_MSFT_holographic_remoting_frame_mirroring";

		private string _ipAddress;
		private ushort _port;
		private bool _sendAudio;
		private int _maxBitrate;
		private bool _wideAudioCapture;
		private bool _frameMirroringEnabled = false;
		private bool _mirrorFrames = false; 

		XrRemotingFrameMirrorImageInfoMSFT mirrorFrameInfo;

		Tex texture;
		Image<Argb32> savedImage;

		Application _mirroringWindow;

		public IntPtr hwnd;
		public string windowTitle = "window";


		public bool FrameMirroringEnabled
		{
			get => _frameMirroringEnabled;
			set
			{
				_frameMirroringEnabled = Backend.OpenXR.ExtEnabled(mirroringExtName) ? value : false;
			}
		}

		public bool MirrorFrames { get => _mirrorFrames;
			set
			{
				_mirrorFrames = value && _frameMirroringEnabled;
				//if(_mirrorFrames && Enabled)
					//_mirroringWindow = new MirroringWindow(texture);
				//else if (_mirroringWindow != null)
				//	if (!_mirroringWindow.Closed)
				//		_mirroringWindow.CloseWindow();

			}
		}

		public bool Enabled { get; private set; } = false;

		public HolographicRemoting(string ipAddress, ushort port = 8265, bool sendAudio = true, int maxBitrateKbps = 20000, bool wideAudioCapture = true)
		{
			_ipAddress = ipAddress;
			_port = port;
			_sendAudio = sendAudio;
			_maxBitrate = maxBitrateKbps;
			_wideAudioCapture = wideAudioCapture;

			if (SK.IsInitialized)
				Log.Err("HolographicRemoting must be constructed before StereoKit is initialized!");

			Backend.OpenXR.RequestExt(remotingExtName);
			Backend.OpenXR.RequestExt(mirroringExtName);
			Backend.OpenXR.OnPreCreateSession += OnPreCreateSession;

			// Set up the OpenXR manifest for the remoting runtime!
			string runtimePath = Path.GetDirectoryName(typeof(HolographicRemoting).Assembly.Location);
			runtimePath = Path.Combine(runtimePath, "RemotingXR.json");
			Environment.SetEnvironmentVariable("XR_RUNTIME_JSON", runtimePath);

		}

		public bool Initialize()
		{
			texture = new Tex(TexType.Rendertarget, TexFormat.Rgba32);
			texture.SetSize(1440, 936);
			texture.AddZBuffer(TexFormat.Depth32);
			savedImage = new Image<Argb32>(texture.Width, texture.Height);
			IntPtr nativeTex = IntPtr.Zero;
            if (MirrorFrames)
            {
                Tex other = Tex.FromFile("IMG.jpg");
                _mirroringWindow = new Application(hwnd);
                nativeTex = _mirroringWindow.Initialize(Backend.D3D11.D3DDevice, Backend.D3D11.D3DContext, texture.GetNativeSurface());
                //_mirroringWindow.Run();
            }

            //IntPtr nativeTex = texture.GetNativeSurface();

			XrRemotingFrameMirrorImageD3D11MSFT mirrorImgD3D11 = new XrRemotingFrameMirrorImageD3D11MSFT();
			mirrorImgD3D11.type = XrStructureType.REMOTING_FRAME_MIRROR_IMAGE_D3D11_MSFT;
			mirrorImgD3D11.texture = nativeTex;

			int size = Marshal.SizeOf(typeof(XrRemotingFrameMirrorImageD3D11MSFT));
			IntPtr memory = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(mirrorImgD3D11, memory, false);

			mirrorFrameInfo = new XrRemotingFrameMirrorImageInfoMSFT();
			mirrorFrameInfo.type = XrStructureType.REMOTING_FRAME_MIRROR_IMAGE_INFO_MSFT;
			mirrorFrameInfo.image = memory;

			
            Enabled = true;
			return Enabled;
		}
		public void Shutdown() { }

		int stepCount = 0;
		int imgCount = 0;
		public void Step()
		{
			var ctxt = Backend.D3D11.D3DContext;
			if (_frameMirroringEnabled && _mirrorFrames)
			{
				
				Backend.OpenXR.AddEndFrameChain(mirrorFrameInfo);

				_mirroringWindow.Tick();

				//_mirroringWindow.Render();
				
				if (stepCount > 15)
				{
					stepCount = 0;

					var colors = texture.GetColors().ToList();
						for (int y = 0; y < texture.Height; y++)
						{
							for (int x = 0; x < texture.Width; x++)
							{
								int i = x + y  * texture.Width;
								savedImage[x, y] = new Argb32(colors[i].a, colors[i].r, colors[i].g, colors[i].a);
							}
						}
						_ = savedImage.SaveAsJpegAsync($"MirrorImgs/TESTMIR_{imgCount}.jpg");
					
					
					imgCount++;
				}
				stepCount++;
				
			}
		}

		void OnPreCreateSession()
		{
			
			if (!Backend.OpenXR.ExtEnabled(remotingExtName)) return;

			NativeAPI.LoadFunctions();

			Log.Info($"Connecting to Holographic Remoting Player at {_ipAddress} : {_port} ...");

			XrRemotingRemoteContextPropertiesMSFT contextProperties = new XrRemotingRemoteContextPropertiesMSFT();
			contextProperties.type = XrStructureType.REMOTING_REMOTE_CONTEXT_PROPERTIES_MSFT;
			contextProperties.enableAudio = _sendAudio ? 1 : 0;
			contextProperties.maxBitrateKbps = (uint)_maxBitrate;
			contextProperties.videoCodec = XrRemotingVideoCodecMSFT.H265;
			contextProperties.depthBufferStreamResolution = XrRemotingDepthBufferStreamResolutionMSFT.HALF;

			IntPtr audioCaptureSettingsPtr = IntPtr.Zero;
			if (_sendAudio && !_wideAudioCapture)
			{
				XrRemotingAudioOutputCaptureSettingsMSFT audioCaptureSettings = new XrRemotingAudioOutputCaptureSettingsMSFT();
				audioCaptureSettings.type = XrStructureType.REMOTING_AUDIO_OUTPUT_CAPTURE_SETTINGS_MSFT;
				audioCaptureSettings.audioOutputCaptureMode = XrRemotingAudioOutputCaptureModeMSFT.AUDIO_OUTPUT_CAPTURE_MODE_APP_ONLY_MSFT;

				int size = Marshal.SizeOf(typeof(XrRemotingAudioOutputCaptureSettingsMSFT));
				audioCaptureSettingsPtr = Marshal.AllocHGlobal(size);
				Marshal.StructureToPtr(audioCaptureSettings, audioCaptureSettingsPtr, false);

				contextProperties.next = audioCaptureSettingsPtr;
			}

			if (NativeAPI.xrRemotingSetContextPropertiesMSFT(Backend.OpenXR.Instance, Backend.OpenXR.SystemId, contextProperties) != XrResult.Success)
			{
				Log.Warn("xrRemotingSetContextPropertiesMSFT failed!");
			}

			Marshal.FreeHGlobal(audioCaptureSettingsPtr);

			XrRemotingConnectInfoMSFT connectInfo = new XrRemotingConnectInfoMSFT();
			connectInfo.type = XrStructureType.REMOTING_CONNECT_INFO_MSFT;
			connectInfo.remoteHostName = Marshal.StringToHGlobalAnsi(_ipAddress);
			connectInfo.remotePort = _port;
			connectInfo.secureConnection = 0;
			XrResult result = NativeAPI.xrRemotingConnectMSFT(Backend.OpenXR.Instance, Backend.OpenXR.SystemId, connectInfo);
			if (result != XrResult.Success)
			{
				Log.Warn("xrRemotingConnectMSFT failed! " + result);
			}
			Marshal.FreeHGlobal(connectInfo.remoteHostName);
		}
	}
}
