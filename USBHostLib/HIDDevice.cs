using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.Win32.SafeHandles;

namespace USBHostLib
{
    public interface IHIDDevice
    {
        UInt32 PID { get; }
        UInt32 VID { get; }
        int RequestTimeoutPeriod { get; set; }

        bool WriteReportViaInterruptTransfer(byte[] buffer);
        byte[] ReadReportViaInterruptTransfer();
    }

    public class HIDDeviceCapabilities
    {
        public Int16 InputReportByteLength { get; set; }
        public Int16 OutputReportByteLength { get; set; }

        public HIDDeviceCapabilities(Int16 inputReportByteLength, Int16 outputReportByteLength)
        {
            InputReportByteLength = inputReportByteLength;
            OutputReportByteLength = outputReportByteLength;
        }
    }

    public class HIDDevice : IHIDDevice
    {
        public UInt32 PID { get; private set; }
        public UInt32 VID { get; private set; }
        public int RequestTimeoutPeriod { get; set; }

        public string DevicePathName { get; private set; }
        public SafeFileHandle DeviceHandle { get; private set; }

        internal HID HIDHandle { get; private set; }
        public HIDDeviceCapabilities Capabilities { get; private set; }

        FileStream _deviceInputReportsStream;

        internal HIDDevice(int requestTimeoutPeriod, UInt32 PID, UInt32 VID, SafeFileHandle deviceHandle, string devicePathName, FileStream streamForReadingDeviceInputReports, HIDDeviceCapabilities deviceCapabilities)
        {
            this.PID = PID;
            this.VID = VID;
            this.RequestTimeoutPeriod = requestTimeoutPeriod;
            DeviceHandle = deviceHandle;
            DevicePathName = devicePathName;
            _deviceInputReportsStream = streamForReadingDeviceInputReports;
            Capabilities = deviceCapabilities;
        }

        public HIDDevice(HIDDeviceCapabilities deviceCapabilities)
        {
            Capabilities = deviceCapabilities;
        }

        public bool WriteReportViaInterruptTransfer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (Capabilities.OutputReportByteLength == 0)
                throw new InvalidOperationException("Device does not supports Ouput Reports.");

            if (buffer.Length > Capabilities.OutputReportByteLength - 1)
                throw new ArgumentException("Passed buffer length is greater than the maximum Output Report length that can be transferred to the device in a single transaction.");

            byte[] payload = new byte[Capabilities.OutputReportByteLength];

            payload[0] = 0;         //  Store the report ID in the first byte of the buffer
            CopyBufferContentsToPayload(buffer, payload);

            bool success = false;
            if (_deviceInputReportsStream.CanWrite)
            {
                _deviceInputReportsStream.Write(payload, 0, payload.Length);
                success = true;
            }

            return success;
        }

        
        public byte[] ReadReportViaInterruptTransfer()
        {
            _inputBuffer = new byte[Capabilities.InputReportByteLength];
            _readSucess = false;
            if (_deviceInputReportsStream.CanRead)
            {
                //_deviceInputReportsStream.ReadTimeout = RequestTimeoutPeriod;
                _deviceInputReportsStream.BeginRead(_inputBuffer, 0, _inputBuffer.Length, new AsyncCallback(GetInputReportData), _inputBuffer);
            }
            DateTime startTime = DateTime.Now;
            TimeSpan timeOutPeriod = TimeSpan.FromMilliseconds(RequestTimeoutPeriod);
            
            while (!_readSucess && (DateTime.Now - startTime) < timeOutPeriod)
                System.Threading.Thread.Sleep(1);

            if (!_readSucess)
                throw new TimeoutException("Read request timedout.");

            return _inputBuffer;
        }


        #region WriteReportViaInterruptTransfer Helpers
        private static void CopyBufferContentsToPayload(byte[] buffer, byte[] payload)
        {
            for (int i = 0; i < buffer.Length; i++)
                payload[i + 1] = buffer[i];
        } 
        #endregion

        #region ReadReportViaInterruptTransfer Helpers
        byte[] _inputBuffer;
        bool _readSucess;
        private void GetInputReportData(IAsyncResult result)
        {
            _inputBuffer = (byte[])result.AsyncState;
            _deviceInputReportsStream.EndRead(result);
            _readSucess = true;
        }
        #endregion
    }
}
