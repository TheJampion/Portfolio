// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.RTCAudio
{
	/// <summary>
	/// This struct is used to call <see cref="RTCAudioInterface.SendAudio" />.
	/// </summary>
	public class SendAudioOptions
	{
		/// <summary>
		/// The Product User ID of the user trying to request this operation.
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// The room this event is registered on.
		/// </summary>
		public string RoomName { get; set; }

		/// <summary>
		/// Audio buffer, which must have a duration of 10 ms.
		/// @note The SDK makes a copy of buffer. There is no need to keep the buffer around after calling <see cref="RTCAudioInterface.SendAudio" />
		/// </summary>
		public AudioBuffer Buffer { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct SendAudioOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_RoomName;
		private System.IntPtr m_Buffer;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string RoomName
		{
			set
			{
				Helper.TryMarshalSet(ref m_RoomName, value);
			}
		}

		public AudioBuffer Buffer
		{
			set
			{
				Helper.TryMarshalSet<AudioBufferInternal, AudioBuffer>(ref m_Buffer, value);
			}
		}

		public void Set(SendAudioOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = RTCAudioInterface.SendaudioApiLatest;
				LocalUserId = other.LocalUserId;
				RoomName = other.RoomName;
				Buffer = other.Buffer;
			}
		}

		public void Set(object other)
		{
			Set(other as SendAudioOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_RoomName);
			Helper.TryMarshalDispose(ref m_Buffer);
		}
	}
}