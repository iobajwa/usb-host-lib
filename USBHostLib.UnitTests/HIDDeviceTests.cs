using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace USBHostLib.UnitTests
{
    [TestFixture]
    class when_sending_a_report_via_interrupt_transfer
    {
        HIDDevice _device;

        [SetUp]
        public void Setup()
        {
            _device = new HIDDevice(new HIDDeviceCapabilities(10, 20));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void _01_SHOULD_throw_ArgumentNullException_WHEN_buffer_is_passed_null()
        {
            _device.WriteReportViaInterruptTransfer(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Passed buffer length is greater than the maximum Output Report length that can be transferred to the device in a single transaction.")]
        public void _02_SHOULD_throw_ArgumentException_WHEN_passed_buffer_length_is_greater_than_OutputReportByteLength()
        {
            _device.WriteReportViaInterruptTransfer(new byte[_device.Capabilities.OutputReportByteLength]);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Device does not supports Ouput Reports.")]
        public void _03_SHOULD_throw_InvalidOperationException_WHEN_the_device_doesnot_support_output_reports()
        {
            _device = new HIDDevice(new HIDDeviceCapabilities(10, 0));

            _device.WriteReportViaInterruptTransfer(new byte[4]);
        }
    }
}
