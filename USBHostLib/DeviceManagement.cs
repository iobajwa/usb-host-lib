///  <summary>
///  Routines for detecting devices and receiving device notifications.
///  </summary>

using System;
using System.Runtime.InteropServices;

namespace USBHostLib
{
    sealed internal partial class DeviceManagement
    {
        ///  <summary>
        ///  Use SetupDi API functions to retrieve the device path name of an
        ///  attached device that belongs to a device interface class.
        ///  </summary>
        ///  
        ///  <param name="myGuid"> an interface class GUID. </param>
        ///  <param name="devicePathNames"> a pointer to the device path name 
        ///  of an attached device. </param>
        ///  
        ///  <returns>
        ///   True if a device is found, False if not. 
        ///  </returns>

        internal Boolean FindDeviceFromGuid(System.Guid myGuid, ref String[] devicePathName)
        {
            Int32 bufferSize = 0;
            IntPtr detailDataBuffer = IntPtr.Zero;
            Boolean deviceFound;
            IntPtr deviceInfoSet = new System.IntPtr();
            Boolean lastDevice = false;
            Int32 memberIndex = 0;
            SP_DEVICE_INTERFACE_DATA MyDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
            Boolean success;

            try
            {
                // ***
                //  API function

                //  summary 
                //  Retrieves a device information set for a specified group of devices.
                //  SetupDiEnumDeviceInterfaces uses the device information set.

                //  parameters 
                //  Interface class GUID.
                //  Null to retrieve information for all device instances.
                //  Optional handle to a top-level window (unused here).
                //  Flags to limit the returned information to currently present devices 
                //  and devices that expose interfaces in the class specified by the GUID.

                //  Returns
                //  Handle to a device information set for the devices.
                // ***

                deviceInfoSet = SetupDiGetClassDevs(ref myGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                deviceFound = false;
                memberIndex = 0;

                // The cbSize element of the MyDeviceInterfaceData structure must be set to
                // the structure's size in bytes. 
                // The size is 28 bytes for 32-bit code and 32 bits for 64-bit code.

                MyDeviceInterfaceData.cbSize = Marshal.SizeOf(MyDeviceInterfaceData);

                do
                {
                    // Begin with 0 and increment through the device information set until
                    // no more devices are available.

                    // ***
                    //  API function

                    //  summary
                    //  Retrieves a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.
                    //  On return, MyDeviceInterfaceData contains the handle to a
                    //  SP_DEVICE_INTERFACE_DATA structure for a detected device.

                    //  parameters
                    //  DeviceInfoSet returned by SetupDiGetClassDevs.
                    //  Optional SP_DEVINFO_DATA structure that defines a device instance 
                    //  that is a member of a device information set.
                    //  Device interface GUID.
                    //  Index to specify a device in a device information set.
                    //  Pointer to a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.

                    //  Returns
                    //  True on success.
                    // ***

                    success = SetupDiEnumDeviceInterfaces
                        (deviceInfoSet,
                        IntPtr.Zero,
                        ref myGuid,
                        memberIndex,
                        ref MyDeviceInterfaceData);

                    // Find out if a device information set was retrieved.

                    if (!success)
                    {
                        lastDevice = true;

                    }
                    else
                    {
                        // A device is present.

                        // ***
                        //  API function: 

                        //  summary:
                        //  Retrieves an SP_DEVICE_INTERFACE_DETAIL_DATA structure
                        //  containing information about a device.
                        //  To retrieve the information, call this function twice.
                        //  The first time returns the size of the structure.
                        //  The second time returns a pointer to the data.

                        //  parameters
                        //  DeviceInfoSet returned by SetupDiGetClassDevs
                        //  SP_DEVICE_INTERFACE_DATA structure returned by SetupDiEnumDeviceInterfaces
                        //  A returned pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA 
                        //  Structure to receive information about the specified interface.
                        //  The size of the SP_DEVICE_INTERFACE_DETAIL_DATA structure.
                        //  Pointer to a variable that will receive the returned required size of the 
                        //  SP_DEVICE_INTERFACE_DETAIL_DATA structure.
                        //  Returned pointer to an SP_DEVINFO_DATA structure to receive information about the device.

                        //  Returns
                        //  True on success.
                        // ***                     

                        success = SetupDiGetDeviceInterfaceDetail
                            (deviceInfoSet,
                            ref MyDeviceInterfaceData,
                            IntPtr.Zero,
                            0,
                            ref bufferSize,
                            IntPtr.Zero);

                        // Allocate memory for the SP_DEVICE_INTERFACE_DETAIL_DATA structure using the returned buffer size.

                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        // Store cbSize in the first bytes of the array. The number of bytes varies with 32- and 64-bit systems.

                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        // Call SetupDiGetDeviceInterfaceDetail again.
                        // This time, pass a pointer to DetailDataBuffer
                        // and the returned required buffer size.

                        success = SetupDiGetDeviceInterfaceDetail
                            (deviceInfoSet,
                            ref MyDeviceInterfaceData,
                            detailDataBuffer,
                            bufferSize,
                            ref bufferSize,
                            IntPtr.Zero);

                        // Skip over cbsize (4 bytes) to get the address of the devicePathNames.

                        IntPtr pDevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

                        // Get the String containing the devicePathNames.

                        devicePathName[memberIndex] = Marshal.PtrToStringAuto(pDevicePathName);

                        if (detailDataBuffer != IntPtr.Zero)
                        {
                            // Free the memory allocated previously by AllocHGlobal.

                            Marshal.FreeHGlobal(detailDataBuffer);
                        }
                        deviceFound = true;
                    }
                    memberIndex = memberIndex + 1;
                }
                while (!((lastDevice == true)));



                return deviceFound;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

                // ***
                //  API function

                //  summary
                //  Frees the memory reserved for the DeviceInfoSet returned by SetupDiGetClassDevs.

                //  parameters
                //  DeviceInfoSet returned by SetupDiGetClassDevs.

                //  returns
                //  True on success.
                // ***

                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }
        }
    }
}
