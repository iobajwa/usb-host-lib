using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32.SafeHandles; 

namespace USBHostLib
{
    internal partial class HID
    {
        //  Used in error messages.

        internal HIDP_CAPS Capabilities;
        internal HIDD_ATTRIBUTES DeviceAttributes;


        ///  <summary>Remove any Input reports waiting in the buffer.</summary>
        ///  <param name="DeviceHandle"> a handle to a device.</param>
        ///  <returns>True on success, False on failure.</returns>
        internal Boolean FlushQueue(SafeFileHandle hidHandle)
        {
            //  ***
            //  API function: HidD_FlushQueue
            //  Purpose: Removes any Input reports waiting in the buffer.
            //  Accepts: a handle to the device.
            //  Returns: True on success, False on failure.
            //  ***
            return HidD_FlushQueue(hidHandle);
        }

        ///  <summary>Retrieves a structure with information about a device's capabilities.</summary>
        ///  <param name="DeviceHandle"> a handle to a device. </param>
        ///  <returns>An HIDP_CAPS structure.</returns>
        internal HIDP_CAPS GetDeviceCapabilities(SafeFileHandle hidHandle)
        {
            IntPtr preparsedData = new System.IntPtr();
            Int32 result = 0;
            Boolean success = false;

            try
            {
                //  ***
                //  API function: HidD_GetPreparsedData
                //  Purpose: retrieves a pointer to a buffer containing information about the device's capabilities.
                //  HidP_GetCaps and other API functions require a pointer to the buffer.
                //  Requires: 
                //  A handle returned by CreateFile.
                //  A pointer to a buffer.
                //  Returns:
                //  True on success, False on failure.
                //  ***
                success = HidD_GetPreparsedData(hidHandle, ref preparsedData);

                //  ***
                //  API function: HidP_GetCaps
                //  Purpose: find out a device's capabilities.
                //  For standard devices such as joysticks, you can find out the specific
                //  capabilities of the device.
                //  For a custom device where the software knows what the device is capable of,
                //  this call may be unneeded.
                //  Accepts:
                //  A pointer returned by HidD_GetPreparsedData
                //  A pointer to a HIDP_CAPS structure.
                //  Returns: True on success, False on failure.
                //  ***
                result = HidP_GetCaps(preparsedData, ref Capabilities);

                if ((result != 0))
                {
                    //  ***
                    //  API function: HidP_GetValueCaps
                    //  Purpose: retrieves a buffer containing an array of HidP_ValueCaps structures.
                    //  Each structure defines the capabilities of one value.
                    //  This application doesn't use this data.
                    //  Accepts:
                    //  A report type enumerator from hidpi.h,
                    //  A pointer to a buffer for the returned array,
                    //  The NumberInputValueCaps member of the device's HidP_Caps structure,
                    //  A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.
                    //  Returns: True on success, False on failure.
                    //  ***                    
                    Int32 vcSize = Capabilities.NumberInputValueCaps;
                    Byte[] valueCaps = new Byte[vcSize];

                    result = HidP_GetValueCaps(HidP_Input, valueCaps, ref vcSize, preparsedData);
                    // (To use this data, copy the ValueCaps byte array into an array of structures.)                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //  ***
                //  API function: HidD_FreePreparsedData
                //  Purpose: frees the buffer reserved by HidD_GetPreparsedData.
                //  Accepts: A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.
                //  Returns: True on success, False on failure.
                //  ***
                if (preparsedData != IntPtr.Zero)
                    success = HidD_FreePreparsedData(preparsedData);
            }
            return Capabilities;
        }
    }
}
