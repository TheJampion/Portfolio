// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.TitleStorage
{
	/// <summary>
	/// Input data for the <see cref="TitleStorageInterface.ReadFile" /> function
	/// </summary>
	public class ReadFileOptions
	{
		/// <summary>
		/// Product User ID of the local user who is reading the requested file (optional)
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// The file name to read; this file must already exist
		/// </summary>
		public string Filename { get; set; }

		/// <summary>
		/// The maximum amount of data in bytes should be available to read in a single <see cref="OnReadFileDataCallback" /> call
		/// </summary>
		public uint ReadChunkLengthBytes { get; set; }

		/// <summary>
		/// Callback function to handle copying read data
		/// </summary>
		public OnReadFileDataCallback ReadFileDataCallback { get; set; }

		/// <summary>
		/// Optional callback function to be informed of download progress, if the file is not already locally cached. If set, this will be called at least once before completion if the request is successfully started
		/// </summary>
		public OnFileTransferProgressCallback FileTransferProgressCallback { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct ReadFileOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_Filename;
		private uint m_ReadChunkLengthBytes;
		private System.IntPtr m_ReadFileDataCallback;
		private System.IntPtr m_FileTransferProgressCallback;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string Filename
		{
			set
			{
				Helper.TryMarshalSet(ref m_Filename, value);
			}
		}

		public uint ReadChunkLengthBytes
		{
			set
			{
				m_ReadChunkLengthBytes = value;
			}
		}

		private static OnReadFileDataCallbackInternal s_ReadFileDataCallback;
		public static OnReadFileDataCallbackInternal ReadFileDataCallback
		{
			get
			{
				if (s_ReadFileDataCallback == null)
				{
					s_ReadFileDataCallback = new OnReadFileDataCallbackInternal(TitleStorageInterface.OnReadFileDataCallbackInternalImplementation);
				}

				return s_ReadFileDataCallback;
			}
		}

		private static OnFileTransferProgressCallbackInternal s_FileTransferProgressCallback;
		public static OnFileTransferProgressCallbackInternal FileTransferProgressCallback
		{
			get
			{
				if (s_FileTransferProgressCallback == null)
				{
					s_FileTransferProgressCallback = new OnFileTransferProgressCallbackInternal(TitleStorageInterface.OnFileTransferProgressCallbackInternalImplementation);
				}

				return s_FileTransferProgressCallback;
			}
		}

		public void Set(ReadFileOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = TitleStorageInterface.ReadfileoptionsApiLatest;
				LocalUserId = other.LocalUserId;
				Filename = other.Filename;
				ReadChunkLengthBytes = other.ReadChunkLengthBytes;
				m_ReadFileDataCallback = other.ReadFileDataCallback != null ? System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(ReadFileDataCallback) : System.IntPtr.Zero;
				m_FileTransferProgressCallback = other.FileTransferProgressCallback != null ? System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(FileTransferProgressCallback) : System.IntPtr.Zero;
			}
		}

		public void Set(object other)
		{
			Set(other as ReadFileOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_LocalUserId);
			Helper.TryMarshalDispose(ref m_Filename);
			Helper.TryMarshalDispose(ref m_ReadFileDataCallback);
			Helper.TryMarshalDispose(ref m_FileTransferProgressCallback);
		}
	}
}