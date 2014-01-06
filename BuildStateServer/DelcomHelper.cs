using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace BuildStateServer
{
    internal sealed partial class DeviceManagement
    {
        ///<summary >
        // API declarations relating to device management (SetupDixxx and 
        // RegisterDeviceNotification functions).   
        /// </summary>

        // from dbt.h

        internal const Int32 DBT_DEVICEARRIVAL = 0X8000;
        internal const Int32 DBT_DEVICEREMOVECOMPLETE = 0X8004;
        internal const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
        internal const Int32 DBT_DEVTYP_HANDLE = 6;
        internal const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        internal const Int32 DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        internal const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        internal const Int32 WM_DEVICECHANGE = 0X219;

        // from setupapi.h

        internal const Int32 DIGCF_PRESENT = 2;
        internal const Int32 DIGCF_DEVICEINTERFACE = 0X10;

        // Two declarations for the DEV_BROADCAST_DEVICEINTERFACE structure.

        // Use this one in the call to RegisterDeviceNotification() and
        // in checking dbch_devicetype in a DEV_BROADCAST_HDR structure:

        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_DEVICEINTERFACE
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            internal Guid dbcc_classguid;
            internal Int16 dbcc_name;
        }

        // Use this to read the dbcc_name String and classguid:

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class DEV_BROADCAST_DEVICEINTERFACE_1
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            internal Byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            internal Char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_HDR
        {
            internal Int32 dbch_size;
            internal Int32 dbch_devicetype;
            internal Int32 dbch_reserved;
        }

        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal Int32 cbSize;
            internal System.Guid InterfaceClassGuid;
            internal Int32 Flags;
            internal IntPtr Reserved;
        }

        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal Int32 cbSize;
            internal String DevicePath;
        }

        internal struct SP_DEVINFO_DATA
        {
            internal Int32 cbSize;
            internal System.Guid ClassGuid;
            internal Int32 DevInst;
            internal Int32 Reserved;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Int32 SetupDiCreateDeviceInfoList(ref System.Guid ClassGuid, Int32 hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern Boolean UnregisterDeviceNotification(IntPtr Handle);
    }

    sealed internal partial class DeviceManagement
    {
        ///  <summary>
        ///  Compares two device path names. Used to find out if the device name 
        ///  of a recently attached or removed device matches the name of a 
        ///  device the application is communicating with.
        ///  </summary>
        ///  
        ///  <param name="m"> a WM_DEVICECHANGE message. A call to RegisterDeviceNotification
        ///  causes WM_DEVICECHANGE messages to be passed to an OnDeviceChange routine.. </param>
        ///  <param name="mydevicePathName"> a device pathname returned by 
        ///  SetupDiGetDeviceInterfaceDetail in an SP_DEVICE_INTERFACE_DETAIL_DATA structure. </param>
        ///  
        ///  <returns>
        ///  True if the names match, False if not.
        ///  </returns>
        ///  
        //internal Boolean DeviceNameMatch(Message m, String mydevicePathName)
        //{
        //    Int32 stringSize;

        //    try
        //    {
        //        DEV_BROADCAST_DEVICEINTERFACE_1 devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE_1();
        //        DEV_BROADCAST_HDR devBroadcastHeader = new DEV_BROADCAST_HDR();

        //        // The LParam parameter of Message is a pointer to a DEV_BROADCAST_HDR structure.

        //        Marshal.PtrToStructure(m.LParam, devBroadcastHeader);

        //        if ((devBroadcastHeader.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE))
        //        {
        //            // The dbch_devicetype parameter indicates that the event applies to a device interface.
        //            // So the structure in LParam is actually a DEV_BROADCAST_INTERFACE structure, 
        //            // which begins with a DEV_BROADCAST_HDR.

        //            // Obtain the number of characters in dbch_name by subtracting the 32 bytes
        //            // in the strucutre that are not part of dbch_name and dividing by 2 because there are 
        //            // 2 bytes per character.

        //            stringSize = System.Convert.ToInt32((devBroadcastHeader.dbch_size - 32) / 2);

        //            // The dbcc_name parameter of devBroadcastDeviceInterface contains the device name. 
        //            // Trim dbcc_name to match the size of the String.         

        //            devBroadcastDeviceInterface.dbcc_name = new Char[stringSize + 1];

        //            // Marshal data from the unmanaged block pointed to by m.LParam 
        //            // to the managed object devBroadcastDeviceInterface.

        //            Marshal.PtrToStructure(m.LParam, devBroadcastDeviceInterface);

        //            // Store the device name in a String.

        //            String DeviceNameString = new String(devBroadcastDeviceInterface.dbcc_name, 0, stringSize);

        //            // Compare the name of the newly attached device with the name of the device 
        //            // the application is accessing (mydevicePathName).
        //            // Set ignorecase True.

        //            if ((String.Compare(DeviceNameString, mydevicePathName, true) == 0))
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    return false;
        //}

        ///  <summary>
        ///  Use SetupDi API functions to retrieve the device path name of an
        ///  attached device that belongs to a device interface class.
        ///  </summary>
        ///  
        ///  <param name="myGuid"> an interface class GUID. </param>
        ///  <param name="devicePathName"> a pointer to the device path name 
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

                        // Skip over cbsize (4 bytes) to get the address of the devicePathName.

                        IntPtr pDevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

                        // Get the String containing the devicePathName.

                        devicePathName[memberIndex] = Marshal.PtrToStringAuto(pDevicePathName);


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
                if (detailDataBuffer != IntPtr.Zero)
                {
                    // Free the memory allocated previously by AllocHGlobal.

                    Marshal.FreeHGlobal(detailDataBuffer);
                }
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


        ///  <summary>
        ///  Requests to receive a notification when a device is attached or removed.
        ///  </summary>
        ///  
        ///  <param name="devicePathName"> handle to a device. </param>
        ///  <param name="formHandle"> handle to the window that will receive device events. </param>
        ///  <param name="classGuid"> device interface GUID. </param>
        ///  <param name="deviceNotificationHandle"> returned device notification handle. </param>
        ///  
        ///  <returns>
        ///  True on success.
        ///  </returns>
        ///  
        internal Boolean RegisterForDeviceNotifications(String devicePathName, IntPtr formHandle, Guid classGuid, ref IntPtr deviceNotificationHandle)
        {
            // A DEV_BROADCAST_DEVICEINTERFACE header holds information about the request.

            DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
            IntPtr devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;
            Int32 size = 0;

            try
            {
                // Set the parameters in the DEV_BROADCAST_DEVICEINTERFACE structure.

                // Set the size.

                size = Marshal.SizeOf(devBroadcastDeviceInterface);
                devBroadcastDeviceInterface.dbcc_size = size;

                // Request to receive notifications about a class of devices.

                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;

                devBroadcastDeviceInterface.dbcc_reserved = 0;

                // Specify the interface class to receive notifications about.

                devBroadcastDeviceInterface.dbcc_classguid = classGuid;

                // Allocate memory for the buffer that holds the DEV_BROADCAST_DEVICEINTERFACE structure.

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);

                // Copy the DEV_BROADCAST_DEVICEINTERFACE structure to the buffer.
                // Set fDeleteOld True to prevent memory leaks.

                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                // ***
                //  API function

                //  summary
                //  Request to receive notification messages when a device in an interface class
                //  is attached or removed.

                //  parameters 
                //  Handle to the window that will receive device events.
                //  Pointer to a DEV_BROADCAST_DEVICEINTERFACE to specify the type of 
                //  device to send notifications for.
                //  DEVICE_NOTIFY_WINDOW_HANDLE indicates the handle is a window handle.

                //  Returns
                //  Device notification handle or NULL on failure.
                // ***

                deviceNotificationHandle = RegisterDeviceNotification(formHandle, devBroadcastDeviceInterfaceBuffer, DEVICE_NOTIFY_WINDOW_HANDLE);

                // Marshal data from the unmanaged block devBroadcastDeviceInterfaceBuffer to
                // the managed object devBroadcastDeviceInterface

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);



                if ((deviceNotificationHandle.ToInt32() == IntPtr.Zero.ToInt32()))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
                {
                    // Free the memory allocated previously by AllocHGlobal.

                    Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
                }
            }
        }

        ///  <summary>
        ///  Requests to stop receiving notification messages when a device in an
        ///  interface class is attached or removed.
        ///  </summary>
        ///  
        ///  <param name="deviceNotificationHandle"> handle returned previously by
        ///  RegisterDeviceNotification. </param>

        internal void StopReceivingDeviceNotifications(IntPtr deviceNotificationHandle)
        {
            try
            {
                // ***
                //  API function

                //  summary
                //  Stop receiving notification messages.

                //  parameters
                //  Handle returned previously by RegisterDeviceNotification.  

                //  returns
                //  True on success.
                // ***

                //  Ignore failures.

                DeviceManagement.UnregisterDeviceNotification(deviceNotificationHandle);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    internal sealed partial class Hid
    {
        //  API declarations for HID communications.

        //  from hidpi.h
        //  Typedef enum defines a set of integer constants for HidP_Report_Type

        internal const Int16 HidP_Input = 0;
        internal const Int16 HidP_Output = 1;
        internal const Int16 HidP_Feature = 2;

        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDD_ATTRIBUTES
        {
            internal Int32 Size;
            internal UInt16 VendorID;
            internal UInt16 ProductID;
            internal UInt16 VersionNumber;
        }

        internal struct HIDP_CAPS
        {
            internal Int16 Usage;
            internal Int16 UsagePage;
            internal Int16 InputReportByteLength;
            internal Int16 OutputReportByteLength;
            internal Int16 FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            internal Int16[] Reserved;
            internal Int16 NumberLinkCollectionNodes;
            internal Int16 NumberInputButtonCaps;
            internal Int16 NumberInputValueCaps;
            internal Int16 NumberInputDataIndices;
            internal Int16 NumberOutputButtonCaps;
            internal Int16 NumberOutputValueCaps;
            internal Int16 NumberOutputDataIndices;
            internal Int16 NumberFeatureButtonCaps;
            internal Int16 NumberFeatureValueCaps;
            internal Int16 NumberFeatureDataIndices;
        }

        //  If IsRange is false, UsageMin is the Usage and UsageMax is unused.
        //  If IsStringRange is false, StringMin is the String index and StringMax is unused.
        //  If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.

        internal struct HidP_Value_Caps
        {
            internal Int16 UsagePage;
            internal Byte ReportID;
            internal Int32 IsAlias;
            internal Int16 BitField;
            internal Int16 LinkCollection;
            internal Int16 LinkUsage;
            internal Int16 LinkUsagePage;
            internal Int32 IsRange;
            internal Int32 IsStringRange;
            internal Int32 IsDesignatorRange;
            internal Int32 IsAbsolute;
            internal Int32 HasNull;
            internal Byte Reserved;
            internal Int16 BitSize;
            internal Int16 ReportCount;
            internal Int16 Reserved2;
            internal Int16 Reserved3;
            internal Int16 Reserved4;
            internal Int16 Reserved5;
            internal Int16 Reserved6;
            internal Int32 LogicalMin;
            internal Int32 LogicalMax;
            internal Int32 PhysicalMin;
            internal Int32 PhysicalMax;
            internal Int16 UsageMin;
            internal Int16 UsageMax;
            internal Int16 StringMin;
            internal Int16 StringMax;
            internal Int16 DesignatorMin;
            internal Int16 DesignatorMax;
            internal Int16 DataIndexMin;
            internal Int16 DataIndexMax;
        }

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_FlushQueue(SafeFileHandle HidDeviceObject);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_FreePreparsedData(IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetFeature(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetInputReport(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern void HidD_GetHidGuid(ref System.Guid HidGuid);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetNumInputBuffers(SafeFileHandle HidDeviceObject, ref Int32 NumberBuffers);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_SetFeature(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_SetNumInputBuffers(SafeFileHandle HidDeviceObject, Int32 NumberBuffers);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_SetOutputReport(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Int32 HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Int32 HidP_GetValueCaps(Int32 ReportType, Byte[] ValueCaps, ref Int32 ValueCapsLength, IntPtr PreparsedData);
    }   

    public class DelcomHID
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HidTxPacketStruct
        {
            public Byte MajorCmd;
            public Byte MinorCmd;
            public Byte LSBData;
            public Byte MSBData;
            public Byte HidData0;
            public Byte HidData1;
            public Byte HidData2;
            public Byte HidData3;
            public Byte ExtData0;
            public Byte ExtData1;
            public Byte ExtData2;
            public Byte ExtData3;
            public Byte ExtData4;
            public Byte ExtData5;
            public Byte ExtData6;
            public Byte ExtData7;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HidRxPacketStruct
        {
            public Byte Data0;
            public Byte Data1;
            public Byte Data2;
            public Byte Data3;
            public Byte Data4;
            public Byte Data5;
            public Byte Data6;
            public Byte Data7;


            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            //public byte[] Data;      //Data 1 .. 8
        }





        // Class variables
        private SafeFileHandle myDeviceHandle;
        private Boolean myDeviceDetected;
        private String myDevicePathName;
        private DeviceManagement MyDeviceManagement = new DeviceManagement();
        private Hid MyHid = new Hid();
        private const String MODULE_NAME = "Delcom HID USB CS";
        private UInt32 MatchingDevicesFound = 0;
        private HidTxPacketStruct myTxHidPacket;
        private HidRxPacketStruct myRxHidPacket;



        /// <summary>
        /// Initializes the class
        /// </summary>
        public DelcomHID()
        {
            //System.Windows.Forms.MessageBox.Show("Initializing DelcomHID");

        }


        /// <summary>
        /// Writes the ports values
        /// returns zero on sucess, else non-zero erro
        /// </summary>
        public UInt32 WritePorts(UInt32 Port0, UInt32 Port1)
        {

            try
            {
                myTxHidPacket.MajorCmd = 101;
                myTxHidPacket.MinorCmd = 10;
                myTxHidPacket.LSBData = Convert.ToByte(Port0);
                myTxHidPacket.MSBData = Convert.ToByte(Port1);
                if (Hid.HidD_SetFeature(myDeviceHandle, StructureToByteArray(myTxHidPacket), 8) == false) return (1);
                else return (0);
            }

            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                //throw;
                return (2);
            }
        }



        /// <summary>
        /// Reads the currenly value at port zero
        /// returns zero on sucess, else non-zero erro
        /// </summary>
        public UInt32 ReadPort0(ref UInt32 Port0)
        {

            try
            {
                Byte[] Ans = new Byte[16];
                Ans[0] = 100; // read ports command
                if (Hid.HidD_GetFeature(myDeviceHandle, Ans, 8) == false) return (1);
                Port0 = Convert.ToUInt32(Ans[0]);
                return (0);

            }

            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                //throw;
                return (2);
            }
        }


        /// <summary>
        /// Reads the currenly value at both ports
        /// returns zero on sucess, else non-zero erro
        /// </summary>
        public UInt32 ReadPorts(ref UInt32 Port0, ref UInt32 Port1)
        {

            try
            {
                Byte[] Ans = new Byte[16];
                Ans[0] = 100;
                if (Hid.HidD_GetFeature(myDeviceHandle, Ans, 8) == false) return (1);
                Port0 = Convert.ToUInt32(Ans[0]);
                Port1 = Convert.ToUInt32(Ans[1]);
                return (0);

            }

            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                //throw;
                return (2);
            }
        }

        /// <summary>
        /// Closed the USB HID devicde
        /// </summary>
        public UInt32 Close()
        {
            try
            {
                if (!myDeviceHandle.IsClosed)
                {
                    myDeviceHandle.Close();
                }
            }
            catch (Exception ex)
            {
                //throw;
                return (2);
            }
            return (0);
        }



        ///  <summary>
        /// Opens the first matching device found
        /// Return zero on success,
        /// otherwise non-zero error
        ///  </summary>
        public UInt32 Open()
        {
            return (OpenNthDevice(1));
        }

        ///  <summary>
        /// Opens the Nth matching device found
        /// Return zero on success,
        /// otherwise non-zero error
        ///  </summary>
        public UInt32 OpenNthDevice(UInt32 NthDevice)
        {
            if (myDeviceHandle != null) Close();    // if the device is already open, then close it first.

            if (!FindTheHid(NthDevice)) return (1);
            myDeviceHandle = FileIO.CreateFile(myDevicePathName, FileIO.GENERIC_READ | FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);
            if (myDeviceHandle.IsInvalid) return (2);
            return (0);  // device found
        }


        // Get the device name of the current device
        public string GetDeviceName()
        {
            return (myDevicePathName);
        }

        // Returns a count of the matching device on the current system
        public UInt32 GetDevicesCount()
        {
            FindTheHid(0);
            return (MatchingDevicesFound);
        }




        ///  <summary>
        ///  Uses a series of API calls to locate a HID-class device
        ///  by its Vendor ID, Product ID and by the Nth number on the list.
        ///  NthDevice: 0=none, used to determine how many matching device are currently
        ///  installed on the system. 1=Find the first matching device, 2=the second matching device,
        ///  and so on...
        ///  </summary>
        ///          
        ///  <returns>
        ///   True if the device is detected, False if not detected.
        ///  </returns>
        private Boolean FindTheHid(UInt32 NthDevice)
        {
            Boolean deviceFound = false;
            String[] devicePathName = new String[128];
            SafeFileHandle hidHandle;
            Guid hidGuid = Guid.Empty;
            Int32 memberIndex = 0;
            UInt16 myProductID = 0xB080;
            UInt16 myVendorID = 0x0FC5;
            Boolean success = false;
            UInt32 MatchingDevices = 0;

            try
            {
                myDeviceDetected = false;
                Hid.HidD_GetHidGuid(ref hidGuid);       //Retrieves the interface class GUID for the HID class.

                //  Fill an array with the device path names of all attached HIDs.
                deviceFound = MyDeviceManagement.FindDeviceFromGuid(hidGuid, ref devicePathName);

                //  If there is at least one HID, attempt to read the Vendor ID and Product ID
                //  of each device until there is a match or all devices have been examined.
                if (deviceFound)
                {
                    memberIndex = 0;
                    do
                    {
                        //  Open the device
                        hidHandle = FileIO.CreateFile(devicePathName[memberIndex], FileIO.GENERIC_READ | FileIO.GENERIC_WRITE, FileIO.FILE_SHARE_READ | FileIO.FILE_SHARE_WRITE, IntPtr.Zero, FileIO.OPEN_EXISTING, 0, 0);

                        if (!hidHandle.IsInvalid)
                        {   //  Device openned, now find out if it's the device we want.
                            MyHid.DeviceAttributes.Size = Marshal.SizeOf(MyHid.DeviceAttributes);
                            //  Retrieves a HIDD_ATTRIBUTES structure containing the Vendor ID, 
                            //  Product ID, and Product Version Number for a device.
                            success = Hid.HidD_GetAttributes(hidHandle, ref MyHid.DeviceAttributes);
                            if (success)
                            {
                                //  Find out if the device matches the one we're looking for.
                                if ((MyHid.DeviceAttributes.VendorID == myVendorID) & (MyHid.DeviceAttributes.ProductID == myProductID))
                                {
                                    MatchingDevices++;
                                    myDeviceDetected = true;
                                }

                                if (myDeviceDetected && (MatchingDevices == NthDevice))
                                {
                                    // Device found
                                    //  Save the DevicePathName
                                    myDevicePathName = devicePathName[memberIndex];
                                    hidHandle.Close();
                                }
                                else
                                {
                                    //  It's not a match, so close the handle. try the next one
                                    myDeviceDetected = false;
                                    hidHandle.Close();
                                }
                            }
                            else
                            {
                                //  There was a problem in retrieving the information.
                                myDeviceDetected = false;
                                hidHandle.Close();
                            }
                        }

                        //  Keep looking until we find the device or there are no devices left to examine.
                        memberIndex = memberIndex + 1;
                    }
                    while (!((myDeviceDetected | (memberIndex == devicePathName.Length))));
                }

                MatchingDevicesFound = MatchingDevices; // save the device found count

                return myDeviceDetected;
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
            }
        }




        ///  <summary>
        ///  Provides a central mechanism for exception handling.
        ///  Displays a message box that describes the exception.
        ///  </summary>
        ///  
        ///  <param name="moduleName"> the module where the exception occurred. </param>
        ///  <param name="e"> the exception </param>
        public static void DisplayException(String moduleName, Exception e)
        {
            String message = null;
            String caption = null;
            //  Create an error message.
            message = "Exception: " + e.Message + "\r\n" + "Module: " + moduleName + "\r\n" + "Method: " + e.TargetSite.Name;
            caption = "Unexpected Exception";
            //System.Windows.Forms.MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.OK);
            //throw;
        }



        // Converts a Structure to byte[]
        static byte[] StructureToByteArray(object obj)
        {
            int len = Marshal.SizeOf(obj);
            byte[] arr = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;

        }

        // Converts a byte[] to Structure
        static void ByteArrayToStructure(byte[] bytearray, ref object obj)
        {
            int len = Marshal.SizeOf(obj);
            IntPtr i = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, 0, i, len);
            obj = Marshal.PtrToStructure(i, obj.GetType());
            Marshal.FreeHGlobal(i);

        }


    }



    internal sealed partial class Hid
    {
        //  Used in error messages.

        private const String MODULE_NAME = "Hid";

        internal HIDP_CAPS Capabilities;
        internal HIDD_ATTRIBUTES DeviceAttributes;

        //  For viewing results of API calls in debug.write statements:

        //internal static Debugging MyDebugging = new Debugging();

        ///  <summary>
        ///  For reports the device sends to the host.
        ///  </summary>

        internal abstract class ReportIn
        {
            ///  <summary>
            ///  Each class that handles reading reports defines a Read method for reading 
            ///  a type of report. Read is declared as a Sub rather
            ///  than as a Function because asynchronous reads use a callback method 
            ///  that can access parameters passed by ByRef but not Function return values.
            ///  </summary>

            internal abstract void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] readBuffer, ref Boolean success);
        }

        ///  <summary>
        ///  For reading Feature reports.
        ///  </summary>

        internal class InFeatureReport : ReportIn
        {
            ///  <summary>
            ///  reads a Feature report from the device.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>
            ///  <param name="myDeviceDetected"> tells whether the device is currently attached.</param>
            ///  <param name="inFeatureReportBuffer"> contains the requested report.</param>
            ///  <param name="success"> read success</param>

            internal override void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] inFeatureReportBuffer, ref Boolean success)
            {
                try
                {
                    //  ***
                    //  API function: HidD_GetFeature
                    //  Attempts to read a Feature report from the device.

                    //  Requires:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 

                    //  Returns: true on success, false on failure.
                    //  ***                    

                    success = HidD_GetFeature(hidHandle, inFeatureReportBuffer, inFeatureReportBuffer.Length);

                    Debug.Print("HidD_GetFeature success = " + success);
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }
        }

        ///  <summary>
        ///  For reading Input reports via control transfers
        ///  </summary>

        internal class InputReportViaControlTransfer : ReportIn
        {
            ///  <summary>
            ///  reads an Input report from the device using a control transfer.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>
            ///  <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
            ///  <param name="inputReportBuffer"> contains the requested report. </param>
            ///  <param name="success"> read success </param>

            internal override void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] inputReportBuffer, ref Boolean success)
            {
                try
                {
                    //  ***
                    //  API function: HidD_GetInputReport

                    //  Purpose: Attempts to read an Input report from the device using a control transfer.
                    //  Supported under Windows XP and later only.

                    //  Requires:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 

                    //  Returns: true on success, false on failure.
                    //  ***

                    success = HidD_GetInputReport(hidHandle, inputReportBuffer, inputReportBuffer.Length + 1);

                    Debug.Print("HidD_GetInputReport success = " + success);
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }
        }

        ///  <summary>
        ///  For reading Input reports.
        ///  </summary>

        internal class InputReportViaInterruptTransfer : ReportIn
        {
            ///  <summary>
            ///  closes open handles to a device.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>

            internal void CancelTransfer(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, IntPtr eventObject)
            {
                try
                {
                    //  ***
                    //  API function: CancelIo

                    //  Purpose: Cancels a call to ReadFile

                    //  Accepts: the device handle.

                    //  Returns: True on success, False on failure.
                    //  ***

                    FileIO.CancelIo(readHandle);

                    Debug.WriteLine("************ReadFile error*************");
                    String functionName = "CancelIo";
                    //Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName));
                    Debug.WriteLine("");

                    //  The failure may have been because the device was removed,
                    //  so close any open handles and
                    //  set myDeviceDetected=False to cause the application to
                    //  look for the device on the next attempt.

                    if ((!(hidHandle.IsInvalid)))
                    {
                        hidHandle.Close();
                    }

                    if ((!(readHandle.IsInvalid)))
                    {
                        readHandle.Close();
                    }

                    if ((!(writeHandle.IsInvalid)))
                    {
                        writeHandle.Close();
                    }
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }

            ///  <summary>
            ///  Creates an event object for the overlapped structure used with ReadFile. 
            ///  </summary>
            ///  
            ///  <param name="hidOverlapped"> the overlapped structure </param>
            ///  <param name="eventObject"> the event object </param>

            internal void PrepareForOverlappedTransfer(ref NativeOverlapped hidOverlapped, ref IntPtr eventObject)
            {
                try
                {
                    //  ***
                    //  API function: CreateEvent

                    //  Purpose: Creates an event object for the overlapped structure used with ReadFile.

                    //  Accepts:
                    //  A security attributes structure or IntPtr.Zero.
                    //  Manual Reset = False (The system automatically resets the state to nonsignaled 
                    //  after a waiting thread has been released.)
                    //  Initial state = False (not signaled)
                    //  An event object name (optional)

                    //  Returns: a handle to the event object
                    //  ***

                    eventObject = FileIO.CreateEvent(IntPtr.Zero, false, false, "");

                    //  Set the members of the overlapped structure.

                    hidOverlapped.OffsetLow = 0;
                    hidOverlapped.OffsetHigh = 0;
                    hidOverlapped.EventHandle = eventObject;
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }

            ///  <summary>
            ///  reads an Input report from the device using interrupt transfers.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>
            ///  <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
            ///  <param name="inputReportBuffer"> contains the requested report. </param>
            ///  <param name="success"> read success </param>

            internal override void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] inputReportBuffer, ref Boolean success)
            {
                IntPtr eventObject = IntPtr.Zero;
                NativeOverlapped HidOverlapped = new NativeOverlapped();
                IntPtr nonManagedBuffer = IntPtr.Zero;
                IntPtr nonManagedOverlapped = IntPtr.Zero;
                Int32 numberOfBytesRead = 0;
                Int32 result = 0;

                try
                {
                    //  Set up the overlapped structure for ReadFile.

                    PrepareForOverlappedTransfer(ref HidOverlapped, ref eventObject);

                    // Allocate memory for the input buffer and overlapped structure. 

                    nonManagedBuffer = Marshal.AllocHGlobal(inputReportBuffer.Length);
                    nonManagedOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(HidOverlapped));
                    Marshal.StructureToPtr(HidOverlapped, nonManagedOverlapped, false);

                    //  ***
                    //  API function: ReadFile
                    //  Purpose: Attempts to read an Input report from the device.

                    //  Accepts:
                    //  A device handle returned by CreateFile
                    //  (for overlapped I/O, CreateFile must have been called with FILE_FLAG_OVERLAPPED),
                    //  A pointer to a buffer for storing the report.
                    //  The Input report length in bytes returned by HidP_GetCaps,
                    //  A pointer to a variable that will hold the number of bytes read. 
                    //  An overlapped structure whose hEvent member is set to an event object.

                    //  Returns: the report in ReadBuffer.

                    //  The overlapped call returns immediately, even if the data hasn't been received yet.

                    //  To read multiple reports with one ReadFile, increase the size of ReadBuffer
                    //  and use NumberOfBytesRead to determine how many reports were returned.
                    //  Use a larger buffer if the application can't keep up with reading each report
                    //  individually. 
                    //  ***                    

                    success = FileIO.ReadFile(readHandle, nonManagedBuffer, inputReportBuffer.Length, ref numberOfBytesRead, nonManagedOverlapped);

                    if (!success)
                    {
                        Debug.WriteLine("waiting for ReadFile");

                        //  API function: WaitForSingleObject

                        //  Purpose: waits for at least one report or a timeout.
                        //  Used with overlapped ReadFile.

                        //  Accepts:
                        //  An event object created with CreateEvent
                        //  A timeout value in milliseconds.

                        //  Returns: A result code.

                        result = FileIO.WaitForSingleObject(eventObject, 3000);

                        //  Find out if ReadFile completed or timeout.

                        switch (result)
                        {
                            case (System.Int32)FileIO.WAIT_OBJECT_0:

                                //  ReadFile has completed

                                success = true;
                                Debug.WriteLine("ReadFile completed successfully.");

                                // Get the number of bytes read.

                                //  API function: GetOverlappedResult

                                //  Purpose: gets the result of an overlapped operation.

                                //  Accepts:
                                //  A device handle returned by CreateFile.
                                //  A pointer to an overlapped structure.
                                //  A pointer to a variable to hold the number of bytes read.
                                //  False to return immediately.

                                //  Returns: non-zero on success and the number of bytes read.	

                                FileIO.GetOverlappedResult(readHandle, nonManagedOverlapped, ref numberOfBytesRead, false);

                                break;

                            case FileIO.WAIT_TIMEOUT:

                                //  Cancel the operation on timeout

                                CancelTransfer(hidHandle, readHandle, writeHandle, eventObject);
                                Debug.WriteLine("Readfile timeout");
                                success = false;
                                myDeviceDetected = false;
                                break;
                            default:

                                //  Cancel the operation on other error.

                                CancelTransfer(hidHandle, readHandle, writeHandle, eventObject);
                                Debug.WriteLine("Readfile undefined error");
                                success = false;
                                myDeviceDetected = false;
                                break;
                        }

                    }
                    if (success)
                    {
                        // A report was received.
                        // Copy the received data to inputReportBuffer for the application to use.

                        Marshal.Copy(nonManagedBuffer, inputReportBuffer, 0, numberOfBytesRead);
                    }
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }
        }

        ///  <summary>
        ///  For reports the host sends to the device.
        ///  </summary>

        internal abstract class ReportOut
        {
            ///  <summary>
            ///  Each class that handles writing reports defines a Write method for 
            ///  writing a type of report.
            ///  </summary>
            ///  
            ///  <param name="reportBuffer"> contains the report ID and report data. </param>
            ///   <param name="deviceHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>             

            internal abstract Boolean Write(Byte[] reportBuffer, SafeFileHandle deviceHandle);
        }

        ///  <summary>
        ///  For Feature reports the host sends to the device.
        ///  </summary>

        internal class OutFeatureReport : ReportOut
        {
            ///  <summary>
            ///  writes a Feature report to the device.
            ///  </summary>
            ///  
            ///  <param name="outFeatureReportBuffer"> contains the report ID and report data. </param>
            ///  <param name="hidHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>            

            internal override Boolean Write(Byte[] outFeatureReportBuffer, SafeFileHandle hidHandle)
            {
                Boolean success = false;

                try
                {
                    //  ***
                    //  API function: HidD_SetFeature

                    //  Purpose: Attempts to send a Feature report to the device.

                    //  Accepts:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 

                    //  Returns: true on success, false on failure.
                    //  ***

                    success = HidD_SetFeature(hidHandle, outFeatureReportBuffer, outFeatureReportBuffer.Length);

                    Debug.Print("HidD_SetFeature success = " + success);

                    return success;
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }
        }

        ///  <summary>
        ///  For writing Output reports via control transfers
        ///  </summary>

        internal class OutputReportViaControlTransfer : ReportOut
        {
            ///  <summary>
            ///  writes an Output report to the device using a control transfer.
            ///  </summary>
            ///  
            ///  <param name="outputReportBuffer"> contains the report ID and report data. </param>
            ///  <param name="hidHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>            

            internal override Boolean Write(Byte[] outputReportBuffer, SafeFileHandle hidHandle)
            {
                Boolean success = false;

                try
                {
                    //  ***
                    //  API function: HidD_SetOutputReport

                    //  Purpose: 
                    //  Attempts to send an Output report to the device using a control transfer.
                    //  Requires Windows XP or later.

                    //  Accepts:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 

                    //  Returns: true on success, false on failure.
                    //  ***                    

                    success = HidD_SetOutputReport(hidHandle, outputReportBuffer, outputReportBuffer.Length + 1);

                    Debug.Print("HidD_SetOutputReport success = " + success);

                    return success;
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }
        }

        ///  <summary>
        ///  For Output reports the host sends to the device.
        ///  Uses interrupt or control transfers depending on the device and OS.
        ///  </summary>

        internal class OutputReportViaInterruptTransfer : ReportOut
        {
            ///  <summary>
            ///  writes an Output report to the device.
            ///  </summary>
            ///  
            ///  <param name="outputReportBuffer"> contains the report ID and report data. </param>
            ///  <param name="writeHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>            

            internal override Boolean Write(Byte[] outputReportBuffer, SafeFileHandle writeHandle)
            {
                Int32 numberOfBytesWritten = 0;
                Boolean success = false;

                try
                {
                    //  The host will use an interrupt transfer if the the HID has an interrupt OUT
                    //  endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version). 
                    //  Otherwise the the host will use a control transfer.
                    //  The application doesn't have to know or care which type of transfer is used.

                    numberOfBytesWritten = 0;

                    //  ***
                    //  API function: WriteFile

                    //  Purpose: writes an Output report to the device.

                    //  Accepts:
                    //  A handle returned by CreateFile
                    //  An integer to hold the number of bytes written.

                    //  Returns: True on success, False on failure.
                    //  ***

                    success = FileIO.WriteFile(writeHandle, outputReportBuffer, outputReportBuffer.Length, ref numberOfBytesWritten, IntPtr.Zero);

                    Debug.Print("WriteFile success = " + success);

                    if (!((success)))
                    {

                        if ((!(writeHandle.IsInvalid)))
                        {
                            writeHandle.Close();
                        }
                    }
                    return success;
                }
                catch (Exception ex)
                {
                    DisplayException(MODULE_NAME, ex);
                    throw;
                }
            }
        }

        ///  <summary>
        ///  Remove any Input reports waiting in the buffer.
        ///  </summary>
        ///  
        ///  <param name="hidHandle"> a handle to a device.   </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>

        internal Boolean FlushQueue(SafeFileHandle hidHandle)
        {
            Boolean success = false;

            try
            {
                //  ***
                //  API function: HidD_FlushQueue

                //  Purpose: Removes any Input reports waiting in the buffer.

                //  Accepts: a handle to the device.

                //  Returns: True on success, False on failure.
                //  ***

                success = HidD_FlushQueue(hidHandle);

                return success;
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
            }
        }

        ///  <summary>
        ///  Retrieves a structure with information about a device's capabilities. 
        ///  </summary>
        ///  
        ///  <param name="hidHandle"> a handle to a device. </param>
        ///  
        ///  <returns>
        ///  An HIDP_CAPS structure.
        ///  </returns>

        internal HIDP_CAPS GetDeviceCapabilities(SafeFileHandle hidHandle)
        {
            IntPtr preparsedData = new System.IntPtr();
            Int32 result = 0;
            Boolean success = false;
            //Byte[] valueCaps = new Byte[ 1024 ]; // (the array size is a guess)

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
                    Debug.WriteLine("");
                    Debug.WriteLine("  Usage: " + Convert.ToString(Capabilities.Usage, 16));
                    Debug.WriteLine("  Usage Page: " + Convert.ToString(Capabilities.UsagePage, 16));
                    Debug.WriteLine("  Input Report Byte Length: " + Capabilities.InputReportByteLength);
                    Debug.WriteLine("  Output Report Byte Length: " + Capabilities.OutputReportByteLength);
                    Debug.WriteLine("  Feature Report Byte Length: " + Capabilities.FeatureReportByteLength);
                    Debug.WriteLine("  Number of Link Collection Nodes: " + Capabilities.NumberLinkCollectionNodes);
                    Debug.WriteLine("  Number of Input Button Caps: " + Capabilities.NumberInputButtonCaps);
                    Debug.WriteLine("  Number of Input Value Caps: " + Capabilities.NumberInputValueCaps);
                    Debug.WriteLine("  Number of Input Data Indices: " + Capabilities.NumberInputDataIndices);
                    Debug.WriteLine("  Number of Output Button Caps: " + Capabilities.NumberOutputButtonCaps);
                    Debug.WriteLine("  Number of Output Value Caps: " + Capabilities.NumberOutputValueCaps);
                    Debug.WriteLine("  Number of Output Data Indices: " + Capabilities.NumberOutputDataIndices);
                    Debug.WriteLine("  Number of Feature Button Caps: " + Capabilities.NumberFeatureButtonCaps);
                    Debug.WriteLine("  Number of Feature Value Caps: " + Capabilities.NumberFeatureValueCaps);
                    Debug.WriteLine("  Number of Feature Data Indices: " + Capabilities.NumberFeatureDataIndices);

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

                    //result = HidP_GetValueCaps(HidP_Input, ref valueCaps[0], ref Capabilities.NumberInputValueCaps, preparsedData); 

                    // (To use this data, copy the ValueCaps byte array into an array of structures.)                   

                }
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
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
                {
                    success = HidD_FreePreparsedData(preparsedData);
                }
            }

            return Capabilities;
        }

        ///  <summary>
        ///  Creates a 32-bit Usage from the Usage Page and Usage ID. 
        ///  Determines whether the Usage is a system mouse or keyboard.
        ///  Can be modified to detect other Usages.
        ///  </summary>
        ///  
        ///  <param name="MyCapabilities"> a HIDP_CAPS structure retrieved with HidP_GetCaps. </param>
        ///  
        ///  <returns>
        ///  A String describing the Usage.
        ///  </returns>

        internal String GetHidUsage(HIDP_CAPS MyCapabilities)
        {
            Int32 usage = 0;
            String usageDescription = "";

            try
            {
                //  Create32-bit Usage from Usage Page and Usage ID.

                usage = MyCapabilities.UsagePage * 256 + MyCapabilities.Usage;

                if (usage == Convert.ToInt32(0X102))
                {
                    usageDescription = "mouse";
                }

                if (usage == Convert.ToInt32(0X106))
                {
                    usageDescription = "keyboard";
                }
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
            }

            return usageDescription;
        }

        ///  <summary>
        ///  Retrieves the number of Input reports the host can store.
        ///  </summary>
        ///  
        ///  <param name="hidDeviceObject"> a handle to a device  </param>
        ///  <param name="numberOfInputBuffers"> an integer to hold the returned value. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>

        internal Boolean GetNumberOfInputBuffers(SafeFileHandle hidDeviceObject, ref Int32 numberOfInputBuffers)
        {
            Boolean success = false;

            try
            {
                if (!((IsWindows98Gold())))
                {
                    //  ***
                    //  API function: HidD_GetNumInputBuffers

                    //  Purpose: retrieves the number of Input reports the host can store.
                    //  Not supported by Windows 98 Gold.
                    //  If the buffer is full and another report arrives, the host drops the 
                    //  ldest report.

                    //  Accepts: a handle to a device and an integer to hold the number of buffers. 

                    //  Returns: True on success, False on failure.
                    //  ***

                    success = HidD_GetNumInputBuffers(hidDeviceObject, ref numberOfInputBuffers);
                }
                else
                {
                    //  Under Windows 98 Gold, the number of buffers is fixed at 2.

                    numberOfInputBuffers = 2;
                    success = true;
                }

                return success;
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
            }
        }

        ///  <summary>
        ///  sets the number of input reports the host will store.
        ///  Requires Windows XP or later.
        ///  </summary>
        ///  
        ///  <param name="hidDeviceObject"> a handle to the device.</param>
        ///  <param name="numberBuffers"> the requested number of input reports.  </param>
        ///  
        ///  <returns>
        ///  True on success. False on failure.
        ///  </returns>

        internal Boolean SetNumberOfInputBuffers(SafeFileHandle hidDeviceObject, Int32 numberBuffers)
        {
            try
            {
                if (!IsWindows98Gold())
                {
                    //  ***
                    //  API function: HidD_SetNumInputBuffers

                    //  Purpose: Sets the number of Input reports the host can store.
                    //  If the buffer is full and another report arrives, the host drops the 
                    //  oldest report.

                    //  Requires:
                    //  A handle to a HID
                    //  An integer to hold the number of buffers. 

                    //  Returns: true on success, false on failure.
                    //  ***

                    HidD_SetNumInputBuffers(hidDeviceObject, numberBuffers);
                    return true;
                }
                else
                {
                    //  Not supported under Windows 98 Gold.

                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
            }
        }

        ///  <summary>
        ///  Find out if the current operating system is Windows XP or later.
        ///  (Windows XP or later is required for HidD_GetInputReport and HidD_SetInputReport.)
        ///  </summary>

        internal Boolean IsWindowsXpOrLater()
        {
            try
            {
                OperatingSystem myEnvironment = Environment.OSVersion;

                //  Windows XP is version 5.1.

                System.Version versionXP = new System.Version(5, 1);

                if (myEnvironment.Version >= versionXP)
                {
                    Debug.Write("The OS is Windows XP or later.");
                    return true;
                }
                else
                {
                    Debug.Write("The OS is earlier than Windows XP.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
            }
        }

        ///  <summary>
        ///  Find out if the current operating system is Windows 98 Gold (original version).
        ///  Windows 98 Gold does not support the following:
        ///  Interrupt OUT transfers (WriteFile uses control transfers and Set_Report).
        ///  HidD_GetNumInputBuffers and HidD_SetNumInputBuffers
        ///  (Not yet tested on a Windows 98 Gold system.)
        ///  </summary>

        internal Boolean IsWindows98Gold()
        {
            Boolean result = false;
            try
            {
                OperatingSystem myEnvironment = Environment.OSVersion;

                //  Windows 98 Gold is version 4.10 with a build number less than 2183.

                System.Version version98SE = new System.Version(4, 10, 2183);

                if (myEnvironment.Version < version98SE)
                {
                    Debug.Write("The OS is Windows 98 Gold.");
                    result = true;
                }
                else
                {
                    Debug.Write("The OS is more recent than Windows 98 Gold.");
                    result = false;
                }
                return result;
            }
            catch (Exception ex)
            {
                DisplayException(MODULE_NAME, ex);
                throw;
            }
        }

        ///  <summary>
        ///  Provides a central mechanism for exception handling.
        ///  Displays a message box that describes the exception.
        ///  </summary>
        ///  
        ///  <param name="moduleName">  the module where the exception occurred. </param>
        ///  <param name="e"> the exception </param>

        internal static void DisplayException(String moduleName, Exception e)
        {
            String message = null;
            String caption = null;

            //  Create an error message.

            message = "Exception: " + e.Message + "\r\n" + "Module: " + moduleName + "\r\n" + "Method: " + e.TargetSite.Name;

            caption = "Unexpected Exception";

           // MessageBox.Show(message, caption, MessageBoxButtons.OK);
            Debug.Write(message);
        }
    }

    internal sealed class FileIO
    {
        internal const Int32 FILE_FLAG_OVERLAPPED = 0X40000000;
        internal const Int32 FILE_SHARE_READ = 1;
        internal const Int32 FILE_SHARE_WRITE = 2;
        internal const UInt32 GENERIC_READ = 0X80000000;
        internal const UInt32 GENERIC_WRITE = 0X40000000;
        internal const Int32 INVALID_HANDLE_VALUE = -1;
        internal const Int32 OPEN_EXISTING = 3;
        internal const Int32 WAIT_TIMEOUT = 0X102;
        internal const Int32 WAIT_OBJECT_0 = 0;

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal Int32 nLength;
            internal Int32 lpSecurityDescriptor;
            internal Int32 bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Int32 CancelIo(SafeFileHandle hFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateEvent(IntPtr SecurityAttributes, Boolean bManualReset, Boolean bInitialState, String lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean GetOverlappedResult(SafeFileHandle hFile, IntPtr lpOverlapped, ref Int32 lpNumberOfBytesTransferred, Boolean bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Int32 WaitForSingleObject(IntPtr hHandle, Int32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean WriteFile(SafeFileHandle hFile, Byte[] lpBuffer, Int32 nNumberOfBytesToWrite, ref Int32 lpNumberOfBytesWritten, IntPtr lpOverlapped);
    }     
}
