using System;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using BuildCommon;
using BuildStateServer.Configuration;

namespace BuildStateServer
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class BuildStatusChangeService : IBuildStatusChange, IDisposable
    {
        private byte _lastBuildStatus;
        private readonly LightRuleCollection _lightRules = LightConfigurationSection.Current.Rules;
        private const string QualityPrefix = "quality:";
        private const string StatusPrefix = "status:";
        

        public BuildStatusChangeService()
        {
            //Ensure all LEDS are off.
            SetLED(Delcom.GREENLED, false, false);
            SetLED(Delcom.BLUELED, false, false);
            SetLED(Delcom.REDLED, false, false);


            SetLED(Delcom.GREENLED, false, false);
            SetLED(Delcom.REDLED, false, false);
            SetLED(Delcom.BLUELED, true, true);
        }

        public void OnBuildFailed()
        {
            ApplyRuleByName(StatusPrefix + "Failed");
        }

        public void OnBuildStarted()
        {
            ApplyRuleByName(StatusPrefix + "Started");
        }

        public void OnBuildStopped()
        {
            SetLED(_lastBuildStatus, true, true,15,true);
        }

        public void OnBuildPartiallySucceeded()
        {
            ApplyRuleByName(StatusPrefix + "PartiallySucceeded");
        }

        public void OnBuildInProgress()
        {
            ApplyRuleByName(StatusPrefix + "InProgress");
        }

        public void OnBuildNotStarted()
        {
            ApplyRuleByName(StatusPrefix + "NotStarted");
        }

        public void OnBuildSuceeded()
        {
            ApplyRuleByName(StatusPrefix + "Succeeded");
        }

        public void OnBuildQualityChange(string quality)
        {
            var buildRuleName = QualityPrefix + quality;
            ApplyRuleByName(buildRuleName);
        }

        private void ApplyRuleByName(string buildRuleName)
        {
            Tracing.Server.TraceInformation("Find rule for {0}", buildRuleName);
            var rule =
                _lightRules.FirstOrDefault(
                    p => p.Name.Equals(buildRuleName, StringComparison.InvariantCultureIgnoreCase));
            if (rule != null)
            {
                ApplyRule(rule);
            }
        }

        private void ApplyRule(LightRuleElement rule)
        {
            Tracing.Server.TraceInformation("Found rule {0}", rule.Name);
            SetLED(Delcom.GREENLED, rule.Green, rule.FlashGreen);
            SetLED(Delcom.BLUELED, rule.Blue, rule.FlashBlue);
            SetLED(Delcom.REDLED, rule.Red, rule.FlashRed);
        }

        public void Dispose()
        {
            SetLED(Delcom.GREENLED, false, false);
            SetLED(Delcom.BLUELED, false, false);
            SetLED(Delcom.REDLED, false, false);

            uint device = GetDelcomDeviceHandle();
            Delcom.DelcomCloseDevice(device);

        }

        private uint GetDelcomDeviceHandle()
        {

            int totalNumberOfDevice = Delcom.DelcomGetDeviceCount(0);
            if (totalNumberOfDevice == 0)
            {
                throw new Exception("Device not found!\n");
            }
            var deviceName = new StringBuilder(Delcom.MAXDEVICENAMELEN);

            uint configuredDeviceNumber = UInt32.Parse(System.Configuration.ConfigurationManager.AppSettings["DelcomDeviceNumber"]);

            //Search for the first match USB device, For USB IO Chips use Delcom.USBIODS
            // With Generation 2 HID devices, you can pass a TypeId of 0 to open any Delcom device.
            int result = Delcom.DelcomGetNthDevice(Delcom.USBDELVI, configuredDeviceNumber, deviceName);

            if (result == 0)
            {
                // if not found, exit
                throw new Exception(String.Format("Could not open Device {0} not found!\n", configuredDeviceNumber));
            }

            uint hUSB = Delcom.DelcomOpenDevice(deviceName, 0); // open the device
            Tracing.Server.TraceInformation("Delcom device connected.");
            return hUSB;
        }

        private void SetLED(byte led, bool turnItOn, bool flashIt)
        {
            SetLED(led, turnItOn, flashIt, null, false);
        }

        private void SetLED(byte led, bool turnItOn, bool flashIt, int? flashDurationInSeconds)
        {
            SetLED(led, turnItOn, flashIt, flashDurationInSeconds, false);
        }

        private void SetLED(byte led, bool turnItOn, bool flashIt, int? flashDurationInSeconds,
                            bool turnOffAfterFlashing)
        {
            try
            {
                uint hUSB = GetDelcomDeviceHandle(); // open the device
                if (hUSB == 0) return;
                if (turnItOn)
                {
                    if (flashIt)
                    {
                        Delcom.DelcomLEDControl(hUSB, led, Delcom.LEDFLASH);
                        if (flashDurationInSeconds.HasValue)
                        {
                            Thread.Sleep(flashDurationInSeconds.Value*1000);
                            byte ledStatus = turnOffAfterFlashing ? Delcom.LEDOFF : Delcom.LEDON;
                            Delcom.DelcomLEDControl(hUSB, led, ledStatus);
                        }
                    }
                    else
                    {
                        Delcom.DelcomLEDControl(hUSB, led, Delcom.LEDON);
                    }
                }
                else
                {
                    Delcom.DelcomLEDControl(hUSB, led, Delcom.LEDOFF);
                }
                Delcom.DelcomCloseDevice(hUSB);
            }
            catch (Exception exception)
            {
                Tracing.Server.TraceError("Delcom device communication failed." + exception);
            }
            finally
            {
                _lastBuildStatus = led;
            }
        }
    }
}