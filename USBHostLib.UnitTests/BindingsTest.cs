using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject;

using USBHostLib;

using NUnit.Framework;

namespace USBHostLib.UnitTests
{
    class when_verifying_bindings__a_call_to_get
    {
        IKernel _kernel;

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel(new Bindings());
        }

        [Test]
        public void _01_IHIDFinder_SHOULD_return_the_same_HIDFinder_instance_each_time()
        {
            var instance1 = _kernel.Get<IHIDFinder>();
            var instance2 = _kernel.Get<IHIDFinder>();

            Assert.That(instance1, Is.EqualTo(instance2));
        }
    }
}
