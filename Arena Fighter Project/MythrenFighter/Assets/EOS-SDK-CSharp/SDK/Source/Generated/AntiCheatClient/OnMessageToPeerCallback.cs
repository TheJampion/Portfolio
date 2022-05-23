// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.AntiCheatClient
{
	/// <summary>
	/// Callback issued when a new message must be dispatched to a connected peer.
	/// 
	/// Messages contain opaque binary data of up to 256 bytes and must be transmitted
	/// to the correct peer using the game's own networking layer, then delivered
	/// to the client anti-cheat instance using the <see cref="AntiCheatClientInterface.ReceiveMessageFromPeer" /> function.
	/// 
	/// This callback is always issued from within <see cref="Platform.PlatformInterface.Tick" /> on its calling thread.
	/// </summary>
	public delegate void OnMessageToPeerCallback(AntiCheatCommon.OnMessageToClientCallbackInfo data);

	[System.Runtime.InteropServices.UnmanagedFunctionPointer(Config.LibraryCallingConvention)]
	internal delegate void OnMessageToPeerCallbackInternal(System.IntPtr data);
}