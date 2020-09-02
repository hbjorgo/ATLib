using HeboTech.ATLib.Communication;
using HeboTech.ATLib.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Tests.Parsers
{
    [TestClass]
    public class AtLineReaderTests
    {
        [TestMethod]
        public void OkResultTest()
        {
            var commMock = new CommunicatorMock();
            commMock.AppendReturnValue("\r\nOK\r\n");
           
            AtLineReader dut = new AtLineReader(commMock);

            string result = dut.ReadLine();

            Assert.AreEqual("OK", result);
        }

        [TestMethod]
        public void SimReadyTest()
        {
            var commMock = new CommunicatorMock();
            commMock.AppendReturnValue("\r\n\r\n\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n+CP READY\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n+SPIC: 3,10,1,10\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n+CSQ: 16,99\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n+CBC: 0,100,4.232V\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\nManufacturer: SIMCOM INCORPORATED\r\nModel: SIMCOM_SIM5320E\r\nRevision: SIM5320E_V1.5\r\nIMEI: 012345678901234\r\n+GCAP: +CGSM,+DS,+ES\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n\r\n");

            AtLineReader dut = new AtLineReader(commMock);

            string result;

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CP READY", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+SPIC: 3,10,1,10", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CSQ: 16,99", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CBC: 0,100,4.232V", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("Manufacturer: SIMCOM INCORPORATED", result);

            result = dut.ReadLine();
            Assert.AreEqual("Model: SIMCOM_SIM5320E", result);

            result = dut.ReadLine();
            Assert.AreEqual("Revision: SIM5320E_V1.5", result);

            result = dut.ReadLine();
            Assert.AreEqual("IMEI: 012345678901234", result);

            result = dut.ReadLine();
            Assert.AreEqual("+GCAP: +CGSM,+DS,+ES", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);
        }

        [TestMethod]
        public void SimPinTest()
        {
            var commMock = new CommunicatorMock();
            commMock.AppendReturnValue("\r\n\r\nSTART\r\n\r\n+ST 25\r\n\r\n+ST 25\r\n\r\n+CP SIM PIN\r\n\r\n\r\nATE0\r\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n+CP SIM PIN\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n+SPIC: 3,10,1,10\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n+CP READY\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\nSMS DONE\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\nPB DONE\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n+CP READY\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n+CSQ: 16,99\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n+CBC: 0,100,4.232V\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\nManufacturer: SIMCOM INCORPORATED\r\nModel: SIMCOM_SIM5320E\r\nRevision: SIM5320E_V1.5\r\nIMEI: 012345678901234\r\n+GCAP: +CGSM,+DS,+ES\r\n\r\nOK\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n");

            AtLineReader dut = new AtLineReader(commMock);

            string result;

            result = dut.ReadLine();
            Assert.AreEqual("START", result);

            result = dut.ReadLine();
            Assert.AreEqual("+ST 25", result);

            result = dut.ReadLine();
            Assert.AreEqual("+ST 25", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CP SIM PIN", result);

            result = dut.ReadLine();
            Assert.AreEqual("ATE0", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CP SIM PIN", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+SPIC: 3,10,1,10", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CP READY", result);

            result = dut.ReadLine();
            Assert.AreEqual("SMS DONE", result);

            result = dut.ReadLine();
            Assert.AreEqual("PB DONE", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CP READY", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CSQ: 16,99", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("+CBC: 0,100,4.232V", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);

            result = dut.ReadLine();
            Assert.AreEqual("Manufacturer: SIMCOM INCORPORATED", result);

            result = dut.ReadLine();
            Assert.AreEqual("Model: SIMCOM_SIM5320E", result);

            result = dut.ReadLine();
            Assert.AreEqual("Revision: SIM5320E_V1.5", result);

            result = dut.ReadLine();
            Assert.AreEqual("IMEI: 012345678901234", result);

            result = dut.ReadLine();
            Assert.AreEqual("+GCAP: +CGSM,+DS,+ES", result);

            result = dut.ReadLine();
            Assert.AreEqual("OK", result);
        }

        private class CommunicatorMock : ICommunicator
        {
            private string returnValue;
            public void AppendReturnValue(string value)
            {
                returnValue += value;
            }

            public ValueTask<int> Read(char[] buffer, int offset, int count, CancellationToken cancellationToken = default)
            {
                var chars = returnValue.ToCharArray();
                int i;
                for (i = 0; i < count && i < chars.Length; i++)
                {
                    buffer[offset + i] = chars[i];
                }
                return new ValueTask<int>(i);
            }

            public ValueTask<bool> Write(string input, CancellationToken cancellationToken = default)
            {
                return new ValueTask<bool>(true);
            }

            public ValueTask<bool> Write(char[] input, int offset, int count, CancellationToken cancellationToken = default)
            {
                return new ValueTask<bool>(true);
            }
        }
    }
}
