using HeboTech.ATLib.Communication;
using HeboTech.ATLib.States;
using Stateless;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Modems
{
    public class GSM
    {
        private readonly ICommunicator comm;
        private StateMachine<State, Trigger> stateMachine;
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(10);

        public GSM(ICommunicator comm)
        {
            this.comm = comm;

            stateMachine = new StateMachine<State, Trigger>(State.Idle);

            stateMachine.Configure(State.Idle)
                .Permit(Trigger.Initialize, State.Initializing)
                .Permit(Trigger.SetCommandEcho, State.SettingCommandEcho);

            stateMachine.Configure(State.WaitingForOkOrErrorResponse)
                .OnEntryAsync(() => WaitForOkOrErrorResponseAsync())
                .Permit(Trigger.OkReceived, State.OkReceived)
                .Permit(Trigger.ErrorReceived, State.ErrorReceived);

            stateMachine.Configure(State.OkReceived)
                .OnEntry(() => OkReceived())
                .Permit(Trigger.Idle, State.Idle);

            stateMachine.Configure(State.ErrorReceived)
                .OnEntry(() => ErrorReceived())
                .Permit(Trigger.Idle, State.Idle);

            stateMachine.Configure(State.Initializing)
                .OnEntryAsync(() => ExecuteInitializeAsync())
                .Permit(Trigger.WaitForOkOrErrorResponse, State.WaitingForOkOrErrorResponse);

            stateMachine.Configure(State.SettingCommandEcho)
                .Permit(Trigger.OkReceived, State.Idle)
                .Permit(Trigger.ErrorReceived, State.Idle);
        }

        private void ErrorReceived()
        {
            stateMachine.Fire(Trigger.Idle);
        }

        private void OkReceived()
        {
            stateMachine.Fire(Trigger.Idle);
        }

        public ResponseFormat ResponseFormat { get; private set; }

        private async Task WaitForOkOrErrorResponseAsync()
        {
            DateTime entryTime = TimeService.UtcNow;

            while (true)
            {
                var message = await comm.ReadLineAsync();
                switch (ResponseFormat)
                {
                    case ResponseFormat.Numeric:
                        if (Regex.Match(message, "0\r\n").Success)
                        {
                            stateMachine.Fire(Trigger.OkReceived);
                            return;
                        }
                        else if (Regex.Match(message, "4\r\n").Success)
                        {
                            stateMachine.Fire(Trigger.ErrorReceived);
                            return;
                        }
                        break;
                    case ResponseFormat.Verbose:
                        if (Regex.Match(message, "\r\nOK\r\n").Success)
                        {
                            stateMachine.Fire(Trigger.OkReceived);
                            return;
                        }
                        else if (Regex.Match(message, "ERROR\r\n").Success)
                        {
                            stateMachine.Fire(Trigger.ErrorReceived);
                            return;
                        }
                        break;
                };

                if (TimeService.UtcNow > entryTime + timeout)
                    break;
            }

            stateMachine.Fire(Trigger.Timeout);
        }

        private async Task ExecuteInitializeAsync()
        {
            await comm.Write("AT\r");
            stateMachine.Fire(Trigger.WaitForOkOrErrorResponse);
        }

        public Guid Initialize()
        {
            throw new NotImplementedException();

            //var id = Guid.NewGuid();
            //stateMachine.Fire(Trigger.Initialize, id);
        }
    }

    public enum State
    {
        Idle,
        WaitingForOkOrErrorResponse,
        OkReceived,
        ErrorReceived,

        Initializing,
        SettingCommandEcho
    }

    public enum Trigger
    {
        WaitForOkOrErrorResponse,
        OkReceived,
        ErrorReceived,
        Timeout,
        Idle,

        Initialize,
        SetCommandEcho
    }
}
