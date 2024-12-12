using HeboTech.ATLib.Parsing;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsing
{
    public class AtChannelTests
    {
        private readonly Mock<IAtWriter> atWriter;
        private readonly DummyAtReader atReader;

        public AtChannelTests()
        {
            atWriter = new Mock<IAtWriter>();
            atReader = new();
        }

        [Fact]
        public void Default_command_timeout()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            Assert.Equal(TimeSpan.FromSeconds(5), dut.DefaultCommandTimeout);
        }

            [Fact]
        public void Final_success_response_ok()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("OK");
            AtResponse response = commandTask.Result;

            Assert.True(response.Success);
        }

        [Fact]
        public void Final_success_response_connect()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("CONNECT");
            AtResponse response = commandTask.Result;

            Assert.True(response.Success);
        }

        [Fact]
        public void Final_error_response_error()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("ERROR");
            AtResponse response = commandTask.Result;

            Assert.False(response.Success);
            Assert.Equal("ERROR", response.FinalResponse);
        }

        [Fact]
        public void Final_error_response_cms()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("+CMS ERROR:Line1");
            AtResponse response = commandTask.Result;

            Assert.False(response.Success);
            Assert.Equal("+CMS ERROR:Line1", response.FinalResponse);
        }

        [Fact]
        public void Final_error_response_cme()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("+CME ERROR:Line1");
            AtResponse response = commandTask.Result;

            Assert.False(response.Success);
            Assert.Equal("+CME ERROR:Line1", response.FinalResponse);
        }

        [Fact]
        public void Final_error_response_no_carrier()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("NO CARRIER");
            AtResponse response = commandTask.Result;

            Assert.False(response.Success);
            Assert.Equal("NO CARRIER", response.FinalResponse);
        }

        [Fact]
        public void Final_error_response_no_answer()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("NO ANSWER");
            AtResponse response = commandTask.Result;

            Assert.False(response.Success);
            Assert.Equal("NO ANSWER", response.FinalResponse);
        }

        [Fact]
        public void Final_error_response_no_dialtone()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("TestCommand");

            atReader.QueueLine("NO DIALTONE");
            AtResponse response = commandTask.Result;

            Assert.False(response.Success);
            Assert.Equal("NO DIALTONE", response.FinalResponse);
        }

        [Fact]
        public async Task Command_timeout_throws()
        {
            AtChannel dut = new(atReader, atWriter.Object);
            dut.DefaultCommandTimeout = TimeSpan.FromMilliseconds(50);
            dut.Open();

            await Assert.ThrowsAsync<TimeoutException>(() => dut.SendCommand("TestCommand"));
        }

        [Fact]
        public void Command_gets_response()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 25,99");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response = commandTask.Result;

            Assert.Single(response.Intermediates);
            Assert.Equal("+CSQ: 25,99", response.Intermediates.First());
            Assert.Equal("OK", response.FinalResponse);
        }

        [Fact]
        public void CommandIsWrittenToOutputStreamTestAsync()
        {
            atWriter
                .Setup(x => x.WriteLineAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask = dut.SendCommand("Test");

            atReader.QueueLine("OK");
            AtResponse response = commandTask.Result;

            atWriter.Verify(x => x.WriteLineAsync("Test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Two_singlelinecommands_get_responses()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask1 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 25,99");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response1 = commandTask1.Result;

            Task<AtResponse> commandTask2 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 50,80");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response2 = commandTask2.Result;

            Assert.Single(response1.Intermediates);
            Assert.Equal("+CSQ: 25,99", response1.Intermediates.First());
            Assert.Equal("OK", response1.FinalResponse);

            Assert.Single(response2.Intermediates);
            Assert.Equal("+CSQ: 50,80", response2.Intermediates.First());
            Assert.Equal("OK", response2.FinalResponse);
        }

        [Fact]
        public void Two_singlelinecommands_and_command_get_responses()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            Task<AtResponse> commandTask1 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 25,99");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response1 = commandTask1.Result;

            Task<AtResponse> commandTask2 = dut.SendCommand("ATA");

            atReader.QueueLine("OK");

            AtResponse response2 = commandTask2.Result;

            Task<AtResponse> commandTask3 = dut.SendSingleLineCommandAsync("AT+CSQ", "+CSQ");

            atReader.QueueLine("");
            atReader.QueueLine("+CSQ: 50,80");
            atReader.QueueLine("");
            atReader.QueueLine("OK");
            atReader.QueueLine("");

            AtResponse response3 = commandTask3.Result;

            Assert.True(response1.Success);
            Assert.Single(response1.Intermediates);
            Assert.Equal("+CSQ: 25,99", response1.Intermediates.First());
            Assert.Equal("OK", response1.FinalResponse);

            Assert.True(response2.Success);
            Assert.Empty(response2.Intermediates);
            Assert.Equal("OK", response2.FinalResponse);

            Assert.True(response3.Success);
            Assert.Single(response3.Intermediates);
            Assert.Equal("+CSQ: 50,80", response3.Intermediates.First());
            Assert.Equal("OK", response3.FinalResponse);
        }

        [Fact]
        public async void Unsolicited_fires_event_one_line()
        {
            UnsolicitedEventArgs unsolicitedEventArgs = null;
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            using AtChannel dut = new(atReader, atWriter.Object);
            dut.UnsolicitedEvent += (s, e) =>
            {
                unsolicitedEventArgs = e;
                semaphore.Release();
            };
            dut.Open();

            atReader.QueueLine("Unsolicited");

            await semaphore.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.NotNull(unsolicitedEventArgs);
            Assert.Equal("Unsolicited", unsolicitedEventArgs.Line1);
            Assert.Null(unsolicitedEventArgs.Line2);
        }

        [Fact]
        public async void Two_consecutive_unsolicited_events_fired()
        {
            UnsolicitedEventArgs unsolicitedEventArgs1 = null;
            UnsolicitedEventArgs unsolicitedEventArgs2 = null;
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            using AtChannel dut = new(atReader, atWriter.Object);
            dut.UnsolicitedEvent += (s, e) =>
            {
                if (unsolicitedEventArgs1 == null)
                    unsolicitedEventArgs1 = e;
                else if (unsolicitedEventArgs2 == null)
                    unsolicitedEventArgs2 = e;
                semaphore.Release();
            };
            dut.Open();

            atReader.QueueLine("Unsolicited1");
            atReader.QueueLine("Unsolicited2");

            await semaphore.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.NotNull(unsolicitedEventArgs1);
            Assert.Equal("Unsolicited1", unsolicitedEventArgs1.Line1);
            Assert.Null(unsolicitedEventArgs1.Line2);
            Assert.NotNull(unsolicitedEventArgs2);
            Assert.Equal("Unsolicited2", unsolicitedEventArgs2.Line1);
            Assert.Null(unsolicitedEventArgs2.Line2);
        }

        [Fact]
        public async void Unsolicited_cmt_event_fired()
        {
            UnsolicitedEventArgs unsolicitedEventArgs = null;
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            using AtChannel dut = new(atReader, atWriter.Object);
            dut.UnsolicitedEvent += (s, e) =>
            {
                unsolicitedEventArgs = e;
                semaphore.Release();
            };
            dut.Open();

            atReader.QueueLine("+CMT:Line1");
            atReader.QueueLine("Line2");

            await semaphore.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.NotNull(unsolicitedEventArgs);
            Assert.Equal("+CMT:Line1", unsolicitedEventArgs.Line1);
            Assert.Equal("Line2", unsolicitedEventArgs.Line2);
        }

        [Fact]
        public async void Unsolicited_cds_event_fired()
        {
            UnsolicitedEventArgs unsolicitedEventArgs = null;
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            using AtChannel dut = new(atReader, atWriter.Object);
            dut.UnsolicitedEvent += (s, e) =>
            {
                unsolicitedEventArgs = e;
                semaphore.Release();
            };
            dut.Open();

            atReader.QueueLine("+CDS:Line1");
            atReader.QueueLine("Line2");

            await semaphore.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.NotNull(unsolicitedEventArgs);
            Assert.Equal("+CDS:Line1", unsolicitedEventArgs.Line1);
            Assert.Equal("Line2", unsolicitedEventArgs.Line2);
        }

        [Fact]
        public async void Unsolicited_cbm_event_fired()
        {
            UnsolicitedEventArgs unsolicitedEventArgs = null;
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            using AtChannel dut = new(atReader, atWriter.Object);
            dut.UnsolicitedEvent += (s, e) =>
            {
                unsolicitedEventArgs = e;
                semaphore.Release();
            };
            dut.Open();

            atReader.QueueLine("+CBM:Line1");
            atReader.QueueLine("Line2");

            await semaphore.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.NotNull(unsolicitedEventArgs);
            Assert.Equal("+CBM:Line1", unsolicitedEventArgs.Line1);
            Assert.Equal("Line2", unsolicitedEventArgs.Line2);
        }

        [Fact]
        public async Task Command_succeeds_after_previous_command_times_out()
        {
            using AtChannel dut = new(atReader, atWriter.Object);
            dut.Open();

            await Assert.ThrowsAsync<TimeoutException>(() =>
                dut.SendCommand("Command1", TimeSpan.FromMilliseconds(1)));

            Task<AtResponse> commandTask2 = dut.SendCommand("Command2");

            atReader.QueueLine("OK");

            AtResponse response2 = commandTask2.GetAwaiter().GetResult();

            Assert.True(response2.Success);
            Assert.Equal("OK", response2.FinalResponse);
        }
    }
}
