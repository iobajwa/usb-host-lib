using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

namespace USBHostLib
{
    public interface IHIDFinder
    {
        IHIDDevice FindDevice(UInt32 VendorID, UInt32 ProductID);
    }

    public class HIDFinder : IHIDFinder
    {
        DeviceManagement _deviceManagement;
        
        public HIDFinder()
        {
            _deviceManagement = new DeviceManagement();
        }

        public IHIDDevice FindDevice(UInt32 vendorID, UInt32 productID)
        {
            Boolean anyHIDDeviceFound = false;
            String[] devicePathNames = new String[128];
            Guid hidGuid = Guid.Empty;
            Int32 memberIndex = 0;
            Boolean success = false;
            bool myDeviceDetected;
            SafeFileHandle deviceHandle = null;
            HID hidHandle = new HID();
            string devicePathName = string.Empty;
            FileStream fileStreamForReadingDeviceData = null;
            HIDDevice deviceFound = null;

            try
            {
                myDeviceDetected = false;
                //CloseCommunications();

                //  Get the device's Vendor ID and Product ID from the form's text boxes.
                //  ***
                //  API function: 'HidD_GetHidGuid
                //  Purpose: Retrieves the interface class GUID for the HIDHandle class.
                //  Accepts: 'A System.Guid object for storing the GUID.
                //  ***

                HID.HidD_GetHidGuid(ref hidGuid);


                //  Fill an array with the device path names of all attached HIDs.
                anyHIDDeviceFound = _deviceManagement.FindDeviceFromGuid(hidGuid, ref devicePathNames);

                //  If there is at least one HIDHandle, attempt to read the Vendor ID and Product ID
                //  of each device until there is a match or all devices have been examined.

                if (anyHIDDeviceFound)
                {
                    memberIndex = 0;

                    do
                    {
                        //  ***
                        //  API function:
                        //  CreateFile

                        //  Purpose:
                        //  Retrieves a handle to a device.

                        //  Accepts:
                        //  A device path name returned by SetupDiGetDeviceInterfaceDetail
                        //  The type of access requested (read/write).
                        //  FILE_SHARE attributes to allow other processes to access the device while this handle is open.
                        //  A Security structure or IntPtr.Zero. 
                        //  A creation disposition value. Use OPEN_EXISTING for devices.
                        //  Flags and attributes for files. Not used for devices.
                        //  Handle to a template file. Not used.

                        //  Returns: a handle without read or write access.
                        //  This enables obtaining information about all HIDs, even system
                        //  keyboards and mice. 
                        //  Separate handles are used for reading and writing.
                        //  ***

                        // Open the handle without read/write access to enable getting information about any HIDHandle, even system keyboards and mice.
                        deviceHandle = FileIO.CreateFile(devicePathNames[memberIndex], 0, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                        if (!deviceHandle.IsInvalid)
                        {
                            //  The returned handle is valid, 
                            //  so find out if this is the device we're looking for.

                            //  Set the Size property of DeviceAttributes to the number of bytes in the structure.

                            hidHandle.DeviceAttributes.Size = Marshal.SizeOf(hidHandle.DeviceAttributes);

                            //  ***
                            //  API function:
                            //  HidD_GetAttributes

                            //  Purpose:
                            //  Retrieves a HIDD_ATTRIBUTES structure containing the Vendor ID, 
                            //  Product ID, and Product Version Number for a device.

                            //  Accepts:
                            //  A handle returned by CreateFile.
                            //  A pointer to receive a HIDD_ATTRIBUTES structure.

                            //  Returns:
                            //  True on success, False on failure.
                            //  ***                            

                            success = HID.HidD_GetAttributes(deviceHandle, ref hidHandle.DeviceAttributes);

                            if (success)
                            {
                                //  Find out if the device matches the one we're looking for.
                                if (myDeviceDetected = ((hidHandle.DeviceAttributes.VendorID == vendorID) && (hidHandle.DeviceAttributes.ProductID == productID)))
                                    devicePathName = devicePathNames[memberIndex];     //  Save the DevicePathName for OnDeviceChange().
                                else
                                    deviceHandle.Close();                                  //  It's not a match, so close the handle.
                            }
                            else
                            {
                                //  There was a problem in retrieving the information.
                                myDeviceDetected = false;
                                deviceHandle.Close();
                            }
                        }
                        //  Keep looking until we find the device or there are no devices left to examine.
                        memberIndex++;
                    }
                    while (!((myDeviceDetected || (memberIndex == devicePathNames.Length))));
                }

                if (myDeviceDetected)
                {
                    //  Learn the capabilities of the device.
                    hidHandle.Capabilities = hidHandle.GetDeviceCapabilities(deviceHandle);

                    //  Find out if the device is a system mouse or keyboard.
                    //hidUsage = hidHandle.GetHidUsage(hidHandle.Capabilities);

                    //Close the handle and reopen it with read/write access.
                    deviceHandle.Close();
                    deviceHandle = FileIO.CreateFile(devicePathName, FileIO.GENERIC_READ | FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                    if (deviceHandle.IsInvalid)
                        throw new InvalidOperationException("The device is a system mouse/keyboard. Windows 2000 and Windows XP obtain exclusive access to Input and Output reports for this devices. Applications can access Feature reports only.");
                    else
                    {
                        if (hidHandle.Capabilities.InputReportByteLength > 0)
                        {
                            //  Set the size of the Input report buffer. 
                            byte[] inputReportBuffer = new byte[hidHandle.Capabilities.InputReportByteLength];
                            fileStreamForReadingDeviceData = new FileStream(deviceHandle, FileAccess.Read | FileAccess.Write, inputReportBuffer.Length, false);
                        }

                        //  Flush any waiting reports in the input buffer. (optional)
                        hidHandle.FlushQueue(deviceHandle);
                    }
                    deviceFound = new HIDDevice(2000, productID, vendorID, deviceHandle, devicePathName, fileStreamForReadingDeviceData, new HIDDeviceCapabilities(hidHandle.Capabilities.InputReportByteLength, hidHandle.Capabilities.OutputReportByteLength));
                }

                return deviceFound;
            }
            catch(Exception Ex)
            {
                throw Ex;
            }
        }
    }
}
