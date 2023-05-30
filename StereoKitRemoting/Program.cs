using StereoKit;
using StereoKit.HolographicRemoting;
using StereoKit.HolographicRemoting.Window;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;


// Adding the Holographic Remoting IStepper based on command line
// arguments allows us to create separate Launch Profiles for running
// locally vs. running with remoting via Visual Studio's UI! Note the
// "Run - Remote" and "Run - Local" options in the run bar.
const string defaultIp   = "192.168.1.81";
int          remotingArg = Array.IndexOf(args, "-remote");
HolographicRemoting remoting = null;

if (remotingArg != -1)
{
	// If the "-remote" option is followed by text that is not another
	// "-" option, we can treat that as an IP address other than the
	// default we have hard-coded here.
	string ip = (remotingArg+1 < args.Length && args[remotingArg + 1].StartsWith("-") == false)
		? args[remotingArg + 1]
		: defaultIp;
	remoting = new HolographicRemoting(ip, wideAudioCapture: false);
	SK.AddStepper(remoting);
}

Backend.OpenXR.RequestExt("XR_MSFT_scene_understanding");
// Initialize StereoKit
SKSettings settings = new SKSettings {
	appName      = "StereoKitRemoting",
	assetsFolder = "Assets",
};
if (!SK.Initialize(settings))
	Environment.Exit(1);

Log.Info($"scene understanding available : {Backend.OpenXR.ExtEnabled("XR_MSFT_scene_understanding")}");
Backend.OpenXR.RequestExt("XR_MSFT_spatial_anchor");
bool spatial = Backend.OpenXR.ExtEnabled("XR_MSFT_spatial_anchor");

remoting.FrameMirroringEnabled = true;

// Create assets used by the app
Pose  cubePose = new Pose(0,0,-0.5f, Quat.Identity);
Model cube     = Model.FromMesh(
	Mesh.GenerateRoundedCube(Vec3.One*0.1f, 0.02f),
	Default.MaterialUI);

Matrix   floorTransform = Matrix.TS(0, -1.5f, 0, new Vec3(30, 0.1f, 30));
Material floorMaterial  = new Material(Shader.FromFile("floor.hlsl"));
floorMaterial.Transparency = Transparency.Blend;

World.RaycastEnabled= true;
World.OcclusionEnabled = true;
var handle = Process.GetCurrentProcess().MainWindowHandle;
var thing  = Process.GetCurrentProcess().MainWindowTitle;
//NativeWindow nativeWindow = new NativeWindow(handle);
//Rectangle bounds = nativeWindow.Bounds;
//nativeWindow.Bounds = new Rectangle(0, 0, 1280, 720);
//var mir = new MirroringWindow();
remoting.hwnd = handle;	
if (remoting.FrameMirroringEnabled)
	remoting.MirrorFrames = true;
//World.OcclusionMaterial.Wireframe= true;
// Core application loop
SK.Run(() => {
	if (SK.System.displayType == Display.Opaque)
		Default.MeshCube.Draw(floorMaterial, floorTransform);

	UI.Handle("Cube", ref cubePose, cube.Bounds);
	cube.Draw(cubePose.ToMatrix());
});