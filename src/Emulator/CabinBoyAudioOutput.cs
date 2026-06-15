using System;
using System.Runtime.InteropServices;

using UnityGB;

using CabinBoy.Config;
using CabinBoy.Core;

namespace CabinBoy.Emulator;

public class CabinBoyAudioOutput : IAudioOutput
{
    private const int SampleRate = 44100;
    private const int Channels   = 2;

    private static readonly int SamplesPerFrame =
        (int)(SampleRate / GameBoyTiming.TargetFramesPerSecond);

    private const int NumBuffers      = 8;
    private const int FramesPerBuffer = 1024;
    private const int BytesPerBuffer  = FramesPerBuffer * Channels * 2;

    private readonly float[] _ring = new float[SampleRate * Channels];
    private int _readPos;
    private int _writePos;
    private int _count;
    private readonly object _lock = new object();

    private IntPtr   _hwo = IntPtr.Zero;
    private IntPtr[] _dataPtr;
    private IntPtr[] _hdrPtr;
    private bool[]   _queued;
    private bool     _open;

    private readonly short[] _scratch = new short[FramesPerBuffer * Channels];

    public CabinBoyAudioOutput()
    {
        try
        {
            OpenDevice();
        }
        catch (Exception ex)
        {
            ModLogger.Error("Audio setup failed: " + ex);
        }
    }

    public int GetOutputSampleRate() => SampleRate;

    public int GetSamplesAvailable() => SamplesPerFrame;

    public void Play(byte[] data)
    {
        if (data == null)
            return;

        lock (_lock)
        {
            for (int i = 0; i < data.Length; i++)
            {
                float sample = ((sbyte)data[i]) / 128f;

                if (_count >= _ring.Length)
                {
                    _readPos = (_readPos + 1) % _ring.Length;
                    _count--;
                }

                _ring[_writePos] = sample;
                _writePos = (_writePos + 1) % _ring.Length;
                _count++;
            }
        }
    }

    public void Pump()
    {
        if (!_open)
            return;

        int need = FramesPerBuffer * Channels;

        float vol = CabinBoyConfigManager.Config.MasterVolume;
        if (vol < 0f) vol = 0f;
        else if (vol > 1f) vol = 1f;

        for (int b = 0; b < NumBuffers; b++)
        {
            if (_queued[b])
            {
                WAVEHDR hdr = Marshal.PtrToStructure<WAVEHDR>(_hdrPtr[b]);
                if ((hdr.dwFlags & WHDR_DONE) == 0)
                    continue;
                _queued[b] = false;
            }

            lock (_lock)
            {
                if (_count < need)
                    continue;

                for (int i = 0; i < need; i++)
                {
                    float f = _ring[_readPos];
                    _readPos = (_readPos + 1) % _ring.Length;
                    _count--;

                    int v = (int)(f * vol * 32767f);
                    if (v > 32767) v = 32767;
                    else if (v < -32768) v = -32768;
                    _scratch[i] = (short)v;
                }
            }

            Marshal.Copy(_scratch, 0, _dataPtr[b], need);

            int wr = waveOutWrite(_hwo, _hdrPtr[b], (uint)Marshal.SizeOf<WAVEHDR>());
            if (wr == 0)
                _queued[b] = true;
        }
    }

    private void OpenDevice()
    {
        WAVEFORMATEX fmt = new WAVEFORMATEX
        {
            wFormatTag      = WAVE_FORMAT_PCM,
            nChannels       = Channels,
            nSamplesPerSec  = SampleRate,
            wBitsPerSample  = 16,
            nBlockAlign     = (ushort)(Channels * 2),
            nAvgBytesPerSec = (uint)(SampleRate * Channels * 2),
            cbSize          = 0
        };

        int res = waveOutOpen(out _hwo, WAVE_MAPPER, ref fmt, IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL);

        if (res != 0 || _hwo == IntPtr.Zero)
        {
            ModLogger.Error("waveOutOpen failed, code=" + res);
            return;
        }

        _dataPtr = new IntPtr[NumBuffers];
        _hdrPtr  = new IntPtr[NumBuffers];
        _queued  = new bool[NumBuffers];

        int hdrSize = Marshal.SizeOf<WAVEHDR>();

        for (int b = 0; b < NumBuffers; b++)
        {
            _dataPtr[b] = Marshal.AllocHGlobal(BytesPerBuffer);

            for (int i = 0; i < BytesPerBuffer; i++)
                Marshal.WriteByte(_dataPtr[b], i, 0);

            WAVEHDR hdr = new WAVEHDR
            {
                lpData         = _dataPtr[b],
                dwBufferLength = BytesPerBuffer,
                dwFlags        = 0
            };

            _hdrPtr[b] = Marshal.AllocHGlobal(hdrSize);
            Marshal.StructureToPtr(hdr, _hdrPtr[b], false);

            waveOutPrepareHeader(_hwo, _hdrPtr[b], (uint)hdrSize);
        }

        _open = true;

        ModLogger.Msg("CabinBoy audio device opened (WinMM, " +
                      SampleRate + "Hz, " + NumBuffers + " buffers).");
    }

    public void Dispose()
    {
        if (!_open && _hwo == IntPtr.Zero)
            return;

        try
        {
            if (_hwo != IntPtr.Zero)
                waveOutReset(_hwo);

            if (_hdrPtr != null)
            {
                uint hdrSize = (uint)Marshal.SizeOf<WAVEHDR>();

                for (int b = 0; b < NumBuffers; b++)
                {
                    if (_hdrPtr[b] != IntPtr.Zero)
                    {
                        waveOutUnprepareHeader(_hwo, _hdrPtr[b], hdrSize);
                        Marshal.FreeHGlobal(_hdrPtr[b]);
                        _hdrPtr[b] = IntPtr.Zero;
                    }

                    if (_dataPtr[b] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_dataPtr[b]);
                        _dataPtr[b] = IntPtr.Zero;
                    }
                }
            }

            if (_hwo != IntPtr.Zero)
                waveOutClose(_hwo);

            ModLogger.Msg("CabinBoy audio device closed.");
        }
        catch (Exception ex)
        {
            ModLogger.Warning("Audio dispose failed: " + ex.Message);
        }

        _hwo  = IntPtr.Zero;
        _open = false;
    }

    private const uint WAVE_MAPPER     = 0xFFFFFFFF;
    private const uint CALLBACK_NULL   = 0;
    private const uint WHDR_DONE       = 0x00000001;
    private const ushort WAVE_FORMAT_PCM = 1;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint   nSamplesPerSec;
        public uint   nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WAVEHDR
    {
        public IntPtr lpData;
        public uint   dwBufferLength;
        public uint   dwBytesRecorded;
        public IntPtr dwUser;
        public uint   dwFlags;
        public uint   dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }

    [DllImport("winmm.dll")]
    private static extern int waveOutOpen(out IntPtr hWaveOut, uint uDeviceID,
        ref WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

    [DllImport("winmm.dll")]
    private static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveOutHdr, uint uSize);

    [DllImport("winmm.dll")]
    private static extern int waveOutReset(IntPtr hWaveOut);

    [DllImport("winmm.dll")]
    private static extern int waveOutClose(IntPtr hWaveOut);
}
