//
// IPhoneDeviceCapabilities.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2016 Microsoft Corp. (www.microsoft.com)
//

using System;

namespace Xamarin.MacDev {
	// https://developer.apple.com/library/content/documentation/DeviceInformation/Reference/iOSDeviceCompatibility/DeviceCompatibilityMatrix/DeviceCompatibilityMatrix.html#//apple_ref/doc/uid/TP40013599-CH17-SW1

	[Flags]
	public enum IPhoneDeviceCapabilities {
		None = 0,
		Accelerometer = 1 << 0,
		ARMv6 = 1 << 1,
		ARMv7 = 1 << 2,
		ARM64 = 1 << 3,
		AutoFocusCamera = 1 << 4,
		BluetoothLE = 1 << 5,
		CameraFlash = 1 << 6,
		FrontFacingCamera = 1 << 7,
		GameKit = 1 << 8,
		GPS = 1 << 9,
		Gyroscope = 1 << 10,
		HealthKit = 1 << 11,
		LocationServices = 1 << 12,
		Magnetometer = 1 << 13,
		Metal = 1 << 14,
		Microphone = 1 << 15,
		OpenGLES1 = 1 << 16,
		OpenGLES2 = 1 << 17,
		OpenGLES3 = 1 << 18,
		PeerPeer = 1 << 19,
		SMS = 1 << 20,
		StillCamera = 1 << 21,
		Telephony = 1 << 22,
		VideoCamera = 1 << 23,
		WatchCompanion = 1 << 24,
		WiFi = 1 << 25
	}

	public static class IPhoneDeviceCapabilitiesExtensions {
		public static IPhoneDeviceCapabilities ToDeviceCapability (this string value)
		{
			switch (value) {
			case "accelerometer": return IPhoneDeviceCapabilities.Accelerometer;
			case "armv6": return IPhoneDeviceCapabilities.ARMv6;
			case "armv7": return IPhoneDeviceCapabilities.ARMv7;
			case "arm64": return IPhoneDeviceCapabilities.ARM64;
			case "auto-focus-camera": return IPhoneDeviceCapabilities.AutoFocusCamera;
			case "bluetooth-le": return IPhoneDeviceCapabilities.BluetoothLE;
			case "camera-flash": return IPhoneDeviceCapabilities.CameraFlash;
			case "front-facing-camera": return IPhoneDeviceCapabilities.FrontFacingCamera;
			case "gamekit": return IPhoneDeviceCapabilities.GameKit;
			case "gps": return IPhoneDeviceCapabilities.GPS;
			case "gyroscope": return IPhoneDeviceCapabilities.Gyroscope;
			case "healthkit": return IPhoneDeviceCapabilities.HealthKit;
			case "location-services": return IPhoneDeviceCapabilities.LocationServices;
			case "magnetometer": return IPhoneDeviceCapabilities.Magnetometer;
			case "metal": return IPhoneDeviceCapabilities.Metal;
			case "microphone": return IPhoneDeviceCapabilities.Microphone;
			case "opengles-1": return IPhoneDeviceCapabilities.OpenGLES1;
			case "opengles-2": return IPhoneDeviceCapabilities.OpenGLES2;
			case "opengles-3": return IPhoneDeviceCapabilities.OpenGLES3;
			case "peer-peer": return IPhoneDeviceCapabilities.PeerPeer;
			case "sms": return IPhoneDeviceCapabilities.SMS;
			case "still-camera": return IPhoneDeviceCapabilities.StillCamera;
			case "telephony": return IPhoneDeviceCapabilities.Telephony;
			case "video-camera": return IPhoneDeviceCapabilities.VideoCamera;
			case "wifi": return IPhoneDeviceCapabilities.WiFi;
			default: return IPhoneDeviceCapabilities.None;
			}
		}
	}
}
